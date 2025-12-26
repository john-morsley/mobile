namespace Morsley.UK.Mobile.Models;

public class TwilioSettings
{
    public string AccountSid { get; set; } = string.Empty;

    public string AuthToken { get; set; } = string.Empty;

    public string PrimaryMobileNumber { get; set; } = string.Empty;

    public string SecondaryMobileNumber { get; set; } = string.Empty;
}