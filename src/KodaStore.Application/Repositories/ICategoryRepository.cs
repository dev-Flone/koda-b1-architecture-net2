
using KodaStore.Application.DTOs;
using KodaStore.Domain.Entities;

namespace KodaStore.Application.Repositories;

public interface ICategoryRepository
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);

    void Add(Category category);

    Task<IReadOnlyList<Product>> GetProductsForAssignmentAsync(IReadOnlyList<string> productCode, CancellationToken cancellationToken);

    Task<CategoryCreateResult> CreateWithProductAsync(string name, IReadOnlyList<string> productCodes, CancellationToken cancellationToken = default);

    Task<CategoryProductsResponse?> GetProductAsync(int categoryId, int Page, int PageSize, CancellationToken cancellationToken = default);

    Task<ProductCategoryStateResponse?> GetProductAssignmentAsync(string productCode, CancellationToken cancellationToken = default);

    Task<CategoryAssignStatus> AssignProductCompareAndSwapAsync(string productCode, int NewCategoryId, int? expectedCategoryId, CancellationToken cancellationToken = default);
}
