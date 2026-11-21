using KodaStore.Application.DTOs;
using KodaStore.Application.Repositories;

namespace KodaStore.Application.Services;

public sealed class CategoryService(ICategoryRepository repository) : ICategoryService
{
    private readonly ICategoryRepository _repository = repository;
    public Task<CategoryAssignStatus> AssignProductAsync(string productCode, AssignedProductCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productCode) || productCode.Trim().Length > 30) throw new ArgumentException("Product Code is not valid.");

        if (request.NewCategoryId <= 0 || request.ExpectedCategoryId < 0) throw new ArgumentException("CategoryId is not valid.");

        return _repository.AssignProductCompareAndSwapAsync(productCode.Trim().ToUpperInvariant(), request.NewCategoryId, request.ExpectedCategoryId, cancellationToken);
    }

    public Task<CategoryCreateResult> CreateWithProductAsync(CreateCategoryProductRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 30) throw new ArgumentException("Category Name is required.");

        if (request.ProductCodes is null || request.ProductCodes.Count is < 1 or > 20) throw new ArgumentException("Choose between 1 to 20 products.");

        var codes = request.ProductCodes.Select(s => (s ?? string.Empty).Trim().ToUpperInvariant()).ToArray();

        if (codes.Any(s => string.IsNullOrWhiteSpace(s) || s.Length > 30) || codes.Distinct(StringComparer.Ordinal).Count() != codes.Length) throw new ArgumentException("ProductCode is not valid or duplicate.");

        return _repository.CreateWithProductAsync(request.Name.Trim(), codes, cancellationToken);
    }

    public Task<ProductCategoryStateResponse?> GetProductAssignmentAsync(string productCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productCode) || productCode.Trim().Length > 30) throw new ArgumentException("Product Code is not valid.");

        return _repository.GetProductAssignmentAsync(productCode.Trim().ToUpperInvariant(), cancellationToken);
    }

    public Task<CategoryProductsResponse?> GetProductAsync(int categoryId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (categoryId <= 0 || page <= 0 || pageSize is < 1 or > 50) throw new ArgumentOutOfRangeException(nameof(page), "Pagination or CategoryId is not valid.");

        return _repository.GetProductAsync(categoryId, page, pageSize, cancellationToken);
    }
}
