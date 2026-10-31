using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Warehouse.Application.Auth;
using Warehouse.Application.Catalog;
using Warehouse.Application.Devices;
using Warehouse.Application.Identity;
using Warehouse.Application.Inventory;
using Warehouse.Application.Messaging;
using Warehouse.Application.Platform;
using Warehouse.Application.Pos;
using Warehouse.Application.Purchasing;
using Warehouse.Application.Reporting;
using Warehouse.Application.Sales;
using Warehouse.Application.Security;
using Warehouse.Application.Sync;
using Warehouse.Application.Tenancy;
using Warehouse.Infrastructure.Auth;
using Warehouse.Infrastructure.Catalog;
using Warehouse.Infrastructure.Devices;
using Warehouse.Infrastructure.Identity;
using Warehouse.Infrastructure.Inventory;
using Warehouse.Infrastructure.Messaging;
using Warehouse.Infrastructure.Platform;
using Warehouse.Infrastructure.Pos;
using Warehouse.Infrastructure.Purchasing;
using Warehouse.Infrastructure.Reporting;
using Warehouse.Infrastructure.Sales;
using Warehouse.Infrastructure.Sync;
using Warehouse.Infrastructure.Sync.Handlers;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddWarehousePersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .PostConfigure(options =>
            {
                if (string.IsNullOrWhiteSpace(options.ConnectionString))
                {
                    options.ConnectionString =
                        PostgresConnectionString.FirstNonEmpty(
                            configuration["WAREHOUSE_DB_CONNECTION"],
                            Environment.GetEnvironmentVariable("WAREHOUSE_DB_CONNECTION"))
                        ?? string.Empty;
                }

                if (!string.IsNullOrWhiteSpace(options.ConnectionString))
                {
                    options.ConnectionString = PostgresConnectionString.Normalize(options.ConnectionString);
                }
            });
        services.Configure<PlatformOptions>(configuration.GetSection(PlatformOptions.SectionName));

        services.AddScoped<AmbientTenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<AmbientTenantContext>());
        services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<AmbientTenantContext>());

        // Keep NullTenantContext available for design-time and tests that construct FixedTenantContext directly.

        services.AddDbContext<WarehouseDbContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            if (string.IsNullOrWhiteSpace(databaseOptions.ConnectionString))
            {
                throw new InvalidOperationException(
                    "Database:ConnectionString is not configured. Set it via environment or appsettings.");
            }

            options.UseNpgsql(databaseOptions.ConnectionString)
                .UseSnakeCaseNamingConvention();
        });

        return services;
    }

    public static IServiceCollection AddWarehouseAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        BindJwtOptions(services, configuration);

        services.AddSingleton<IPasswordHasher, AspNetPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IDeviceRegistrationService, DeviceRegistrationService>();
        services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();
        services.AddScoped<IRoleQueryService, RoleQueryService>();
        services.AddScoped<IPermissionQueryService, PermissionQueryService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IPlatformAuthenticationService, PlatformAuthenticationService>();
        services.AddScoped<IPlatformTenantService, PlatformTenantService>();
        services.AddScoped<IEntitlementService, EntitlementService>();
        services.AddScoped<IAuditWriter, AuditWriter>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IStockCountService, StockCountService>();
        services.AddScoped<IReportService, ReportService>();
        services.Configure<Warehouse.Application.Billing.StripeOptions>(
            configuration.GetSection(Warehouse.Application.Billing.StripeOptions.SectionName));
        services.AddScoped<Warehouse.Application.Billing.IBillingService, Warehouse.Infrastructure.Billing.StripeBillingService>();
        BindResala(services, configuration);
        BindSmtpEmail(services, configuration);
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IPurchasingService, PurchasingService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISalesService, SalesService>();
        services.AddScoped<IPosCheckoutService, PosCheckoutService>();
        services.AddScoped<ICashShiftService, CashShiftService>();
        services.AddScoped<IPosConflictService, PosConflictService>();
        services.AddScoped<IPosPricingService, PosPricingService>();
        services.AddScoped<IRestaurantPosService, RestaurantPosService>();
        services.AddScoped<ISelfCheckoutService, SelfCheckoutService>();
        BindSelfCheckoutOptions(services, configuration);
        services.AddScoped<IChangeLogWriter, ChangeLogWriter>();
        services.AddScoped<ISyncService, SyncService>();
        services.AddScoped<ISyncOperationHandler, ProductUpsertSyncHandler>();
        services.AddScoped<ISyncOperationHandler, ProductDeactivateSyncHandler>();
        services.AddScoped<ISyncOperationHandler, StockAdjustmentPostSyncHandler>();
        services.AddScoped<ISyncOperationHandler, TransferDispatchSyncHandler>();
        services.AddScoped<ISyncOperationHandler, TransferReceiveSyncHandler>();
        services.AddScoped<ISyncOperationHandler, TransferCancelSyncHandler>();
        services.AddScoped<ISyncOperationHandler, PosSaleFinalizeSyncHandler>();

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        ApplyJwtOptionsFromEnvironment(jwt, configuration);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(EnsureSigningKey(jwt.SigningKey))),
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "sub",
                    RoleClaimType = AuthClaimTypes.Role,
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static DbContextOptionsBuilder<WarehouseDbContext> ConfigureWarehouseNpgsql(
        this DbContextOptionsBuilder<WarehouseDbContext> optionsBuilder,
        string connectionString)
    {
        optionsBuilder.UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();
        return optionsBuilder;
    }

    private static void BindJwtOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .PostConfigure(options => ApplyJwtOptionsFromEnvironment(options, configuration));
    }

    private static void BindSelfCheckoutOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SelfCheckoutOptions>()
            .Bind(configuration.GetSection(SelfCheckoutOptions.SectionName))
            .PostConfigure(options => ApplySelfCheckoutOptionsFromEnvironment(options, configuration));
    }

    private static void ApplyJwtOptionsFromEnvironment(JwtOptions options, IConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(options.SigningKey))
        {
            options.SigningKey =
                configuration["JWT_SIGNING_KEY"]
                ?? Environment.GetEnvironmentVariable("JWT_SIGNING_KEY")
                ?? string.Empty;
        }

        if (TryReadPositiveInt(configuration, "JWT_ACCESS_TOKEN_MINUTES", out var accessMinutes))
        {
            options.AccessTokenMinutes = accessMinutes;
        }

        if (TryReadPositiveInt(configuration, "JWT_REFRESH_TOKEN_DAYS", out var refreshDays))
        {
            options.RefreshTokenDays = refreshDays;
        }
    }

    private static void ApplySelfCheckoutOptionsFromEnvironment(
        SelfCheckoutOptions options,
        IConfiguration configuration)
    {
        if (TryReadPositiveInt(configuration, "SELF_CHECKOUT_SESSION_HOURS", out var hours))
        {
            options.SessionHours = hours;
        }
    }

    private static bool TryReadPositiveInt(IConfiguration configuration, string key, out int value)
    {
        value = 0;
        var raw = configuration[key] ?? Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        if (!int.TryParse(raw.Trim(), out var parsed) || parsed <= 0)
        {
            return false;
        }

        value = parsed;
        return true;
    }

    private static void BindResala(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ResalaOptions>(configuration.GetSection(ResalaOptions.SectionName));
        services.PostConfigure<ResalaOptions>(options =>
        {
            var key = configuration["RESALA_API_KEY"]
                      ?? Environment.GetEnvironmentVariable("RESALA_API_KEY")
                      ?? configuration["Resala:ApiKey"];
            if (!string.IsNullOrWhiteSpace(key))
            {
                options.ApiKey = key.Trim();
            }

            var baseUrl = configuration["RESALA_BASE_URL"]
                          ?? Environment.GetEnvironmentVariable("RESALA_BASE_URL")
                          ?? configuration["Resala:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                options.BaseUrl = baseUrl.Trim().TrimEnd('/');
            }

            var sender = configuration["RESALA_SENDER"]
                         ?? Environment.GetEnvironmentVariable("RESALA_SENDER")
                         ?? configuration["Resala:DefaultSender"];
            if (!string.IsNullOrWhiteSpace(sender))
            {
                options.DefaultSender = sender.Trim();
            }

            var templateId = configuration["RESALA_RECEIPT_SMS_TEMPLATE_ID"]
                             ?? Environment.GetEnvironmentVariable("RESALA_RECEIPT_SMS_TEMPLATE_ID")
                             ?? configuration["Resala:ReceiptSmsTemplateId"];
            if (!string.IsNullOrWhiteSpace(templateId))
            {
                options.ReceiptSmsTemplateId = templateId.Trim();
            }
        });

        services.AddHttpClient<IResalaSmsClient, ResalaSmsClient>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<ResalaOptions>>().Value;
            var baseUrl = string.IsNullOrWhiteSpace(opts.BaseUrl)
                ? "https://dev.resala.ly"
                : opts.BaseUrl.Trim().TrimEnd('/');
            client.BaseAddress = new Uri(baseUrl + "/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
    }

    private static void BindSmtpEmail(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.PostConfigure<SmtpSettings>(options =>
        {
            ApplyIfPresent(configuration, "SMTP_SERVER", "SmtpSettings:Server", v => options.Server = v);
            ApplyIfPresent(configuration, "SMTP_SENDER_NAME", "SmtpSettings:SenderName", v => options.SenderName = v);
            ApplyIfPresent(configuration, "SMTP_SENDER_EMAIL", "SmtpSettings:SenderEmail", v => options.SenderEmail = v);
            ApplyIfPresent(configuration, "SMTP_USERNAME", "SmtpSettings:Username", v => options.Username = v);
            ApplyIfPresent(configuration, "SMTP_PASSWORD", "SmtpSettings:Password", v => options.Password = v);

            var portRaw = configuration["SMTP_PORT"]
                          ?? Environment.GetEnvironmentVariable("SMTP_PORT")
                          ?? configuration["SmtpSettings:Port"];
            if (int.TryParse(portRaw, out var port) && port > 0)
            {
                options.Port = port;
            }

            var sslRaw = configuration["SMTP_ENABLE_SSL"]
                         ?? Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL")
                         ?? configuration["SmtpSettings:EnableSsl"];
            if (bool.TryParse(sslRaw, out var ssl))
            {
                options.EnableSsl = ssl;
            }
        });

        services.AddTransient<IEmailService, EmailService>();
        services.AddScoped<IReceiptNotificationService, ReceiptNotificationService>();
    }

    private static void ApplyIfPresent(
        IConfiguration configuration,
        string envKey,
        string configKey,
        Action<string> apply)
    {
        var value = configuration[envKey]
                    ?? Environment.GetEnvironmentVariable(envKey)
                    ?? configuration[configKey];
        if (!string.IsNullOrWhiteSpace(value))
        {
            apply(value.Trim());
        }
    }

    private static string EnsureSigningKey(string signingKey)
    {
        if (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey must be configured and at least 32 characters. " +
                "Set Jwt__SigningKey or JWT_SIGNING_KEY. Testing hosts must supply a key via configuration.");
        }

        return signingKey;
    }
}
