namespace Morsley.UK.Mobile;

public class TwilioSettings
{
    public required string AccountSid { get; set; }

    public required string AuthToken { get; set; }

    public required string PrimaryMobileNumber { get; set; }

    public required string SecondaryMobileNumber { get; set; }
}