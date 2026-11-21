using KodaStore.Domain.Entities;

namespace KodaStore.Application.Repositories;

public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Customer?> TryAddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<bool> TryUpdateAsync(int id, Customer customer, CancellationToken cancellationToken = default);
    Task<bool> TryDeleteAsync(int id, CancellationToken cancellationToken = default);

}
