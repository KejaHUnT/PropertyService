using AutoMapper;
using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Models.Enums;
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace KejaHUnt_PropertiesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitController : ControllerBase
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IImageRepository _imageRepository;
        private readonly IMapper _mapper;

        public UnitController(IUnitRepository unitRepository, IImageRepository imageRepository, IMapper mapper)
        {
            _unitRepository = unitRepository;
            _imageRepository = imageRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUnit([FromForm] CreateUnitsJsonDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            List<CreateUnitRequestDto> unitDtos;
            try
            {
                unitDtos = JsonConvert.DeserializeObject<List<CreateUnitRequestDto>>(request.Units);
            }
            catch (Exception)
            {
                return BadRequest("Invalid units JSON format.");
            }

            if (request.ImageFiles == null || request.ImageFiles.Count != unitDtos.Count)
            {
                return BadRequest("Number of images must match number of units.");
            }

            var unitsToSave = new List<Unit>();

            for (int i = 0; i < unitDtos.Count; i++)
            {
                var unitDto = unitDtos[i];
                var unitEntity = _mapper.Map<Unit>(unitDto);

                var image = request.ImageFiles[i];
                if (image != null)
                {
                    unitEntity.ImageUrl = await _imageRepository.Upload(image, "units");
                }

                await _unitRepository.CreateUnitAsync(unitEntity);
                unitsToSave.Add(unitEntity);
            }

            return Ok(_mapper.Map<List<UnitDto>>(unitsToSave));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var units = await _unitRepository.GetAllAsync();
            return Ok(_mapper.Map<List<UnitDto>>(units));
        }

        [HttpGet]
        [Route("{id:long}")]
        public async Task<IActionResult> GetUnitByIdAsync([FromRoute] long id)
        {
            var unit = await _unitRepository.GetUnitByIdAsync(id);
            if (unit == null) return NotFound();
            return Ok(_mapper.Map<UnitDto>(unit));
        }

        [HttpGet]
        [Route("property/{propertyId:long}")]
        public async Task<IActionResult> GetUnitByPropertyIdAsync([FromRoute] long propertyId)
        {
            var units = await _unitRepository.GetUnitsByPropertyIdAsync(propertyId);
            if (units == null || !units.Any()) return NotFound();
            return Ok(_mapper.Map<List<UnitDto>>(units));
        }

        [HttpPut]
        [Route("{id:long}")]
        public async Task<IActionResult> UpdateUnits([FromRoute] long id, [FromForm] UpdateUnitJsonDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedUnits = new List<UnitDto>();

            List<UpdateUnitRequestDto> unitDtos;
            try
            {
                unitDtos = JsonConvert.DeserializeObject<List<UpdateUnitRequestDto>>(request.Units);
            }
            catch (Exception)
            {
                return BadRequest("Invalid units JSON format.");
            }

            foreach (var unit in unitDtos)
            {
                var unitDto = _mapper.Map<Unit>(unit);
                unitDto.Id = id;

                if (request.ImageFile != null)
                {
                    unitDto.ImageUrl = await _imageRepository.Edit(unitDto.ImageUrl, request.ImageFile, "units");
                }

                await _unitRepository.UpdateAsync(unitDto);
                updatedUnits.Add(_mapper.Map<UnitDto>(unitDto));
            }

            return Ok(_mapper.Map<List<UnitDto>>(updatedUnits));
        }

        [HttpPut]
        [Route("status")]
        public async Task<IActionResult> UpdateUnitStatus([FromBody] UnitStatusDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedUnit = await _unitRepository.UpdateUnitStatusAsync(request);
            if (updatedUnit == null)
                return NotFound($"No unit found with ID {request.UnitId}");

            return Ok(_mapper.Map<UnitDto>(updatedUnit));
        }

        // NEW: PUT: api/unit/{unitId}/status (uses enum)
        [HttpPut]
        [Route("{unitId:long}/status")]
        public async Task<IActionResult> UpdateUnitStatusDirect([FromRoute] long unitId, [FromBody] UnitStatus status)
        {
            var unit = await _unitRepository.GetUnitByIdAsync(unitId);
            if (unit == null)
                return NotFound($"Unit with ID {unitId} not found.");

            unit.Status = status;
            await _unitRepository.UpdateAsync(unit);   // Use existing UpdateAsync

            return Ok(_mapper.Map<UnitDto>(unit));
        }

        [HttpDelete]
        [Route("{id:long}")]
        public async Task<IActionResult> DeleteUnitById([FromRoute] long id)
        {
            var existingUnit = await _unitRepository.GetUnitByIdAsync(id);
            if (existingUnit == null)
                return NotFound($"Unit with ID {id} not found.");

            await _unitRepository.DeleteAync(id);   // Changed to DeleteAync
            return Ok(_mapper.Map<UnitDto>(existingUnit));
        }
    }
}