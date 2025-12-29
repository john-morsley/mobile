namespace Morsley.UK.Mobile.Persistence;

public interface ISentSmsPersistenceService : ISmsPersistenceService
{
    Task<PaginatedResponse<SmsMessage>> GetPageAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);
}