using KodaStore.Application.DTOs;

namespace KodaStore.Application.Repositories;

public interface IOrderRepository
{
    Task<IReadOnlyList<OrderSummaryResponse>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<OrderDetailResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<OrderDetailResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderDetailResponse?> ConfirmAsync(long id, CancellationToken cancellationToken = default);
}
