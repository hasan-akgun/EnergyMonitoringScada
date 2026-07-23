namespace EnergyMonitoring.Api.Application.DTO.Organizations;

public sealed record OrganizationResponse(
    int Id,
    string Name,
    string? Description,
    int? ParentOrganizationId,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

