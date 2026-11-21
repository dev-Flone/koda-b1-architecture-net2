using KodaStore.Application.DTOs;

namespace KodaStore.Application.Services;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CustomerDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CustomerResponse?> CreateAsync(CustomerRequest request, CancellationToken cancellationToken = default);
    Task<CustomerResponse?> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

}
