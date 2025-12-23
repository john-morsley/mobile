namespace Morsley.UK.Mobile.API.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    private readonly CosmosClient _cosmosClient;
    private readonly CosmosDbOptions _cosmosOptions;

    public HomeController(
        ILogger<HomeController> logger,
        IConfiguration configuration,
        CosmosClient cosmosClient,
        IOptions<CosmosDbOptions> cosmosDbOptions)
    {
        _logger = logger;
        _configuration = configuration;
        _cosmosClient = cosmosClient;
        _cosmosOptions = cosmosDbOptions.Value;

        ViewBag.KeyVaultResults = null;
        ViewBag.CosmosDbResults = null;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.IsKeyVaultHealthy = IsKeyVaultHealthy(out var keyVaultDetails);
        ViewBag.KeyVaultDetails = keyVaultDetails;

        var cosmosHealth = await IsCosmosDbHealthy();
        ViewBag.IsCosmosDbHealthy = cosmosHealth.IsHealthy;
        ViewBag.CosmosDbDetails = cosmosHealth.Details;

        return View();
    }

    public IActionResult Swagger()
    {
        return View();
    }

    private bool IsKeyVaultHealthy(out IList<string> details)
    {
        details = [];

        var morsleyUkCosmosDbEndpoint = _configuration["CosmosDb:Endpoint"];
        details.Add($"MorsleyUk--CosmosDb--Endpoint: {morsleyUkCosmosDbEndpoint}");

        var morsleyUkCosmosDbPrimaryReadKey = _configuration["CosmosDb:PrimaryReadKey"];
        details.Add($"MorsleyUk--CosmosDb--PrimaryReadKey: {morsleyUkCosmosDbPrimaryReadKey.ToMaskedSecret()}");

        var morsleyUkCosmosDbSecondaryReadKey = _configuration["CosmosDb:SecondaryReadKey"];
        details.Add($"MorsleyUk--CosmosDb--SecondaryReadKey: {morsleyUkCosmosDbSecondaryReadKey.ToMaskedSecret()}");

        var morsleyUkCosmosDbPrimaryReadWriteKey = _configuration["CosmosDb:PrimaryReadWriteKey"];
        details.Add($"MorsleyUk--CosmosDb--PrimaryReadWriteKey: {morsleyUkCosmosDbPrimaryReadWriteKey.ToMaskedSecret()}");

        var morsleyUkCosmosDbSecondaryReadWriteKey = _configuration["CosmosDb:SecondaryReadWriteKey"];
        details.Add($"MorsleyUk--CosmosDb--SecondaryReadWriteKey: {morsleyUkCosmosDbSecondaryReadWriteKey.ToMaskedSecret()}");

        var morsleyUkTwilioSettingsAccountSid = _configuration["TwilioSettings:AccountSid"];
        details.Add($"MorsleyUk--TwilioSettings--AccountSid: {morsleyUkTwilioSettingsAccountSid.ToMaskedSecret()}");

        var morsleyUkTwilioSettingsAuthToken = _configuration["TwilioSettings:AuthToken"];
        details.Add($"MorsleyUk--TwilioSettings--AuthToken: {morsleyUkTwilioSettingsAuthToken.ToMaskedSecret()}");

        var morsleyUkTwilioSettingsPrimaryMobileNumber = _configuration["TwilioSettings:PrimaryMobileNumber"];
        details.Add($"MorsleyUk--TwilioSettings--PrimaryMobileNumber: {morsleyUkTwilioSettingsPrimaryMobileNumber.ToMaskedSecret()}");

        var morsleyUkTwilioSettingsSecondaryMobileNumber = _configuration["TwilioSettings:SecondaryMobileNumber"];
        details.Add($"MorsleyUk--TwilioSettings--SecondaryMobileNumber: {morsleyUkTwilioSettingsSecondaryMobileNumber.ToMaskedSecret()}");

        return true;
    }

    private async Task<(bool IsHealthy, IList<string> Details)> IsCosmosDbHealthy()
    {
        var results = new List<string>();
        var isHealthy = true;

        try
        {
            if (_cosmosOptions.UseLocalEmulator)
            {
                results.Add($"Using Local Emulator: Yes");
            }
            else
            {
                results.Add($"Using Local Emulator: No");
            }

            var account = await _cosmosClient.ReadAccountAsync();
            results.Add($"Endpoint: {_cosmosClient.Endpoint}");

            var database = _cosmosClient.GetDatabase(_cosmosOptions.DatabaseName);
            var dbResponse = await database.ReadAsync();
            results.Add($"Database '{_cosmosOptions.DatabaseName}' status: {dbResponse.StatusCode}");

            var sentContainer = database.GetContainer(_cosmosOptions.SentSmsContainerName);
            var sentResponse = await sentContainer.ReadContainerAsync();
            results.Add($"Container '{_cosmosOptions.SentSmsContainerName}' status: {sentResponse.StatusCode}");

            var receivedContainer = database.GetContainer(_cosmosOptions.ReceivedSmsContainerName);
            var receivedResponse = await receivedContainer.ReadContainerAsync();
            results.Add($"Container '{_cosmosOptions.ReceivedSmsContainerName}' status: {receivedResponse.StatusCode}");
        }
        catch (CosmosException cex)
        {
            isHealthy = false;
            results.Add($"CosmosException: Status={(int)cex.StatusCode} {cex.StatusCode}; Message={cex.Message}");
        }
        catch (Exception ex)
        {
            isHealthy = false;
            results.Add($"Exception: {ex.GetType().Name}; Message={ex.Message}");
        }

        ViewBag.CosmosDbResults = results;

        return (isHealthy, results);
    }
}