using AutoMapper;
using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Repositories.Implementation;
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using Microsoft.AspNetCore.Http;
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

        // POST: {apibaseurl}/api/unit
        [HttpPost]
        public async Task<IActionResult> CreateUnit([FromForm] CreateUnitsJsonDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Deserialize the units JSON string into a list of CreateUnitRequestDto
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

            // 2. Loop through units and images in parallel
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

        // GET: {apibaseurl}/api/unit
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var units = await _unitRepository.GetAllAsync();

            return Ok(_mapper.Map<List<UnitDto>>(units));
        }

        // GET: {apibaseurl}/api/unit/{id}
        [HttpGet]
        [Route("{id:long}")]
        public async Task<IActionResult> GetUnitByIdAsync([FromRoute] long id)
        {
            var unit = await _unitRepository.GetUnitByIdAsync(id);

            if (unit == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<UnitDto>(unit));
        }

        // GET: {apibaseurl}/api/Unit/property/{propertyId}
        [HttpGet]
        [Route("property/{propertyId:long}")]
        public async Task<IActionResult> GetUnitByPropertyIdAsync([FromRoute] long propertyId)
        {
            var unit = await _unitRepository.GetUnitsByPropertyIdAsync(propertyId);

            if (unit == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<List<UnitDto>>(unit));
        }

        // PUT: {apibaseurl}/api/unit/{id}
        [HttpPut]
        [Route("{id:long}")]
        public async Task<IActionResult> UpdateUnits([FromRoute] long id, [FromForm] UpdateUnitJsonDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedUnits = new List<UnitDto>();

            // Deserialize the units JSON string into a list of UpdateUnitRequestDto
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
                // Map the unit DTO to the domain model
                var unitDto = _mapper.Map<Unit>(unit);
                unitDto.Id = id;

                // Handle image update if a file is included
                if (request.ImageFile != null)
                {
                    unitDto.ImageUrl = await _imageRepository.Edit(unitDto.ImageUrl, request.ImageFile, "units");
                }

                // Save to DB
                await _unitRepository.UpdateAsync(unitDto);

                // Add the updated unit to the response list
                updatedUnits.Add(_mapper.Map<UnitDto>(unitDto));
            }

            return Ok(_mapper.Map<List<UnitDto>>(updatedUnits)); ;
        }

        // PUT: api/Units/status
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



        // DELETE: {apibaseurl}/api/unit/{id}
        [HttpDelete]
        [Route("{id:long}")]
        public async Task<IActionResult> DeleteUnitById([FromRoute] long id)
        {
            // 1. Get the unit
            var existingUnit = await _unitRepository.GetUnitByIdAsync(id);
            if (existingUnit == null)
                return NotFound($"Unit with ID {id} not found.");

            // 2. Delete associated image if present
            //if (existingUnit.DocumentId != null && existingUnit.DocumentId != Guid.Empty)
            //{
            //    await _imageRepository.Delete(existingUnit.DocumentId.Value);
            //}

            // 3. Delete unit
            await _unitRepository.DeleteAync(id);

            //return deleted unit
            return Ok(_mapper.Map<UnitDto>(existingUnit));
        }

    }
}
