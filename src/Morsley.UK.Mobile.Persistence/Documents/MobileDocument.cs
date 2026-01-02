namespace Morsley.UK.Mobile.Persistence.Documents;

public class SmsDocument
{
    private DateTime _created = DateTime.UtcNow;

    [JsonProperty("id")]
    public string Id { get; set; } = Uuid.NewDatabaseFriendly(UUIDNext.Database.Other).ToString();

    [JsonProperty("partitionKey")]
    public string PartitionKey => Id;

    public string To { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime Created 
    { 
        get => _created;
        set 
        {
            _created = value;
        }
    }
}