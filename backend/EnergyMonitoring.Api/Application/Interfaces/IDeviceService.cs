
using EnergyMonitoring.Api.Application.DTO.Devices;

namespace EnergyMonitoring.Api.Application.Interfaces
{
    public interface IDeviceService
    {
        Task<IReadOnlyList<DeviceResponse>> GetAllAsync(
            CancellationToken cancellationToken);

        Task<DeviceResponse?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken);

        Task<DeviceResponse> CreateAsync(
            CreateDeviceRequest request,
            CancellationToken cancellationToken);

        Task<DeviceResponse?> UpdateAsync(
            int id,
            UpdateDeviceRequest request,
            CancellationToken cancellationToken);

        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken);
    }
}
