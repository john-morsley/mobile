namespace Morsley.UK.Mobile;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmsReader(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "TwilioSettings")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var test = configuration.GetSection(sectionName);

        services
            .AddOptions<TwilioSettings>()
            .Bind(configuration.GetSection(sectionName))
            .Validate(s => !string.IsNullOrWhiteSpace(s.AccountSid), "TwilioSettings:AccountSid is required")
            .Validate(s => !string.IsNullOrWhiteSpace(s.AuthToken), "TwilioSettings:AuthToken is required")
            .Validate(s => !string.IsNullOrWhiteSpace(s.PrimaryMobileNumber), "TwilioSettings:PrimaryMobileNumber is required")
            .Validate(s => !string.IsNullOrWhiteSpace(s.SecondaryMobileNumber), "TwilioSettings:SecondaryMobileNumber is required")
            .ValidateOnStart();

        services.AddSingleton<ISmsReader, SmsReader>();

        return services;
    }

    public static IServiceCollection AddSmsSender(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "TwilioSettings")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var test = configuration.GetSection(sectionName);

        services
            .AddOptions<TwilioSettings>()
            .Bind(configuration.GetSection(sectionName))
            .Validate(s => !string.IsNullOrWhiteSpace(s.AccountSid), "TwilioSettings:AccountSid is required")
            .Validate(s => !string.IsNullOrWhiteSpace(s.AuthToken), "TwilioSettings:AuthToken is required")
            .Validate(s => !string.IsNullOrWhiteSpace(s.PrimaryMobileNumber), "TwilioSettings:PrimaryMobileNumber is required")
            .Validate(s => !string.IsNullOrWhiteSpace(s.SecondaryMobileNumber), "TwilioSettings:SecondaryMobileNumber is required")
            .ValidateOnStart();

        services.AddSingleton<ISmsSender, SmsSender>();

        return services;
    }
}