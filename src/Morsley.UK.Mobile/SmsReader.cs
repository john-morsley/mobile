namespace Morsley.UK.Mobile;

public class SmsReader : ISmsReader
{
    private readonly IOptionsMonitor<TwilioSettings> _options;

    public SmsReader(IOptionsMonitor<TwilioSettings> options)
    {
        _options = options;
    }

    public async Task ReadAsync(string fromNumber, string toNumber, string message)
    {
        var settings = _options.CurrentValue;

        try
        {
            // ToDo --> Read the SMS messages from the Cosmos DB
        }
        catch (Exception)
        {
            throw;
        }
    }
}