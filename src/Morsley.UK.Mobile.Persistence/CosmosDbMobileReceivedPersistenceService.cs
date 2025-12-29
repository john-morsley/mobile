namespace Morsley.UK.Mobile.Persistence;

public class CosmosDbMobileReceivedPersistenceService : CosmosDbMobilePersistenceService, ISmsReceivedPersistenceService
{
    public CosmosDbMobileReceivedPersistenceService(
        CosmosClient cosmosClient, 
        CosmosDbOptions options, 
        ILoggerFactory loggerFactory) : base(
            cosmosClient, 
            options.DatabaseName, 
            options.ReceivedSmsContainerName, 
            loggerFactory) { }    
}