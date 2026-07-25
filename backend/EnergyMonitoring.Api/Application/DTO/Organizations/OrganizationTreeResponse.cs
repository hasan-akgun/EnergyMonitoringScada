namespace EnergyMonitoring.Api.Application.DTO.Organizations
{
    public sealed record OrganizationTreeResponse(
        int Id,
        string Name,
        bool IsActive,
        IReadOnlyList<OrganizationTreeResponse> Children
        );
}
