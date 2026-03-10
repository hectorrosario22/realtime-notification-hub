using NotificationHub.Domain.ValueObjects;

namespace NotificationHub.Domain.Tests.ValueObjects;

public class NotificationMetadataTests
{
    [Fact]
    public void Empty_ReturnsEmptyDictionary()
    {
        var metadata = NotificationMetadata.Empty();

        Assert.Empty(metadata.Data);
    }

    [Fact]
    public void Create_WithData_StoresDataCorrectly()
    {
        var data = new Dictionary<string, string> { ["key"] = "value" };

        var metadata = NotificationMetadata.Create(data);

        Assert.Single(metadata.Data);
        Assert.Equal("value", metadata.Data["key"]);
    }

    [Fact]
    public void Create_WithNull_ReturnsEmpty()
    {
        var metadata = NotificationMetadata.Create(null);

        Assert.Empty(metadata.Data);
    }

    [Fact]
    public void Equals_SameData_ReturnsTrue()
    {
        var metadata1 = NotificationMetadata.Create(new Dictionary<string, string> { ["a"] = "1" });
        var metadata2 = NotificationMetadata.Create(new Dictionary<string, string> { ["a"] = "1" });

        Assert.Equal(metadata1, metadata2);
    }

    [Fact]
    public void Equals_DifferentData_ReturnsFalse()
    {
        var metadata1 = NotificationMetadata.Create(new Dictionary<string, string> { ["a"] = "1" });
        var metadata2 = NotificationMetadata.Create(new Dictionary<string, string> { ["a"] = "2" });

        Assert.NotEqual(metadata1, metadata2);
    }
}
