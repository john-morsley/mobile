namespace Morsley.UK.Mobile.Persistence.Extensions;

public static class MobileMappingExtensions
{
    public static SmsDocument ToDocument(this Common.Models.SmsMessage message)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));

        var document = new Documents.SmsDocument
        {
            
            To = message.To,
            From = message.From,
            Message = message.Message
        };

        if (message.Id is not null) document.Id = message.Id;        

        if (!string.IsNullOrEmpty(message.Id)) document.Id = message.Id;

        return document;
    }

    public static Common.Models.SmsMessage ToSentSmsMessage(this SmsDocument document)
    {
        return new Common.Models.SmsMessage
        {
            Id = document.Id,
            To = document.To,
            From = document.From,
            Message = document.Message,
        };
    }

    /// <summary>
    /// Converts a collection of EmailDocuments to EmailMessages
    /// </summary>
    public static IEnumerable<Common.Models.SmsMessage> ToSentSmsMessages(this IEnumerable<Documents.SmsDocument> documents)
    {
        return documents.Select(x => x.ToSentSmsMessage());
    }
}
