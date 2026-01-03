using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Morsley.UK.Mobile.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMobilePersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "CosmosDb")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Configure CosmosDB options
        services
            .AddOptions<CosmosDbOptions>()
            .Bind(configuration.GetSection(sectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Endpoint), "CosmosDb:Endpoint is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.PrimaryReadWriteKey), "CosmosDb:PrimaryReadWriteKey is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.DatabaseName), "CosmosDb:DatabaseName is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.PartitionKey), "CosmosDb:PartitionKey is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.SentSmsContainerName), "CosmosDb:SentSmsContainerName is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ReceivedSmsContainerName), "CosmosDb:ReceivedSmsContainerName is required")
            .ValidateOnStart();

        // Register CosmosClient
        services.AddSingleton<CosmosClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
            var cosmosClientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                },
                MaxRetryAttemptsOnRateLimitedRequests = 5,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),
                RequestTimeout = TimeSpan.FromSeconds(30),
                OpenTcpConnectionTimeout = TimeSpan.FromSeconds(30)
            };
            
            // For Cosmos DB emulator, bypass SSL certificate validation and add retry logic
            if (options.UseLocalEmulator)
            {
                cosmosClientOptions.HttpClientFactory = () =>
                {
                    HttpMessageHandler httpMessageHandler = new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
                    };
                    return new HttpClient(httpMessageHandler)
                    {
                        Timeout = TimeSpan.FromSeconds(30)
                    };
                };
            }
            
            return new CosmosClient(options.Endpoint, options.PrimaryReadWriteKey, cosmosClientOptions);
        });


        services.AddScoped<ISmsSentPersistenceService>(serviceProvider =>
        {
            var cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();
            var options = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            return new CosmosDbMobileSentPersistenceService(cosmosClient, options, loggerFactory);
        });

        services.AddScoped<ISmsReceivedPersistenceService>(serviceProvider =>
        {
            var cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();
            var options = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            return new CosmosDbMobileReceivedPersistenceService(cosmosClient, options, loggerFactory);
        });

        return services;
    }

    public static IServiceCollection AddMobilePersistence(
        this IServiceCollection services,
        Action<CosmosDbOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        // Configure CosmosDB options
        services
            .AddOptions<CosmosDbOptions>()
            .Configure(configure)
            .Validate(options => !string.IsNullOrWhiteSpace(options.Endpoint), "CosmosDb:Endpoint is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.PrimaryReadWriteKey), "CosmosDb:PrimaryReadWriteKey is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.DatabaseName), "CosmosDb:DatabaseName is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.SentSmsContainerName), "CosmosDb:SentSmsContainerName is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ReceivedSmsContainerName), "CosmosDb:ReceivedSmsContainerName is required")
            .ValidateOnStart();

        // Register CosmosClient
        services.AddSingleton<CosmosClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
            var cosmosClientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                },
                MaxRetryAttemptsOnRateLimitedRequests = 5,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),
                RequestTimeout = TimeSpan.FromSeconds(30),
                OpenTcpConnectionTimeout = TimeSpan.FromSeconds(30)
            };
            
            // For Cosmos DB emulator, bypass SSL certificate validation and add retry logic
            if (options.Endpoint.Contains("localhost") || options.Endpoint.Contains("127.0.0.1"))
            {
                cosmosClientOptions.HttpClientFactory = () =>
                {
                    HttpMessageHandler httpMessageHandler = new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
                    };
                    return new HttpClient(httpMessageHandler)
                    {
                        Timeout = TimeSpan.FromSeconds(30)
                    };
                };
            }
            
            return new CosmosClient(options.Endpoint, options.PrimaryReadWriteKey, cosmosClientOptions);
        });

        services.AddScoped<ISmsSentPersistenceService>(serviceProvider =>
        {
            var cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();
            var options = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            return new CosmosDbMobileSentPersistenceService(cosmosClient, options, loggerFactory);
        });

        services.AddScoped<ISmsReceivedPersistenceService>(serviceProvider =>
        {
            var cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();
            var options = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            return new CosmosDbMobileReceivedPersistenceService(cosmosClient, options, loggerFactory);
        });

        return services;
    }

    public static async Task InitializeCosmosDbAsync(this IServiceProvider serviceProvider, bool throwOnError = false)
    {
        var logger = serviceProvider.GetService<ILogger<CosmosClient>>();
        
        const int maxRetries = 10;
        const int delayMs = 3000;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();
                var options = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;

                logger?.LogInformation("Initializing Cosmos DB (attempt {Attempt}/{MaxRetries}): Endpoint={Endpoint}, Database={DatabaseName}", 
                    attempt, maxRetries, options.Endpoint, options.DatabaseName);

                // Create database if it doesn't exist
                logger?.LogInformation("Creating database '{DatabaseName}'...", options.DatabaseName);
                var databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(
                    options.DatabaseName,
                    throughput: 400); // Set appropriate throughput for your needs

                logger?.LogInformation("Database '{DatabaseName}' status: {StatusCode}", options.DatabaseName, databaseResponse.StatusCode);

                // Create sent SMS container if it doesn't exist
                logger?.LogInformation("Creating container '{ContainerName}'...", options.SentSmsContainerName);
                var sentContainerProperties = new ContainerProperties(options.SentSmsContainerName, options.PartitionKey);
                var sentContainerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync(sentContainerProperties);
                logger?.LogInformation("Container '{ContainerName}' status: {StatusCode}", options.SentSmsContainerName, sentContainerResponse.StatusCode);
                
                // Create received SMS container if it doesn't exist
                logger?.LogInformation("Creating container '{ContainerName}'...", options.ReceivedSmsContainerName);
                var receivedContainerProperties = new ContainerProperties(options.ReceivedSmsContainerName, options.PartitionKey);
                var receivedContainerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync(receivedContainerProperties);
                logger?.LogInformation("Container '{ContainerName}' status: {StatusCode}", options.ReceivedSmsContainerName, receivedContainerResponse.StatusCode);

                logger?.LogInformation("Cosmos DB initialization completed successfully");
                return; // Success, exit retry loop
            }
            catch (Exception ex) when (attempt < maxRetries && IsRetryableException(ex))
            {
                logger?.LogWarning(ex, "Cosmos DB initialization failed on attempt {Attempt}/{MaxRetries}. Error: {ErrorMessage}. Retrying in {DelayMs}ms...", 
                    attempt, maxRetries, ex.Message, delayMs);
                await Task.Delay(delayMs);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to initialize CosmosDB on attempt {Attempt}/{MaxRetries}. Error: {ErrorMessage}", attempt, maxRetries, ex.Message);
                
                if (throwOnError)
                    throw;
                    
                return; // Non-retryable error, exit
            }
        }
        
        // All retries exhausted
        var finalException = new InvalidOperationException($"Failed to initialize Cosmos DB after {maxRetries} attempts");
        logger?.LogError(finalException, "Cosmos DB initialization failed after all retry attempts");
        
        if (throwOnError)
            throw finalException;
    }

    private static bool IsRetryableException(Exception ex)
    {
        return ex.Message.Contains("Gone") || 
               ex.Message.Contains("ServiceUnavailable") || 
               ex.Message.Contains("RequestTimeout") ||
               ex.Message.Contains("TooManyRequests") ||
               ex.Message.Contains("InternalServerError") ||
               ex.Message.Contains("BadGateway") ||
               ex.Message.Contains("GatewayTimeout") ||
               ex is HttpRequestException ||
               ex is TaskCanceledException;
    }
}