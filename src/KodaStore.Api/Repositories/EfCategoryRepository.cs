using KodaStore.Api.Data;
using KodaStore.Api.DTOs;
using KodaStore.Api.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KodaStore.Api.Repositories;

public sealed class EfCategoryRepository(AppDbContext context, ILogger<EfCategoryRepository> logger) : ICategoryRepository
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<EfCategoryRepository> _logger = logger;

    public async Task<CategoryAssignStatus> AssignProductCompareAndSwapAsync(string productCode, int newCategoryId, int? expectedCategoryId, CancellationToken cancellationToken)
    {
        var newCategory = await _context.Category.AsNoTracking()
            .Where(c => c.Id == newCategoryId)
            .Select(c => new { c.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (newCategory is null) return CategoryAssignStatus.CategoryNotFound;

        int updated;

        try
        {
            updated = await _context.Products
                            .Where(p => p.ProductCode == productCode && p.CategoryId == expectedCategoryId)
                            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.CategoryId, newCategoryId).SetProperty(p => p.Category, newCategory.Name), cancellationToken);
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return CategoryAssignStatus.Conflict;
        }

        if (updated == 1) return CategoryAssignStatus.Updated;

        bool exists = await _context.Products.AsNoTracking()
            .AnyAsync(p => p.ProductCode == productCode, cancellationToken);

        return exists ? CategoryAssignStatus.Conflict : CategoryAssignStatus.ProductNotFound;
    }

    public async Task<CategoryCreateResult> CreateWithProductAsync(string name, IReadOnlyList<string> productCodes, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            if (await _context.Category.AnyAsync(c => c.Name == name, cancellationToken)) return new CategoryCreateResult(CategoryCreateStatus.DuplicateName, Detail: "Category Name already registered.");

            var category = new Category(name);
            _context.Category.Add(category);
            await _context.SaveChangesAsync(cancellationToken);

            var products = await _context.Products.Where(p => productCodes.Contains(p.ProductCode)).ToListAsync(cancellationToken);

            if (products.Count != productCodes.Count)
            {
                await transaction.RollbackAsync(cancellationToken);

                return new CategoryCreateResult(CategoryCreateStatus.ProductNotFound, Detail: "One or more ProductCode Not found.");
            }

            foreach (var product in products) product.AssignCategory(category);

            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new CategoryCreateResult(CategoryCreateStatus.Created, new CategoryCreatedReponse(category.Id, category.Name, [.. productCodes]));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            _logger.LogWarning(ex, "Concurren category assignment detected.");
            return new CategoryCreateResult(CategoryCreateStatus.Conflict, Detail: "Product chanded. Reload the data.");
        }

        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            await transaction.RollbackAsync(CancellationToken.None);
            return new CategoryCreateResult(CategoryCreateStatus.DuplicateName, Detail: "Category with this name already created.");
        }

        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<ProductCategoryStateResponse?> GetProductAssignmentAsync(string productCode, CancellationToken cancellationToken)
    {
        var state = await _context.Products.AsNoTracking()
            .Where(p => p.ProductCode == productCode)
            .Select(p => new
            {
                p.ProductCode,
                p.CategoryId,
                CategoryName = p.CategoryEntity == null ? null : p.CategoryEntity.Name,
                p.RowVersion
            })
            .SingleOrDefaultAsync(cancellationToken);
        return state is null ? null : new ProductCategoryStateResponse(
            state.ProductCode, state.CategoryId, state.CategoryName,
            Convert.ToBase64String(state.RowVersion));
    }

    public async Task<CategoryProductsResponse?> GetProductAsync(int categoryId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var category = await _context.Category.AsNoTracking()
                .Where(c => c.Id == categoryId)
                .Select(c => new
                {
                    c.Id,
                    c.Name
                })
                .SingleOrDefaultAsync(cancellationToken);
        if (category is null) return null;

        var query = _context.Products.AsNoTracking()
            .Where(p => p.CategoryId == categoryId);

        long totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query.OrderBy(p => p.ProductCode)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new CategoryProductResponse(p.ProductCode, p.Name, p.Price, p.Stock, p.CategoryEntity!.Name)).ToArrayAsync(cancellationToken);

        return new CategoryProductsResponse(category.Id, category.Name,
         page, pageSize, totalCount, items);
    }
}