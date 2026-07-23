using EnergyMonitoring.Api.Application.DTO.Organizations;
using EnergyMonitoring.Api.Application.Interfaces;
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
                throw new ArgumentException(
                    "Description ve ClearDescription aynı anda gönderilemez.");
            }

            if (request.ClearParentOrganization && request.ParentOrganizationId.HasValue)
            {
                throw new ArgumentException(
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
            else if (request.ParentOrganizationId is not null)
            {
                var parentId = request.ParentOrganizationId;

                if (parentId == id)
                {
                    throw new ArgumentException(
                        "Organizasyon kendisinin üst organizasyonu olamaz.");
                }

                var parentExists = await this.dbContext.Organizations
                    .AnyAsync(
                        x => x.Id == parentId,
                        cancellationToken);

                if (!parentExists)
                {
                    throw new ArgumentException(
                        "Belirtilen üst organizasyon bulunamadı.");
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
                organization.CreatedAtUtc,
                organization.UpdatedAtUtc);
        }
    }
}
