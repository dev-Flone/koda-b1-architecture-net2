using KodaStore.Application.Common;
using KodaStore.Application.DTOs;
using KodaStore.Application.Repositories;
using KodaStore.Domain.Exceptions;

namespace KodaStore.Application.Services;

public class OrderService(IOrderRepository repository) : IOrderService
{
    private readonly IOrderRepository _repository = repository;
    public async Task<Result<OrderDetailResponse>> ConfirmAsync(long id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _repository.ConfirmAsync(id, cancellationToken);

            if (order is null) return Result<OrderDetailResponse>.Failure(ResultErrorType.NotFound, "Order tidak ditemukan.");

            return Result<OrderDetailResponse>.Success(order);
        }
        catch (OrderStateException e)
        {
            return Result<OrderDetailResponse>.Failure(ResultErrorType.Conflict, e.Message);
        }
    }

    public Task<OrderDetailResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.Items is null || request.Items.Count == 0) throw new ArgumentException("Order minimal 1 item");

        if (request.Items.Any(i => string.IsNullOrWhiteSpace(i.ProductCode) || i.Quantity <= 0)) throw new ArgumentException("ProductCode dan Quantity harus valid.");

        var codes = request.Items.Select(i => i.ProductCode.Trim().ToUpperInvariant().ToArray());

        return _repository.CreateAsync(request, cancellationToken);
    }

    public Task<OrderDetailResponse?> GetByIdAsync(long id, CancellationToken cancellationToken) => _repository.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<OrderSummaryResponse>> GetPageAsync(int page, int size, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        size = Math.Clamp(size, 1, 50);

        if (page > 10000) throw new ArgumentException("Page terlalu besar untuk pagination");

        return _repository.GetPageAsync(page, size, cancellationToken);
    }
}
