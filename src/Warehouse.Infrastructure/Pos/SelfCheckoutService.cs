using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Platform;
using Warehouse.Application.Pos;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Catalog;
using Warehouse.Domain.Pos;
using Warehouse.Domain.Subscription;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.Infrastructure.Pos;

public sealed class SelfCheckoutService(
    WarehouseDbContext db,
    ITenantContext tenantContext,
    AmbientTenantContext ambientTenant,
    IEntitlementService entitlements,
    IOptions<SelfCheckoutOptions> selfCheckoutOptions) : ISelfCheckoutService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const decimal HighValueAuditThreshold = 500m;
    private const double GuestAuditRate = 0.20;
    private const double MemberAuditRate = 0.15;

    public async Task<AppResult<SelfCheckoutSessionResponse>> StartAsync(
        StartSelfCheckoutCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.TenantCode))
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.ValidationFailed, "Tenant code is required.");
        }

        var tenant = await db.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.Code == command.TenantCode.Trim().ToUpperInvariant() && t.IsActive && !t.IsSuspended,
                cancellationToken);
        if (tenant is null)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.ValidationFailed, "Tenant was not found or is inactive.");
        }

        ambientTenant.SetTenantScope(tenant.Id);
        if (!await entitlements.IsFeatureEnabledAsync(SubscriptionFeatures.Sales, cancellationToken))
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(
                ErrorCodes.Forbidden,
                "Sales / POS is not enabled for this tenant.");
        }

        var warehouse = command.WarehouseId is Guid wid
            ? await db.Warehouses.FirstOrDefaultAsync(w => w.Id == wid && w.IsActive, cancellationToken)
            : await db.Warehouses.OrderBy(w => w.Code).FirstOrDefaultAsync(w => w.IsActive, cancellationToken);
        if (warehouse is null)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.ValidationFailed, "No active warehouse available.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        var session = SelfCheckoutSession.Create(
            Guid.NewGuid(),
            tenant.Id,
            warehouse.Id,
            GenerateCode(),
            GenerateAccessToken(),
            command.IsGuest,
            tenant.CurrencyCode,
            utcNow,
            ResolveSessionLifetime());

        db.SelfCheckoutSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<SelfCheckoutSessionResponse>.Success(await MapAsync(session, cancellationToken));
    }

    public async Task<AppResult<SelfCheckoutSessionResponse>> GetByAccessTokenAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        var session = await FindByAccessTokenAsync(accessToken, cancellationToken);
        if (session is null)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.ValidationFailed, "Session not found.");
        }

        try
        {
            session.EnsureUsable(DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }

        return AppResult<SelfCheckoutSessionResponse>.Success(await MapAsync(session, cancellationToken));
    }

    public async Task<AppResult<SelfCheckoutSessionResponse>> AddLineByBarcodeAsync(
        string accessToken,
        string barcode,
        decimal quantity,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.ValidationFailed, "Barcode is required.");
        }

        if (quantity <= 0)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.ValidationFailed, "Quantity must be positive.");
        }

        var session = await FindByAccessTokenAsync(accessToken, cancellationToken);
        if (session is null)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.ValidationFailed, "Session not found.");
        }

        try
        {
            session.EnsureUsable(DateTimeOffset.UtcNow);
            if (session.Status != SelfCheckoutSessionStatus.Open)
            {
                return AppResult<SelfCheckoutSessionResponse>.Failure(
                    ErrorCodes.InvalidOperation,
                    "Cart can only be changed while the session is open.");
            }
        }
        catch (InvalidOperationException ex)
        {
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }

        var normalized = barcode.Trim();
        var link = await db.ProductBarcodes.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Barcode == normalized, cancellationToken);
        if (link is null)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.ValidationFailed, "Product barcode not found.");
        }

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == link.ProductId && p.IsActive, cancellationToken);
        if (product is null)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.ValidationFailed, "Product not found or inactive.");
        }

        var lines = await db.SelfCheckoutLines
            .Where(l => l.SessionId == session.Id)
            .ToListAsync(cancellationToken);
        var existing = lines.FirstOrDefault(l => l.ProductId == product.Id);
        if (existing is not null)
        {
            existing.SetQuantity(existing.Quantity + quantity);
        }
        else
        {
            db.SelfCheckoutLines.Add(SelfCheckoutLine.Create(
                Guid.NewGuid(),
                session.TenantId,
                session.Id,
                product.Id,
                product.Sku,
                product.Name,
                normalized,
                quantity,
                product.SalePrice));
        }

        await RecalculateAsync(session, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<SelfCheckoutSessionResponse>.Success(await MapAsync(session, cancellationToken));
    }

    public async Task<AppResult<SelfCheckoutSessionResponse>> SetLineQuantityAsync(
        string accessToken,
        Guid lineId,
        decimal quantity,
        CancellationToken cancellationToken)
    {
        var session = await FindByAccessTokenAsync(accessToken, cancellationToken);
        if (session is null)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.ValidationFailed, "Session not found.");
        }

        if (session.Status != SelfCheckoutSessionStatus.Open)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(
                ErrorCodes.InvalidOperation,
                "Cart can only be changed while the session is open.");
        }

        var line = await db.SelfCheckoutLines.FirstOrDefaultAsync(
            l => l.Id == lineId && l.SessionId == session.Id,
            cancellationToken);
        if (line is null)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.ValidationFailed, "Line not found.");
        }

        if (quantity <= 0)
        {
            db.SelfCheckoutLines.Remove(line);
        }
        else
        {
            line.SetQuantity(quantity);
        }

        await RecalculateAsync(session, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<SelfCheckoutSessionResponse>.Success(await MapAsync(session, cancellationToken));
    }

    public async Task<AppResult<SelfCheckoutSessionResponse>> RemoveLineAsync(
        string accessToken,
        Guid lineId,
        CancellationToken cancellationToken) =>
        await SetLineQuantityAsync(accessToken, lineId, 0, cancellationToken);

    public async Task<AppResult<SelfCheckoutTransferResponse>> TransferAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        var session = await FindByAccessTokenAsync(accessToken, cancellationToken);
        if (session is null)
        {
            return AppResult<SelfCheckoutTransferResponse>.Failure(ErrorCodes.ValidationFailed, "Session not found.");
        }

        try
        {
            session.EnsureUsable(DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<SelfCheckoutTransferResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }

        if (session.Status != SelfCheckoutSessionStatus.Open)
        {
            return AppResult<SelfCheckoutTransferResponse>.Failure(
                ErrorCodes.InvalidOperation,
                "Session already transferred.");
        }

        var lines = await db.SelfCheckoutLines.AsNoTracking()
            .Where(l => l.SessionId == session.Id)
            .ToListAsync(cancellationToken);
        if (lines.Count == 0)
        {
            return AppResult<SelfCheckoutTransferResponse>.Failure(ErrorCodes.ValidationFailed, "Cart is empty.");
        }

        await RecalculateAsync(session, cancellationToken);

        var barcodes = lines
            .Select(l => l.BarcodeSnapshot)
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var rate = session.IsGuest ? GuestAuditRate : MemberAuditRate;
        var auditRequired = session.GrandTotal >= HighValueAuditThreshold
            || barcodes.Count > 0 && Random.Shared.NextDouble() < rate;
        var auditCount = 0;
        string? targetsJson = null;
        if (auditRequired && barcodes.Count > 0)
        {
            auditCount = Math.Clamp(Math.Min(3, barcodes.Count), 1, barcodes.Count);
            var picked = barcodes.OrderBy(_ => Random.Shared.Next()).Take(auditCount).ToList();
            targetsJson = JsonSerializer.Serialize(picked, JsonOptions);
        }
        else
        {
            auditRequired = false;
        }

        session.MarkTransferred(auditRequired, auditCount, targetsJson, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(cancellationToken);

        return AppResult<SelfCheckoutTransferResponse>.Success(
            new SelfCheckoutTransferResponse(
                session.Id,
                session.Code,
                BuildQrPayload(session.Code),
                session.AuditRequired,
                session.AuditItemCount,
                session.Status.ToString(),
                session.GrandTotal,
                session.CurrencyCode));
    }

    public async Task<AppResult<SelfCheckoutSessionResponse>> ImportByCodeAsync(
        string codeOrQrPayload,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        var code = ExtractCode(codeOrQrPayload);
        if (string.IsNullOrWhiteSpace(code))
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.ValidationFailed, "Order code is required.");
        }

        var session = await db.SelfCheckoutSessions
            .FirstOrDefaultAsync(s => s.Code == code, cancellationToken);
        if (session is null)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.ValidationFailed, "Transfer order not found.");
        }

        try
        {
            session.EnsureUsable(DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }

        if (session.Status is SelfCheckoutSessionStatus.Open)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(
                ErrorCodes.InvalidOperation,
                "Customer has not finished transferring this cart yet.");
        }

        return AppResult<SelfCheckoutSessionResponse>.Success(await MapAsync(session, cancellationToken));
    }

    public async Task<AppResult<SelfCheckoutSessionResponse>> StartAuditAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        var session = await db.SelfCheckoutSessions.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
        if (session is null)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.ValidationFailed, "Session not found.");
        }

        if (!session.AuditRequired || session.AuditCompleted)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(
                ErrorCodes.InvalidOperation,
                "No audit is required for this order.");
        }

        try
        {
            session.StartAudit(DateTimeOffset.UtcNow);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }

        return AppResult<SelfCheckoutSessionResponse>.Success(await MapAsync(session, cancellationToken));
    }

    public async Task<AppResult<SelfCheckoutSessionResponse>> CompleteAuditAsync(
        Guid sessionId,
        IReadOnlyList<string> scannedBarcodes,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        var session = await db.SelfCheckoutSessions.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
        if (session is null)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.ValidationFailed, "Session not found.");
        }

        if (!session.AuditRequired)
        {
            session.CompleteAudit(DateTimeOffset.UtcNow);
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<SelfCheckoutSessionResponse>.Success(await MapAsync(session, cancellationToken));
        }

        var targets = ParseTargets(session.AuditTargetBarcodesJson);
        if (targets.Count == 0)
        {
            session.CompleteAudit(DateTimeOffset.UtcNow);
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<SelfCheckoutSessionResponse>.Success(await MapAsync(session, cancellationToken));
        }

        var scanned = (scannedBarcodes ?? Array.Empty<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = targets.Where(t => !scanned.Contains(t)).ToList();
        if (missing.Count > 0)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(
                ErrorCodes.ValidationFailed,
                $"Audit incomplete — rescan: {string.Join(", ", missing)}");
        }

        if (session.Status == SelfCheckoutSessionStatus.Transferred)
        {
            session.StartAudit(DateTimeOffset.UtcNow);
        }

        session.CompleteAudit(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<SelfCheckoutSessionResponse>.Success(await MapAsync(session, cancellationToken));
    }

    public async Task<AppResult<SelfCheckoutSessionResponse>> MarkClosedAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        var session = await db.SelfCheckoutSessions.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
        if (session is null)
        {
            return AppResult<SelfCheckoutSessionResponse>.Failure(ErrorCodes.ValidationFailed, "Session not found.");
        }

        session.Close(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<SelfCheckoutSessionResponse>.Success(await MapAsync(session, cancellationToken));
    }

    private async Task<SelfCheckoutSession?> FindByAccessTokenAsync(string accessToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        var session = await db.SelfCheckoutSessions.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.AccessToken == accessToken.Trim(), cancellationToken);
        if (session is null)
        {
            return null;
        }

        ambientTenant.SetTenantScope(session.TenantId);
        return session;
    }

    private async Task RecalculateAsync(SelfCheckoutSession session, CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants.FirstAsync(t => t.Id == session.TenantId, cancellationToken);
        var lines = await db.SelfCheckoutLines.Where(l => l.SessionId == session.Id).ToListAsync(cancellationToken);
        var productIds = lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, cancellationToken);

        decimal subtotal = 0m;
        decimal taxTotal = 0m;
        foreach (var line in lines)
        {
            subtotal += line.LineTotal;
            if (products.TryGetValue(line.ProductId, out var product))
            {
                taxTotal += ProductTaxCalculator.CalculateTaxAmount(tenant, product, line.LineTotal);
            }
        }

        session.RecalculateTotals(
            decimal.Round(subtotal, 2, MidpointRounding.AwayFromZero),
            decimal.Round(taxTotal, 2, MidpointRounding.AwayFromZero),
            DateTimeOffset.UtcNow);
    }

    private async Task<SelfCheckoutSessionResponse> MapAsync(
        SelfCheckoutSession session,
        CancellationToken cancellationToken)
    {
        var lines = await db.SelfCheckoutLines.AsNoTracking()
            .Where(l => l.SessionId == session.Id)
            .OrderBy(l => l.NameSnapshot)
            .Select(l => new SelfCheckoutLineResponse(
                l.Id,
                l.ProductId,
                l.SkuSnapshot,
                l.NameSnapshot,
                l.BarcodeSnapshot,
                l.Quantity,
                l.UnitPrice,
                l.LineTotal))
            .ToListAsync(cancellationToken);

        return new SelfCheckoutSessionResponse(
            session.Id,
            session.WarehouseId,
            session.Code,
            session.AccessToken,
            session.IsGuest,
            session.Status.ToString(),
            session.CurrencyCode,
            session.Subtotal,
            session.TaxTotal,
            session.GrandTotal,
            session.AuditRequired,
            session.AuditItemCount,
            ParseTargets(session.AuditTargetBarcodesJson),
            session.AuditCompleted,
            session.ExpiresAtUtc,
            BuildQrPayload(session.Code),
            lines);
    }

    private static IReadOnlyList<string> ParseTargets(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    internal static string BuildQrPayload(string code) => $"MTSC:{code}";

    internal static string ExtractCode(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var value = raw.Trim();
        const string prefix = "MTSC:";
        if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            value = value[prefix.Length..];
        }

        return value.Trim().ToUpperInvariant();
    }

    private static string GenerateCode()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        Span<char> chars = stackalloc char[8];
        for (var i = 0; i < chars.Length; i++)
        {
            chars[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
        }

        return new string(chars);
    }

    private static string GenerateAccessToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();

    private TimeSpan ResolveSessionLifetime()
    {
        var hours = selfCheckoutOptions.Value.SessionHours;
        if (hours <= 0)
        {
            hours = 3;
        }

        return TimeSpan.FromHours(hours);
    }
}
