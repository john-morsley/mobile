namespace Morsley.UK.Mobile.Persistence.Documents;

/// <summary>
/// CosmosDB-specific document model for mobile persistence
/// </summary>
public class SmsDocument
{
    private DateTime _createdAt = DateTime.UtcNow;

    [JsonProperty("id")]
    public string Id { get; set; } = Uuid.NewDatabaseFriendly(UUIDNext.Database.Other).ToString();

    [JsonProperty("partitionKey")]
    public string PartitionKey => Id;

    public string To { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt 
    { 
        get => _createdAt;
        set 
        {
            _createdAt = value;
        }
    }
}
