using KodaStore.Application.Repositories;
using KodaStore.Infrastructure.Data;
using KodaStore.Application.DTOs;
using KodaStore.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KodaStore.Infrastructure.Repositories;

public sealed class EfOrderRepository(AppDbContext db) : IOrderRepository
{
    private readonly AppDbContext _db = db;

    public async Task<OrderDetailResponse?> ConfirmAsync(long id, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == id, cancellationToken);
        if (order is null) return null;

        order.Confirmation();
        await _db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<OrderDetailResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var orderNumber = request.OrderNumer.Trim().ToUpperInvariant();

        if (await _db.Orders.AnyAsync(o => o.OrderNumber == orderNumber, cancellationToken)) throw new ArgumentException("Order Number sudah diguankan");

        var items = request.Items.Select(i => new { Code = i.ProductCode.Trim().ToUpperInvariant(), i.Quantity }).ToArray();

        var codes = items.Select(i => i.Code).Distinct().ToArray();
        var products = await _db.Products.Where(p => codes.Contains(p.ProductCode)).ToDictionaryAsync(p => p.ProductCode, cancellationToken);
        var order = new Order(orderNumber);

        foreach (var item in items)
        {
            if (!products.TryGetValue(item.Code, out var product)) throw new ArgumentException($"Product Code {item.Code} tidak ditemukan.");
        }
        _db.Orders.Add(order);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new Exception("OrderNumber atau kombinasi item sudah digunakan");
        }

        return (await GetByIdAsync(order.OrderId, cancellationToken))!;
    }

    public async Task<OrderDetailResponse?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.AsNoTracking().Include(o => o.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(o => o.OrderId == id, cancellationToken);

        return order is null ? null : MapDetail(order);
    }

    public async Task<IReadOnlyList<OrderSummaryResponse>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.Orders.AsNoTracking().OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderId).Skip((page - 1) * pageSize).Take(pageSize).Select(o => new OrderSummaryResponse(
            o.OrderId, o.OrderNumber, o.OrderDate, o.Status, o.Items.Sum(i => i.Quantity * i.UnitPrice)
        ));

        string sqlQuery = query.ToQueryString();
        Console.WriteLine(sqlQuery);

        return await query.ToListAsync(cancellationToken);
    }

    public static OrderDetailResponse MapDetail(Order order)
    {
        var items = order.Items
       .OrderBy(i => i.OrderItemId)
       .Select(i => new OrderItemResponse(
           i.ProductCode, i.Product.Name, i.Quantity, i.UnitPrice, i.Quantity * i.UnitPrice
       )).ToArray();

        return new OrderDetailResponse(order.OrderId, order.OrderNumber, order.OrderDate, order.Status, items.Sum(i => i.Subtotal), items);
    }
}

