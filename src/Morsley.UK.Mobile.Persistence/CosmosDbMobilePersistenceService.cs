namespace Morsley.UK.Mobile.Persistence;

public class CosmosDbMobilePersistenceService : ISmsPersistenceService
{
    private readonly Container _container;
    private readonly ILogger<CosmosDbMobilePersistenceService> _logger;

    public CosmosDbMobilePersistenceService(
        CosmosClient cosmosClient, 
        string databaseName,
        string containerName,
        ILogger<CosmosDbMobilePersistenceService> logger)
    {
        _logger = logger;               
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task<string> SaveSmsAsync(Common.Models.SmsMessage sms, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Saving SMS");
            
            var smsDocument = sms.ToDocument();

            var response = await _container.UpsertItemAsync(smsDocument, new PartitionKey(smsDocument.PartitionKey));
            
            _logger.LogInformation("Successfully saved SMS with ID: {SmsId}. Request charge: {RequestCharge}", sms.Id, response.RequestCharge);
            
            return response.Resource.Id;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to save SMS: Status: {Status}, Message: {Message}", ex.StatusCode, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error saving SMS");
            throw;
        }
    }    

    public async Task<Common.Models.SmsMessage?> GetSmsAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS with ID: {SmsId}", id);
            
            // Since we don't know the partition key for the email, we need to query across partitions
            var query = _container.GetItemQueryIterator<SmsDocument>(
                $"SELECT * " +
                $"  FROM c " +
                $" WHERE c.id = '{id}'");
            
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                var smsDocument = response.FirstOrDefault();
                if (smsDocument != null)
                {
                    _logger.LogInformation("Successfully retrieved SMS with ID: {SmsId}", id);
                    return smsDocument.ToSentSmsMessage();
                }
            }
            
            _logger.LogWarning("SMS with ID: {SmsId} not found", id);
            return null;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to retrieve email with ID: {EmailId}. Status: {Status}", id, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving email with ID: {EmailId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteSmsAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting SMS with ID: {EmailId}", id);

            // First, find the SMS to get its partition key
            var sms = await GetSmsAsync(id, cancellationToken);
            if (sms == null)
            {
                _logger.LogWarning("SMS with ID: {SmsId} not found for deletion", id);
                return false;
            }

            var smsDocument = sms.ToDocument();
            await _container.DeleteItemAsync<SmsDocument>(id, new PartitionKey(smsDocument.PartitionKey));

            _logger.LogInformation("Successfully deleted email with ID: {SmsId}", id);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Sms with ID: {SmsId} not found for deletion", id);
            return false;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to delete SMS with ID: {SmsId}. Status: {Status}", id, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting SMS with ID: {SmsId}", id);
            throw;
        }
    }
}