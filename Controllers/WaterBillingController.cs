using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Services.WaterBills;
using Microsoft.AspNetCore.Mvc;

namespace KejaHUnt_PropertiesAPI.Controllers;

[ApiController]
[Route("api/water-billing")]
public class WaterBillingController : ControllerBase
{
    private readonly IWaterBillingService _service;

    public WaterBillingController(IWaterBillingService service) => _service = service;

    [HttpPost("rate")]
    public async Task<IActionResult> SetRate([FromBody] SetWaterRateDto dto)
    {
        var rate = await _service.SetPropertyRateAsync(dto);
        return Ok(rate);
    }

    [HttpGet("rate/{propertyId}/history")]
    public async Task<IActionResult> GetRateHistory(long propertyId)
    {
        var history = await _service.GetRateHistoryAsync(propertyId);
        return Ok(history);
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateWaterBillsRequestDto request)
    {
        var result = await _service.GenerateMonthlyBillsAsync(request);
        return Ok(result);
    }

    [HttpGet("property/{propertyId}/{year}/{month}")]
    public async Task<IActionResult> GetPropertyBills(long propertyId, int year, int month)
    {
        var bills = await _service.GetPropertyBillsAsync(propertyId, year, month);
        return Ok(bills);
    }

    [HttpGet("unit/{unitId}/history")]
    public async Task<IActionResult> GetUnitHistory(long unitId)
    {
        var history = await _service.GetUnitReadingHistoryAsync(unitId);
        return Ok(history);
    }
}