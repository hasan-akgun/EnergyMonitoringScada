using EnergyMonitoring.Api.Application.DTO.Devices;
using EnergyMonitoring.Api.Application.DTO.Organizations;
using EnergyMonitoring.Api.Application.Interfaces;
using EnergyMonitoring.Api.Common.Exceptions;
using EnergyMonitoring.Api.Domain.Entities;
using EnergyMonitoring.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnergyMonitoring.Api.Application.Services
{
    public sealed class OrganizationService : IOrganizationService
    {
        private readonly ApplicationDbContext dbContext;

        public OrganizationService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IReadOnlyList<OrganizationResponse>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await this.dbContext.Organizations
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new OrganizationResponse(
                    x.Id,
                    x.Name,
                    x.Description,
                    x.ParentOrganizationId,
                    x.IsActive,
                    x.IsDeleted,
                    x.CreatedAtUtc,
                    x.UpdatedAtUtc))
                .ToListAsync(cancellationToken);
        }
        public async Task<OrganizationResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await this.dbContext.Organizations
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new OrganizationResponse(
                    x.Id,
                    x.Name,
                    x.Description,
                    x.ParentOrganizationId,
                    x.IsActive,
                    x.IsDeleted,
                    x.CreatedAtUtc,
                    x.UpdatedAtUtc))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<OrganizationResponse> CreateAsync(CreateOrganizationRequest request, CancellationToken cancellationToken)
        {
            var organization = new Organization
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                ParentOrganizationId = request.ParentOrganizationId
            };

            this.dbContext.Organizations.Add(organization);

            await this.dbContext.SaveChangesAsync(cancellationToken);

            return new OrganizationResponse(
                organization.Id,
                organization.Name,
                organization.Description,
                organization.ParentOrganizationId,
                organization.IsActive,
                organization.IsDeleted,
                organization.CreatedAtUtc,
                organization.UpdatedAtUtc);
        }

        public async Task<OrganizationResponse?> UpdateAsync(int id, UpdateOrganizationRequest request, CancellationToken cancellationToken)
        {
            var hasAnyChange =
               request.Name is not null ||
               request.Description is not null ||
               request.ClearDescription ||
               request.ParentOrganizationId is not null ||
               request.ClearParentOrganization ||
               request.IsActive is not null;

            if (!hasAnyChange)
            {
                throw new ArgumentException(
                    "Güncellenecek en az bir alan gönderilmelidir.");
            }

            if (request.ClearDescription && request.Description is not null)
            {
                throw new ValidationException(
                    nameof(request.Description),
                    "Description ve ClearDescription aynı anda gönderilemez.");
            }

            if (request.ClearParentOrganization && request.ParentOrganizationId.HasValue)
            {
                throw new ValidationException(
                    nameof(request.ParentOrganizationId),
                    "ParentOrganizationId ve ClearParentOrganization aynı anda gönderilemez.");
            }

            var organization = await this.dbContext.Organizations
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

            if (organization is null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                organization.Name = request.Name.Trim();
            }

            if (request.ClearDescription)
            {
                organization.Description = null;
            }
            else if (!string.IsNullOrWhiteSpace(request.Description))
            {
                organization.Description = request.Description?.Trim();
            }

            if (request.ClearParentOrganization)
            {
                organization.ParentOrganizationId = null;
            }
            else if (request.ParentOrganizationId is int parentId)
            {
                if (parentId == id)
                {
                    throw new ValidationException(
                        nameof(request.ParentOrganizationId),
                        "Organizasyon kendisinin üst organizasyonu olamaz.");
                }

                var parentExists = await this.dbContext.Organizations
                    .AnyAsync(
                        x => x.Id == parentId,
                        cancellationToken);

                if (!parentExists)
                {
                    throw new KeyNotFoundException(
                        "Belirtilen üst organizasyon bulunamadı.");
                }

                if (await this.WouldCreateCycleAsync(id, parentId, cancellationToken))
                {
                    throw new ValidationException(
                        nameof(request.ParentOrganizationId),
                        "Organizasyon kendi alt organizasyonlarından birinin altına taşınamaz.");
                }

                organization.ParentOrganizationId = parentId;
            }

            if (request.IsActive is bool isActive)
            {
                organization.IsActive = isActive;
            }
            await this.dbContext.SaveChangesAsync(cancellationToken);

            return new OrganizationResponse(
                organization.Id,
                organization.Name,
                organization.Description,
                organization.ParentOrganizationId,
                organization.IsActive,
                organization.IsDeleted,
                organization.CreatedAtUtc,
                organization.UpdatedAtUtc);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var affectedRows = await dbContext.Organizations
                .Where(x => x.Id == id && !x.IsDeleted)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.IsDeleted, true)
                        .SetProperty(x => x.UpdatedAtUtc, DateTime.UtcNow),
                    cancellationToken);

            return affectedRows > 0;
        }

        public async Task<IReadOnlyList<OrganizationTreeResponse>> GetTreeAsync(int? id, CancellationToken cancellationToken)
        {
            var organizations = await this.dbContext.Organizations
            .AsNoTracking()
            .Include(x => x.Devices)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

            return BuildTree(organizations, id);
        }

        private static List<OrganizationTreeResponse> BuildTree(List<Organization> organizations, int? parentId)
        {
            return organizations
                .Where(x => x.ParentOrganizationId == parentId
                    && x.IsActive
                    && !x.IsDeleted)
                .Select(x => new OrganizationTreeResponse(
                    x.Id,
                    x.Name,
                    x.IsActive,
                    x.Devices
                        .Where(device => device.IsActive && !device.IsDeleted)
                        .OrderBy(device => device.Name)
                        .Select(device => new DeviceTreeResponse(
                            device.Id,
                            device.Name,
                            device.SerialNumber,
                            device.IsActive))
                        .ToList(),
                    BuildTree(organizations, x.Id)))
                .ToList();
        }

        private async Task<bool> WouldCreateCycleAsync(int organizationId, int newParentId, CancellationToken cancellationToken)
        {
            int? currentId = newParentId;
            var visitedIds = new HashSet<int>();

            while (currentId.HasValue)
            {
                if (currentId.Value == organizationId)
                {
                    return true;
                }

                if (!visitedIds.Add(currentId.Value))
                {
                    return true;
                }

                currentId = await this.dbContext.Organizations
                    .Where(x => x.Id == currentId.Value)
                    .Select(x => x.ParentOrganizationId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return false;
        }
    }
}
