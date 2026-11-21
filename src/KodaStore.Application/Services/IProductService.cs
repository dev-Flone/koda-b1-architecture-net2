using KodaStore.Application.DTOs;
using KodaStore.Domain.Entities;

namespace KodaStore.Application.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductResponse?> GetByCodeAsync(string productCode, CancellationToken cancellationToken = default);
    Task<ProductResponse?> CreateAsync(ProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse?> UpdateAsync(string productCode, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string productCode, CancellationToken cancellationToken = default);
    Task<bool> ActivateAsync(string productCode, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(string productCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetActivateProductAsync(CancellationToken cancellationToken = default);

}
