using KodaStore.Application.Repositories;
using KodaStore.Infrastructure.Data;
using KodaStore.Application.DTOs;
using KodaStore.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KodaStore.Infrastructure.Repositories;

public sealed class EfCategoryRepository(AppDbContext context) : ICategoryRepository
{
    private readonly AppDbContext _context = context;
    public void Add(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        _context.Category.Add(category);
    }

    public async Task<CategoryAssignStatus> AssignProductCompareAndSwapAsync(string productCode, int NewCategoryId, int? expectedCategoryId, CancellationToken cancellationToken = default)
    {
        var newCategory = await _context.Category.AsNoTracking()
            .Where(c => c.Id == NewCategoryId)
            .Select(c => new { c.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (newCategory is null) return CategoryAssignStatus.CategoryNotFound;

        int updated;
        try
        {
            updated = await _context.Products
                .Where(p => p.ProductCode == productCode && p.CategoryId == expectedCategoryId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.CategoryId, NewCategoryId)
                    .SetProperty(p => p.Category, newCategory.Name), cancellationToken);
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return CategoryAssignStatus.Conflict;
        }

        if (updated == 1) return CategoryAssignStatus.Updated;

        bool exists = await _context.Products.AsNoTracking().AnyAsync(p => p.ProductCode == productCode, cancellationToken);

        return exists ? CategoryAssignStatus.Conflict : CategoryAssignStatus.CategoryNotFound;
    }

    public Task<CategoryCreateResult> CreateWithProductAsync(string name, IReadOnlyList<string> productCodes, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
    {
        return _context.Category.AsNoTracking().AnyAsync(category => category.Name == name, cancellationToken);
    }

    public async Task<ProductCategoryStateResponse?> GetProductAssignmentAsync(string productCode, CancellationToken cancellationToken = default)
    {
        var state = await _context.Products.AsNoTracking().Where(p => p.ProductCode == productCode).Select(p => new
        {
            p.ProductCode,
            p.CategoryId,
            categoryName = p.CategoryEntity == null ? null : p.CategoryEntity.Name,
            p.RowVersion
        }).SingleOrDefaultAsync(cancellationToken);

        return state is null ? null : new ProductCategoryStateResponse(state.ProductCode, state.CategoryId, state.categoryName, Convert.ToBase64String(state.RowVersion));
    }

    public async Task<CategoryProductsResponse?> GetProductAsync(int categoryId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var category = await _context.Category.AsNoTracking()
            .Where(c => c.Id == categoryId)
            .Select(c => new { c.Id, c.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (category is null) return null;

        var query = _context.Products.AsNoTracking()
            .Where(p => p.CategoryId == categoryId);

        long totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query.OrderBy(p => p.ProductCode)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new CategoryProductResponse(
                p.ProductCode, p.Name, p.Price, p.Stock, p.CategoryEntity!.Name
            )).ToArrayAsync(cancellationToken);

        return new CategoryProductsResponse(category.Id, category.Name, page, pageSize, totalCount, items);
    }

    public async Task<IReadOnlyList<Product>> GetProductsForAssignmentAsync(IReadOnlyList<string> productCode, CancellationToken cancellationToken)
    {
        return await _context.Products.AsNoTracking().Where(p => productCode.Contains(p.ProductCode)).ToListAsync(cancellationToken);
    }
}

