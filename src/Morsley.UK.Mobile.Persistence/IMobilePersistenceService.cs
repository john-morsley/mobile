namespace Morsley.UK.Mobile.Persistence;

public interface ISmsPersistenceService
{
    Task<string> SaveSmsAsync(Common.Models.SmsMessage sms, CancellationToken cancellationToken = default);

    Task<Common.Models.SmsMessage?> GetSmsAsync(string id, CancellationToken cancellationToken = default);

    Task<bool> DeleteSmsAsync(string id, CancellationToken cancellationToken = default);
}
