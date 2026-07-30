namespace EnergyMonitoring.Api.Application.DTO.Devices;

public sealed record UpdateDeviceRequest
(
    string? Name,

    string? SerialNumber,

    string? Description,

    bool ClearDescription,

    int? OrganizationId,

    bool? IsActive
);
