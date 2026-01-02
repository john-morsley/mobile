namespace Morsley.UK.Mobile.Common.Models;

public class SmsMessage
{
    public string? Id { get; set; }

    public string To { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime? CreatedUtc { get; set; }
}