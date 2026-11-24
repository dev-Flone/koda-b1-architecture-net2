using KodaStore.Application.DTOs;
using KodaStore.Application.Security;
using KodaStore.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KodaStore.Api.Controllers;

[ApiController]
[Route("api/v1/categories")]
[Authorize(Policy = AppPolicies.CanManageCategory)]
public sealed class CategoryController(ICategoryService service) : ControllerBase
{
    private readonly ICategoryService _service = service;

    [AllowAnonymous]
    [HttpGet("{categoryId:int}/produtcs")]
    public async Task<ActionResult<CategoryProductsResponse>> GetProducts(int categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        if (categoryId <= 0 || page < 1 || pageSize is < 1 or > 50) return BadRequest(new ProblemDetails
        {
            Title = "Invalid paging parameter.",
            Status = 400
        });

        var data = await _service.GetProductAsync(categoryId, page, pageSize, cancellationToken);

        return data is null ? NotFound() : Ok(data);
    }
    [EnableRateLimiting("category-writes")]
    [HttpPost("with-products")]
    public async Task<IActionResult> CreateWithProducts([FromBody] CreateCategoryProductRequest request, CancellationToken ct)
    {
        CategoryCreateResult result;
        try
        {
            result = await _service.CreateWithProductAsync(request, ct);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = 400
            });
        }
        return result.Status switch
        {
            CategoryCreateStatus.Created => CreatedAtAction(nameof(GetProducts), new { categoryId = result.Data!.CategoryId }, result.Data),
            CategoryCreateStatus.ProductNotFound => NotFound(new ProblemDetails { Title = result.Detail, Status = 404 }),
            CategoryCreateStatus.DuplicateName => Conflict(new ProblemDetails { Title = result.Detail, Status = 429 }),
            CategoryCreateStatus.Conflict => Conflict(new ProblemDetails { Title = result.Detail, Status = 409 }),
            _ => StatusCode(500)
        };
    }
}
