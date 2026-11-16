using System.ComponentModel.DataAnnotations;

namespace KodaStore.Api.DTOs;

public sealed class CreateCategoryProductRequest : IValidatableObject
{
    [Required, StringLength(30)] public string Name { get; private set; } = string.Empty;
    [Required, MinLength(1), MaxLength(20)] public List<string> ProductCodes { get; private set; } = [];


    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ProductCodes is null) yield break;
        if (ProductCodes.Any(code => string.IsNullOrWhiteSpace(code) || code.Trim().Length > 30)) yield return new ValidationResult("Every Product Code must be filled with maximum of 30 characters.", [nameof(ProductCodes)]);

        if (ProductCodes.Where(pc => !string.IsNullOrWhiteSpace(pc)).Select(pc => pc.Trim().ToUpperInvariant()).Distinct(StringComparer.Ordinal).Count() != ProductCodes.Count) yield return new ValidationResult("Product Code cannot repeat.", [nameof(ProductCodes)]);
    }
}

public sealed record CategoryProductResponse(string ProductCode, string Name, decimal Price, int Stock, string CategoryName);

public sealed record CategoryProductsResponse(int CategoryId, string CategoryName, int Page, int PageSize, long TotalCount, IReadOnlyList<CategoryProductResponse> Items);

public sealed record CategoryCreatedReponse(int CategoryId, string Name, IReadOnlyList<string> AssignedProductCodes);

public enum CategoryCreateStatus
{
    Created, DuplicateName, ProductNotFound, Conflict
}

public sealed record CategoryCreateResult(
    CategoryCreateStatus Status, CategoryCreatedReponse? Data = null, string? Detail = null
);

public sealed class AssignedProductCategoryRequest
{
    [Range(1, int.MaxValue)]
    public int NewCategoryId { get; set; }
    public int? ExpectedCategoryId { get; set; }
}

public enum CategoryAssignStatus
{
    Updated, ProductNotFound, CategoryNotFound, Conflict
}

public sealed record ProductCategoryStateResponse(string ProductCode, int? CategoryId, string? CategoryName, string RowVersionBase64);