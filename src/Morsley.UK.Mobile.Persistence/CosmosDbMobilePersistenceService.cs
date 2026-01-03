namespace Morsley.UK.Mobile.Persistence;

public abstract class CosmosDbMobilePersistenceService
{
    protected readonly Container _container;

    public CosmosDbMobilePersistenceService(
        CosmosClient cosmosClient, 
        string databaseName,
        string containerName,
        ILoggerFactory loggerFactory)
    {
        _container = cosmosClient.GetContainer(databaseName, containerName);

        var t = GetType();
        Logger = loggerFactory.CreateLogger(t);
    }

    protected ILogger Logger { get; }

    public async Task<bool> DeleteByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Deleting SMS with ID: {SmsId}", id);

            var sms = await GetByIdAsync(id, cancellationToken);
            if (sms == null)
            {
                Logger.LogWarning("SMS with ID: {SmsId} not found for deletion", id);
                return false;
            }

            var smsDocument = sms.ToDocument();
            await _container.DeleteItemAsync<SmsDocument>(id, new PartitionKey(smsDocument.PartitionKey));

            Logger.LogInformation("Successfully deleted SMS with ID: {SmsId}", id);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Logger.LogWarning("Sms with ID: {SmsId} not found for deletion", id);
            return false;
        }
        catch (CosmosException ex)
        {
            Logger.LogError(ex, "Failed to delete SMS with ID: {SmsId}. Status: {Status}", id, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error deleting SMS with ID: {SmsId}", id);
            throw;
        }
    }

    public async Task<SmsMessage?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Retrieving SMS with ID: {SmsId}", id);

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
                    Logger.LogInformation("Successfully retrieved SMS with ID: {SmsId}", id);
                    return smsDocument.ToSentSmsMessage();
                }
            }

            Logger.LogWarning("SMS with ID: {SmsId} not found", id);
            return null;
        }
        catch (CosmosException ex)
        {
            Logger.LogError(ex, "Failed to retrieve SMS with ID: {SmsId}. Status: {Status}", id, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error retrieving SMS with ID: {SmsId}", id);
            throw;
        }
    }

    public async Task<PaginatedResponse<SmsMessage>> GetPageAsync(PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Retrieving SMS messages with pagination - Page: {Page}, PageSize: {PageSize}", pagination.Page, pagination.PageSize);

            var countQuery = _container.GetItemQueryIterator<int>("SELECT VALUE COUNT(1) FROM c");

            int totalCount = 0;
            while (countQuery.HasMoreResults)
            {
                var countResponse = await countQuery.ReadNextAsync();
                totalCount = countResponse.FirstOrDefault();
                break;
            }

            var queryText =
                $"""
                  SELECT *
                    FROM c
                ORDER BY c.CreatedUtc DESC
                  OFFSET {pagination.Skip}
                   LIMIT {pagination.PageSize}
                """;

            var query = _container.GetItemQueryIterator<SmsDocument>(queryText);

            var pageOfSmsDocuments = new List<SmsDocument>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                pageOfSmsDocuments.AddRange(response.ToList());
            }

            var pageOfSmsMessages = pageOfSmsDocuments.ToSentSmsMessages();

            var paginatedResponse = new PaginatedResponse<SmsMessage>
            {
                Items = pageOfSmsMessages,
                Page = pagination.Page,
                PageSize = pagination.PageSize,
                TotalItems = totalCount
            };

            Logger.LogInformation(
                "Successfully retrieved {Count} SMS (Page {Page}/{TotalPages})",
                pageOfSmsMessages.Count(),
                paginatedResponse.Page,
                paginatedResponse.TotalPages);

            return paginatedResponse;
        }
        catch (CosmosException ex)
        {
            Logger.LogError(ex, "Failed to retrieve paginated SMS. Status: {Status}", ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error retrieving paginated SMS");
            throw;
        }
    }

    public async Task<string> SaveAsync(SmsMessage sms, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Saving SMS");

            var smsDocument = sms.ToDocument();

            var response = await _container.UpsertItemAsync(smsDocument, new PartitionKey(smsDocument.PartitionKey));

            Logger.LogInformation("Successfully saved SMS with ID: {SmsId}. Request charge: {RequestCharge}", sms.Id, response.RequestCharge);

            return response.Resource.Id;
        }
        catch (CosmosException ex)
        {
            Logger.LogError(ex, "Failed to save SMS: Status: {Status}, Message: {Message}", ex.StatusCode, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error saving SMS");
            throw;
        }
    }    

    public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Deleting all SMS");

            var queryDefinition = new QueryDefinition("SELECT VALUE c.id FROM c");

            var query = _container.GetItemQueryIterator<string>(
                queryDefinition,
                requestOptions: new QueryRequestOptions { MaxItemCount = 100 });

            var deletedCount = 0;

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync(cancellationToken);

                foreach (var id in response)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        await _container.DeleteItemAsync<SmsDocument>(
                            id,
                            new PartitionKey(id),
                            cancellationToken: cancellationToken);
                        deletedCount++;
                    }
                    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                    }
                }
            }

            Logger.LogInformation("Successfully deleted all SMS. Deleted count: {DeletedCount}", deletedCount);
        }
        catch (CosmosException ex)
        {
            Logger.LogError(ex, "Failed to delete all SMS. Status: {Status}", ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error deleting all SMS");
            throw;
        }
    }

}