namespace EnergyMonitoring.Api.Domain.Entities
{
    public sealed class Device : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;
    }
}
