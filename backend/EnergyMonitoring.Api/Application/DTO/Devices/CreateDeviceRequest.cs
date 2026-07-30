namespace EnergyMonitoring.Api.Application.DTO.Devices;

public sealed record CreateDeviceRequest(
    string Name,
    string SerialNumber,
    string? Description,
    int OrganizationId);
