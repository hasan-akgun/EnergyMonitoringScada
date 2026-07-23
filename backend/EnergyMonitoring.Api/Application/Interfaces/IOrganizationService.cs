using EnergyMonitoring.Api.Application.DTO.Organizations;

namespace EnergyMonitoring.Api.Application.Interfaces
{
    public interface IOrganizationService
    {
        Task<IReadOnlyList<OrganizationResponse>> GetAllAsync(
            CancellationToken cancellationToken);

        Task<OrganizationResponse?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken);

        Task<OrganizationResponse> CreateAsync(
            CreateOrganizationRequest request,
            CancellationToken cancellationToken);

        Task<OrganizationResponse?> UpdateAsync(
            int id,
            UpdateOrganizationRequest request,
            CancellationToken cancellationToken);
    }
}
