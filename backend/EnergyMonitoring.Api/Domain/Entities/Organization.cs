namespace EnergyMonitoring.Api.Domain.Entities
{
    public sealed class Organization : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentOrganizationId { get; set; }
        public Organization? ParentOrganization { get; set; }
        public ICollection<Organization> Children { get; set; } = new List<Organization>();
        public ICollection<Device> Devices { get; set; } = new List<Device>();
    }
}
