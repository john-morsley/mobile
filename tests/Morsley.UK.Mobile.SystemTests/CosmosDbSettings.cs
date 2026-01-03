namespace Morsley.UK.Mobile.API.SystemTests;

public class CosmosDbSettings
{
    public string Endpoint { get; set; } = string.Empty;

    public string DatabaseName { get; set; } = string.Empty;

    public string PartitionKey { get; set; } = string.Empty;

    public string SentSmsContainerName { get; set; } = string.Empty;

    public string ReceivedSmsContainerName { get; set; } = string.Empty;
}