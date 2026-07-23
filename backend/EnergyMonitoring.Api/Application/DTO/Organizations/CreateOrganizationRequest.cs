namespace EnergyMonitoring.Api.Application.DTO.Organizations;

public sealed record CreateOrganizationRequest(
    string Name,
    string? Description,
    int? ParentOrganizationId);