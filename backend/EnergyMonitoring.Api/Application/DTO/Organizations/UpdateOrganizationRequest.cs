namespace EnergyMonitoring.Api.Application.DTO.Organizations;

public sealed record UpdateOrganizationRequest
(
    string? Name,

    string? Description,

    bool ClearDescription,

    int? ParentOrganizationId,

    bool ClearParentOrganization,

    bool? IsActive
);