using EnergyMonitoring.Api.Application.DTO.Organizations;
using EnergyMonitoring.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMonitoring.Api.Controllers
{
    [ApiController]
    [Route("api/organization")]
    public sealed class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService organizationService;

        public OrganizationController (IOrganizationService organizationService)
        {
            this.organizationService = organizationService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrganizationResponse>>> GetAll(
        CancellationToken cancellationToken)
        {
            var organizations = await this.organizationService.GetAllAsync(cancellationToken);
            return Ok(organizations);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrganizationResponse>> GetById(int id, CancellationToken cancellationToken)
        {
            var organization = await this.organizationService.GetByIdAsync(id, cancellationToken);

            if (organization is null)
            {
                return NotFound(new
                {
                    message = "Organization not found."
                });
            }

            return Ok(organization);
        }

        [HttpPost]
        public async Task<ActionResult<OrganizationResponse>> Create( CreateOrganizationRequest request, CancellationToken cancellationToken)
        {
            var organization = await this.organizationService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = organization.Id },
                organization);
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult<OrganizationResponse>> Update(int id, UpdateOrganizationRequest request, CancellationToken cancellationToken)
        {
            var organization = await this.organizationService.UpdateAsync(id, request, cancellationToken);

            if (organization is null)
            {
                return NotFound(new
                {
                    message = "Organization not found."
                });
            }

            return Ok(organization);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var isDeleted = await this.organizationService.DeleteAsync(id, cancellationToken);
            return isDeleted ? NoContent() : NotFound();
        }

        [HttpGet("tree")]
        public async Task<ActionResult<IReadOnlyList<OrganizationTreeResponse>>> GetTreeAsync(CancellationToken cancellationToken)
        {
            var tree = await this.organizationService.GetTreeAsync(cancellationToken);
            return Ok(tree);
        }
    }
}
