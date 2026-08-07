using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Services.Invoices;
using Microsoft.AspNetCore.Mvc;

namespace KejaHUnt_PropertiesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(IInvoiceService invoiceService, ILogger<InvoiceController> logger)
        {
            _invoiceService = invoiceService;
            _logger = logger;
        }

        // GENERATE (OR UPDATE) INVOICE — called when a manager updates a unit's water bill
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] CreateInvoiceDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Invalid request data");

                if (dto.WaterBillAmount < 0)
                    return BadRequest("Water bill amount cannot be negative");

                var result = await _invoiceService.GenerateInvoiceAsync(dto);

                return Ok(new
                {
                    success = true,
                    message = "Invoice generated successfully",
                    data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error generating invoice",
                    error = ex.Message
                });
            }
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _invoiceService.GetAllAsync();

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        // GET BY ID
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _invoiceService.GetByIdAsync(id);

            if (result == null)
                return NotFound($"Invoice with ID {id} not found");

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        // GET BY PROPERTY
        [HttpGet("property/{propertyId:long}")]
        public async Task<IActionResult> GetByProperty(long propertyId)
        {
            var result = await _invoiceService.GetByPropertyIdAsync(propertyId);

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        // GET BY UNIT
        [HttpGet("unit/{unitId:long}")]
        public async Task<IActionResult> GetByUnit(long unitId)
        {
            var result = await _invoiceService.GetByUnitIdAsync(unitId);

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        // GET BY TENANT
        [HttpGet("tenant/{tenantId:long}")]
        public async Task<IActionResult> GetByTenant(long tenantId)
        {
            var result = await _invoiceService.GetByTenantIdAsync(tenantId);

            return Ok(new
            {
                success = true,
                data = result
            });
        }
    }
}