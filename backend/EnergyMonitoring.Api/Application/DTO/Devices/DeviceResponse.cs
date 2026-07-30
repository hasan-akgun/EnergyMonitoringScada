namespace EnergyMonitoring.Api.Application.DTO.Devices;

public sealed record DeviceResponse(
    int Id,
    string Name,
    string SerialNumber,
    string? Description,
    int OrganizationId,
    bool IsActive,
    bool IsDeleted,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
