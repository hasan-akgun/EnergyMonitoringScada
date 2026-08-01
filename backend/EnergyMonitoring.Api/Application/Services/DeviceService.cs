using EnergyMonitoring.Api.Application.DTO.Devices;
using EnergyMonitoring.Api.Application.Interfaces;
using EnergyMonitoring.Api.Common.Exceptions;
using EnergyMonitoring.Api.Domain.Entities;
using EnergyMonitoring.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnergyMonitoring.Api.Application.Services
{
    public sealed class DeviceService : IDeviceService
    {
        private readonly ApplicationDbContext dbContext;

        public DeviceService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IReadOnlyList<DeviceResponse>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await this.dbContext.Devices
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new DeviceResponse(
                    x.Id,
                    x.Name,
                    x.SerialNumber,
                    x.Description,
                    x.OrganizationId,
                    x.IsActive,
                    x.IsDeleted,
                    x.CreatedAtUtc,
                    x.UpdatedAtUtc))
                .Where(x => x.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<DeviceResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await this.dbContext.Devices
                .AsNoTracking()
                .Select(x => new DeviceResponse(
                    x.Id,
                    x.Name,
                    x.SerialNumber,
                    x.Description,
                    x.OrganizationId,
                    x.IsActive,
                    x.IsDeleted,
                    x.CreatedAtUtc,
                    x.UpdatedAtUtc))
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<DeviceResponse> CreateAsync(CreateDeviceRequest request, CancellationToken cancellationToken)
        {
            var organizationExists = await this.dbContext.Organizations
                .AnyAsync(organization => organization.Id == request.OrganizationId
                    && organization.IsActive
                    && !organization.IsDeleted, cancellationToken);

            if (!organizationExists)
            {
                throw new KeyNotFoundException($"Organization with id {request.OrganizationId} was not found.");
            }

            var device = new Device
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                SerialNumber = request.SerialNumber.Trim(),
                OrganizationId = request.OrganizationId
            };

            this.dbContext.Devices.Add(device);
            await this.dbContext.SaveChangesAsync(cancellationToken);

            return new DeviceResponse(
                device.Id,
                device.Name,
                device.SerialNumber,
                device.Description,
                device.OrganizationId,
                device.IsActive,
                device.IsDeleted,
                device.CreatedAtUtc,
                device.UpdatedAtUtc
                );
        }

        public async Task<DeviceResponse?> UpdateAsync(int id, UpdateDeviceRequest request, CancellationToken cancellationToken)
        {
            var hasAnyChange =
               request.Name is not null ||
               request.Description is not null ||
               request.ClearDescription ||
               request.OrganizationId is not null ||
               request.IsActive is not null;

            if (!hasAnyChange)
            {
                throw new ArgumentException(
                    "Güncellenecek en az bir alan gönderilmelidir.");
            }

            if (request.ClearDescription && request.Description is not null)
            {
                throw new ValidationException(nameof(request.Description), "Description ve ClearDescription aynı anda gönderilemez.");
            }

            var organizationExists = await this.dbContext.Organizations
                .AnyAsync(organization => organization.Id == request.OrganizationId
                    && organization.IsActive
                    && !organization.IsDeleted, cancellationToken);

            if (!organizationExists)
            {
                throw new KeyNotFoundException($"Organization with id {request.OrganizationId} was not found.");
            }

            var device = await this.dbContext.Devices
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

            if (device is null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                device.Name = request.Name.Trim();
            }

            if (request.ClearDescription)
            {
                device.Description = null;
            }
            else if (!string.IsNullOrWhiteSpace(request.Description))
            {
                device.Description = request.Description?.Trim();
            }

            if (request.IsActive is bool isActive)
            {
                device.IsActive = isActive;
            }

            await this.dbContext.SaveChangesAsync(cancellationToken);

            return new DeviceResponse(
                device.Id,
                device.Name,
                device.SerialNumber,
                device.Description,
                device.OrganizationId,
                device.IsActive,
                device.IsDeleted,
                device.CreatedAtUtc,
                device.UpdatedAtUtc
                );
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var affectedRows = await dbContext.Devices
                .Where(x => x.Id == id && !x.IsDeleted)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.IsDeleted, true)
                        .SetProperty(x => x.UpdatedAtUtc, DateTime.UtcNow),
                    cancellationToken);

            return affectedRows > 0;
        }
    }
}
