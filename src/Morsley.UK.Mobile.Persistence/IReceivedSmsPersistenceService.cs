namespace Morsley.UK.Mobile.Persistence;

public interface IReceivedSmsPersistenceService : ISmsPersistenceService
{
    Task<PaginatedResponse<SmsMessage>> GetPageAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);
}
