namespace Morsley.UK.Mobile.API
{
    public class SmsMessage
    {
        public string Sid { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string To { get; set; } = string.Empty;

        public string From { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;
    }
}