using EnergyMonitoring.Api.Application.DTO.Devices;
using EnergyMonitoring.Api.Application.DTO.Organizations;
using EnergyMonitoring.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMonitoring.Api.Controllers
{
    [ApiController]
    [Route("api/devices")]
    public sealed class DeviceController : ControllerBase
    {
        private readonly IDeviceService deviceService;

        public DeviceController(IDeviceService deviceService)
        {
            this.deviceService = deviceService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<DeviceResponse>>> GetAll(CancellationToken cancellationToken)
        {
            var devices = await this.deviceService.GetAllAsync(cancellationToken);
            return Ok(devices);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<DeviceResponse>> GetById(int id, CancellationToken cancellationToken)
        {
            var device = await this.deviceService.GetByIdAsync(id, cancellationToken);

            if (device is null)
            {
                return NotFound(new
                {
                    message = "Device not found."
                });
            }

            return Ok(device);
        }

        [HttpPost]
        public async Task<ActionResult<DeviceResponse>> Create(CreateDeviceRequest request, CancellationToken cancellationToken)
        {
            var device = await this.deviceService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = device.Id },
                device);
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult<DeviceResponse>> Update(int id, UpdateDeviceRequest request, CancellationToken cancellationToken)
        {
            var device = await this.deviceService.UpdateAsync(id, request, cancellationToken);

            if (device is null)
            {
                return NotFound(new
                {
                    message = "Device not found."
                });
            }

            return Ok(device);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var isDeleted = await this.deviceService.DeleteAsync(id, cancellationToken);
            return isDeleted ? NoContent() : NotFound();
        }
    }
}
