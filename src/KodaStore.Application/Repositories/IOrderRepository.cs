using KodaStore.Application.DTOs;

namespace KodaStore.Application.Repositories;

public interface IOrderRepository
{
    Task<IReadOnlyList<OrderSummaryResponse>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<OrderDetailResponse?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<OrderDetailResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<OrderDetailResponse?> ConfirmAsync(long id, CancellationToken cancellationToken);
}
