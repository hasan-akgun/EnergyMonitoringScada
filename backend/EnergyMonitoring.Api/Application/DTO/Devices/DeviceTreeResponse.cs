namespace EnergyMonitoring.Api.Application.DTO.Devices
{
    public sealed record DeviceTreeResponse(
        int Id,
        string Name,
        string SerialNumber,
        bool IsActive
        );
}
