using KodaStore.Application.DTOs;
using KodaStore.Domain.Entities;
using KodaStore.Application.Repositories;

namespace KodaStore.Application.Services;

public sealed class ProductService(IProductRepository repository) : IProductService
{
    private readonly IProductRepository _repository = repository;

    public async Task<bool> ActivateAsync(string productCode, CancellationToken cancellationToken)
    {
        return await _repository.ActivateAsync(productCode, cancellationToken);
    }

    // CREATE
    public async Task<ProductResponse?> CreateAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product(request.CustomerId, request.ProductCode, request.Name, request.Price, request.Stock, request.Category);

        var created = await _repository.TryAddAsync(product, cancellationToken);

        return created ? ProductResponse.FromModel(product) : null;
    }

    public async Task<bool> DeactivateAsync(string productCode, CancellationToken cancellationToken)
    {
        return await _repository.DeactivateAsync(productCode, cancellationToken);
    }

    // DELETE
    public Task<bool> DeleteAsync(string productCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(productCode) || productCode.Trim().Length > 30)
        {
            return Task.FromResult(false);
        }
        return _repository.TryDeleteAsync(productCode, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetActivateProductAsync(CancellationToken cancellationToken)
    {
        return await _repository.GetActivateProductAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var products = await _repository.GetAllAsync(cancellationToken);
        return [.. products.Select(ProductResponse.FromModel)];
    }

    public async Task<ProductResponse?> GetByCodeAsync(string productCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(productCode) | productCode.Trim().Length > 30)
        {
            return null;
        }
        var product = await _repository.GetByCodeAsync(productCode, cancellationToken);
        return product is null ? null : ProductResponse.FromModel(product);
    }

    // UPDATE
    public async Task<ProductResponse?> UpdateAsync(string productCode, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product(request.CustomerId, productCode, request.Name, request.Price, request.Stock, request.Category);

        var updated = await _repository.TryUpdateAsync(product, cancellationToken);

        return updated ? ProductResponse.FromModel(product) : null;
    }
}
