Console.WriteLine("Running Morsley.UK.Mobile.Example\n");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        cfg.AddJsonFile("AppSettings.Example.json", optional: false, reloadOnChange: true);
        cfg.AddUserSecrets<Program>();
        cfg.AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) => {
        services.AddSmsSender(ctx.Configuration);
        //services.AddSmsReader(ctx.Configuration);
    })
    .Build();

var sender = host.Services.GetRequiredService<ISmsSender>();

var unique = Guid.NewGuid();
var primaryMobileNumber = host.Services.GetRequiredService<IConfiguration>()["TwilioSettings:PrimaryMobileNumber"];
var secondaryMobileNumber = host.Services.GetRequiredService<IConfiguration>()["TwilioSettings:SecondaryMobileNumber"];
var message = $"Test - {unique}";

Console.WriteLine("============================== SENDING SMS ==============================");
try
{
    Console.WriteLine("Sending SMS...");

    //Console.WriteLine($"To: {emailTo}");
    //Console.WriteLine($"Subject: {emailSubject}");
    //Console.WriteLine($"Body: {emailBody}");

    if (string.IsNullOrWhiteSpace(primaryMobileNumber))
    {
        Console.WriteLine("Could not determine primary mobile number to send to.");
    }
    if (string.IsNullOrWhiteSpace(secondaryMobileNumber))
    {
        Console.WriteLine("Could not determine secondary mobile number to send to.");
    }

    if (!string.IsNullOrWhiteSpace(primaryMobileNumber) &&
        !string.IsNullOrWhiteSpace(secondaryMobileNumber))
    {
        await sender.SendAsync(primaryMobileNumber, secondaryMobileNumber, message);
    }

    Console.WriteLine("Successfully sent");
}
catch (Exception)
{
    Console.WriteLine("Sending failed unexpectedly!");
}
Console.WriteLine("============================== SENDING SMS ==============================\n");

Console.WriteLine("Press any key");
Console.ReadLine();