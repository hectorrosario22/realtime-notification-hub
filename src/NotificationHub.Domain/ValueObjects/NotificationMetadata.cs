using NotificationHub.Domain.Common;

namespace NotificationHub.Domain.ValueObjects;

public sealed class NotificationMetadata : ValueObject
{
    private readonly Dictionary<string, string> _data;

    public IReadOnlyDictionary<string, string> Data => _data.AsReadOnly();

    private NotificationMetadata(Dictionary<string, string> data) => _data = data;

    public static NotificationMetadata Empty() => new(new Dictionary<string, string>());

    public static NotificationMetadata Create(Dictionary<string, string>? data) =>
        new(data is not null ? new Dictionary<string, string>(data) : []);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        foreach (var kvp in _data.OrderBy(x => x.Key))
        {
            yield return kvp.Key;
            yield return kvp.Value;
        }
    }
}
