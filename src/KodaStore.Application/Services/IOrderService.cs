using KodaStore.Application.Common;
using KodaStore.Application.DTOs;

namespace KodaStore.Application.Services;

public interface IOrderService
{
    Task<IReadOnlyList<OrderSummaryResponse>> GetPageAsync(int page, int size, CancellationToken cancellationToken);
    Task<OrderDetailResponse?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<OrderDetailResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<Result<OrderDetailResponse>> ConfirmAsync(long id, CancellationToken cancellationToken);
}
