using KodaStore.Domain.Entities;

namespace KodaStore.Application.Repositories;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetByCodeAsync(string productCode, CancellationToken cancellationToken = default);
    Task<bool> TryAddAsync(Product product, CancellationToken cancellationToken = default);
    Task<bool> TryUpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task<bool> TryDeleteAsync(string productCode, CancellationToken cancellationToken = default);
    Task<bool> ActivateAsync(string productCode, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(string productCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetActivateProductAsync(CancellationToken cancellationToken = default);

}
