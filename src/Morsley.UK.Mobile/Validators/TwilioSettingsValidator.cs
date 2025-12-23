namespace Morsley.UK.Mobile.API.Validators;

public class TwilioSettingsValidator : IValidateOptions<TwilioSettings>
{
    public ValidateOptionsResult Validate(string? name, TwilioSettings options)
    {
        var errors = new List<string>();

        if (options.AccountSid == "[Stored in User Secrets]")
        {
            errors.Add("TwilioSettings.AccountSid is not configured. Please set it in user secrets.");
        }

        if (options.AuthToken == "[Stored in User Secrets]")
        {
            errors.Add("TwilioSettings.AuthToken is not configured. Please set it in user secrets.");
        }

        if (options.PrimaryMobileNumber == "[Stored in User Secrets]")
        {
            errors.Add("TwilioSettings.PrimaryMobileNumber is not configured. Please set it in user secrets.");
        }

        if (options.SecondaryMobileNumber == "[Stored in User Secrets]")
        {
            errors.Add("TwilioSettings.SecondaryMobileNumber is not configured. Please set it in user secrets.");
        }

        if (errors.Any())
        {
            return ValidateOptionsResult.Fail(errors);
        }

        return ValidateOptionsResult.Success;
    }
}