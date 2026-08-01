using EnergyMonitoring.Api.Application.DTO.Devices;

namespace EnergyMonitoring.Api.Application.DTO.Organizations
{
    public sealed record OrganizationTreeResponse(
        int Id,
        string Name,
        bool IsActive,
        IReadOnlyList<DeviceTreeResponse> Devices,
        IReadOnlyList<OrganizationTreeResponse> Children
        );
}
