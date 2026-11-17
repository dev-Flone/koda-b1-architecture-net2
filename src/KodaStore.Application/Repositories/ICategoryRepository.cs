
using KodaStore.Application.DTOs;

namespace KodaStore.Application.Repositories;

public interface ICategoryRepository
{
    Task<CategoryCreateResult> CreateWithProductAsync(string name, IReadOnlyList<string> productCodes, CancellationToken cancellationToken);

    Task<CategoryProductsResponse?> GetProductAsync(int categoryId, int Page, int PageSize, CancellationToken cancellationToken);

    Task<ProductCategoryStateResponse?> GetProductAssignmentAsync(string productCode, CancellationToken cancellationToken);

    Task<CategoryAssignStatus> AssignProductCompareAndSwapAsync(string productCode, int NewCategoryId, int? expectedCategoryId, CancellationToken cancellationToken);
}
