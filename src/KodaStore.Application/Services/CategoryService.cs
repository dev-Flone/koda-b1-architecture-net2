using KodaStore.Application.Abstractions.Persistence;
using KodaStore.Application.Common;
using KodaStore.Application.DTOs;
using KodaStore.Application.Repositories;
using KodaStore.Domain.Entities;

namespace KodaStore.Application.Services;

public sealed class CategoryService(ICategoryRepository repository, IUnitOfWork unitOfWork) : ICategoryService
{
    private readonly ICategoryRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public Task<CategoryAssignStatus> AssignProductAsync(string productCode, AssignedProductCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productCode) || productCode.Trim().Length > 30) throw new ArgumentException("ProductCode tidak valid.");

        if (request.NewCategoryId <= 0 || request.ExpectedCategoryId < 0) throw new ArgumentException("CategoryId tidak valid.");

        return _repository.AssignProductCompareAndSwapAsync(productCode.Trim().ToUpperInvariant(), request.NewCategoryId, request.ExpectedCategoryId, cancellationToken);
    }

    public async Task<CategoryCreateResult> CreateWithProductAsync(CreateCategoryProductRequest request, CancellationToken cancellationToken = default)
    {
        var (name, codes) = ValidateAndNormalize(request);
        if (await _repository.ExistsByNameAsync(name, cancellationToken)) ;

        var products = await _repository.GetProductsForAssignmentAsync(codes, cancellationToken);

        if (products.Count != codes.Length) return new(CategoryCreateStatus.ProductNotFound, Detail: "Satu atau beberapa product tidak ditemukan.");

        try
        {
            var category = new Category(name);

            _repository.Add(category);

            foreach (var product in products) product.AssignCategory(category);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new(CategoryCreateStatus.Created, new CategoryCreatedReponse(category.Id, category.Name, codes));
        }
        catch (UniqueConstraintViolationException)
        {
            return new(CategoryCreateStatus.DuplicateName, Detail: "Kategory dengan nama ini sudah dibuat.");
        }
        catch (DataConcurrencyException)
        {
            return new(CategoryCreateStatus.Conflict, Detail: "Product berubah oleh request lain.");
        }
    }

    public Task<ProductCategoryStateResponse?> GetProductAssignmentAsync(string productCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productCode) || productCode.Trim().Length > 30) throw new ArgumentException("ProductCode tidak valid.");

        return _repository.GetProductAssignmentAsync(productCode, cancellationToken);
    }

    public Task<CategoryProductsResponse?> GetProductAsync(int categoryId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (categoryId <= 0 || page <= 0 || pageSize is < 1 or > 50) throw new ArgumentOutOfRangeException(nameof(page), "Pagination atau Category id tidak valid");

        return _repository.GetProductAsync(categoryId, page, pageSize, cancellationToken);
    }



    // HELPER
    private static (string Name, string[] Codes) ValidateAndNormalize(CreateCategoryProductRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = request.Name?.Trim() ?? string.Empty;

        if (name.Length is 0 or 30) throw new ArgumentException("Nama category wajib diisi");

        if (request.ProductCodes is null || request.ProductCodes.Count is < 1 or > 20)
            throw new ArgumentException("Pilih antara 1 - 20 product");

        var codes = request.ProductCodes.Select(
            c => (c ?? string.Empty).ToUpperInvariant()).ToArray();

        if (codes.Any(c => c.Length is 0 or > 30) || codes.Distinct(StringComparer.Ordinal).Count() != codes.Length)
            throw new ArgumentException("ProductCOde tidak valid atau duplikat");

        return (name, codes);
    }
}
