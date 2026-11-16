using KodaStore.Api.DTOs;

namespace KodaStore.Api.Services;

public interface ICategoryService
{
    Task<CategoryCreateResult> CreateWithProductAsync(CreateCategoryProductRequest request, CancellationToken cancellationToken);
    Task<CategoryProductsResponse?> GetProductAsync(int categoryId, int page, int pageSize, CancellationToken cancellationToken);
    Task<ProductCategoryStateResponse?> GetProductAssignmentAsync(string productCode, CancellationToken cancellationToken);
    Task<CategoryAssignStatus> AssignProductAsync(string productCode, AssignedProductCategoryRequest request, CancellationToken cancellationToken);
}