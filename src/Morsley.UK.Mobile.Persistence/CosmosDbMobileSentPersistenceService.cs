namespace Morsley.UK.Mobile.Persistence;

public class CosmosDbMobileSentPersistenceService : CosmosDbMobilePersistenceService, ISmsSentPersistenceService
{
    public CosmosDbMobileSentPersistenceService(
        CosmosClient cosmosClient, 
        CosmosDbOptions options, 
        ILoggerFactory loggerFactory) : base(
            cosmosClient, 
            options.DatabaseName, 
            options.SentSmsContainerName,
            loggerFactory) { }
}