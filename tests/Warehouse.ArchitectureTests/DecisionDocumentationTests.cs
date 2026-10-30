namespace Warehouse.ArchitectureTests;

public class DecisionDocumentationTests
{
    [Theory]
    [InlineData("docs/architecture.md", "MaxOfflineDays = 7")]
    [InlineData("docs/architecture.md", "OnHand")]
    [InlineData("docs/architecture.md", "Reserved")]
    [InlineData("docs/architecture.md", "Available = OnHand - Reserved")]
    [InlineData("docs/architecture.md", "IN_TRANSIT")]
    [InlineData("docs/architecture.md", "AllowWithPermission")]
    [InlineData("docs/architecture.md", "DocumentLifecycleStatus")]
    [InlineData("docs/architecture.md", "ChangeLog")]
    [InlineData("docs/architecture.md", "refresh_tokens")]
    [InlineData("docs/architecture.md", "IsSystem = true")]
    [InlineData("docs/offline-first.md", "MaxOfflineDays = 7")]
    [InlineData("docs/offline-first.md", "DefaultMaxOfflineDays")]
    [InlineData("docs/offline-first.md", "DeviceRevoked")]
    [InlineData("docs/offline-first.md", "TenantSuspended")]
    [InlineData("docs/synchronization.md", "ProcessedSyncOperation")]
    [InlineData("docs/synchronization.md", "ChangeVersion")]
    [InlineData("docs/synchronization.md", "Do not create this table in Phase 1")]
    [InlineData("docs/authentication.md", "TokenHash")]
    [InlineData("docs/authentication.md", "refresh_tokens")]
    [InlineData("docs/authentication.md", "implemented")]
    [InlineData("docs/authorization.md", "TENANT-OWNED")]
    [InlineData("docs/authorization.md", "IsSystem = true")]
    [InlineData("docs/authorization.md", "RequirePermission")]
    [InlineData("docs/database.md", "OnHand")]
    [InlineData("docs/database.md", "Reserved")]
    [InlineData("docs/database.md", "Available = OnHand - Reserved")]
    [InlineData("docs/business-rules.md", "Prevent")]
    [InlineData("docs/business-rules.md", "AllowWithPermission")]
    [InlineData("docs/business-rules.md", "MaxOfflineDays = 7")]
    [InlineData("docs/conflict-resolution.md", "IN_TRANSIT")]
    [InlineData("docs/conflict-resolution.md", "OnHand")]
    public void Decision_documents_record_required_statements(string relativePath, string requiredText)
    {
        var path = Path.Combine(RepoRoot.Locate(), relativePath);
        Assert.True(File.Exists(path), $"Missing document: {relativePath}");
        Assert.Contains(requiredText, File.ReadAllText(path), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Architecture_documents_default_offline_window_without_hard_coding_business_logic()
    {
        var path = Path.Combine(RepoRoot.Locate(), "docs/architecture.md");
        var content = File.ReadAllText(path);

        Assert.Contains("MaxOfflineDays = 7", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("DefaultMaxOfflineDays", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Do not scatter the literal", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("default unset", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Architecture_does_not_add_warn_negative_stock_policy()
    {
        var path = Path.Combine(RepoRoot.Locate(), "docs/architecture.md");
        var content = File.ReadAllText(path);
        Assert.Contains("Do not add `Warn`", content, StringComparison.OrdinalIgnoreCase);
    }
}
