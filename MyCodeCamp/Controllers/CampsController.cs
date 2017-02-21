using System;
using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AutoMapper;
using MyCodeCamp.Models;
using System.Collections.Generic;

namespace MyCodeCamp.Controllers
{
    [Route("api/[controller]")]
    public class CampsController : Controller
    {
        private ILogger<CampsController> _logger;
        private ICampRepository _repo;
        private IMapper _mapper;

        public CampsController(
            ICampRepository repo
            , ILogger<CampsController> logger
            ,IMapper mapper)
        {
            _repo = repo;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("")]
        public IActionResult Get()
        {
            var camps = _repo.GetAllCamps();

            return Ok(_mapper.Map<IEnumerable<CampModel>>(camps));
        }

        [HttpGet("{id}", Name = "CampGet")]
        public IActionResult Get(int id, bool includeSpeakers = false)
        {
            try
            {
                Camp camp = null;

                if (includeSpeakers) camp = _repo.GetCampWithSpeakers(id);
                else camp = _repo.GetCamp(id);

                if (camp == null) return NotFound($"Camp {id} was not found");

                return Ok(_mapper.Map<CampModel>(camp));
            }
            catch
            {
            }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Camp model)
        {
            try
            {
                _logger.LogInformation("Creating a new Code Camp");

                _repo.Add(model);
                if (await _repo.SaveAllAsync()){
                    var newUri = Url.Link("CampGet", new { id = model.Id });
                    return Created(newUri, model);
                }else
                {
                    _logger.LogWarning("Could not save Camp to the database");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Threw exception while saving Camp: {ex}");
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Camp model)
        {
            try
            {
                _logger.LogInformation("Updating an existing Code Camp");

                var oldCamp = _repo.GetCamp(id);
                if (oldCamp == null)
                {

                    return NotFound($"Could not find a camp with in ID of {id}");
                }

                // Map
                oldCamp.Name = model.Name ?? oldCamp.Name;
                oldCamp.Description = model.Description ?? oldCamp.Description;
                oldCamp.Moniker = model.Moniker ?? oldCamp.Moniker;
                oldCamp.Location = model.Location ?? oldCamp.Location;
                oldCamp.Length = model.Length > 0 ? model.Length : oldCamp.Length;
                oldCamp.EventDate = model.EventDate != DateTime.MinValue ? model.EventDate : oldCamp.EventDate;

                if (await _repo.SaveAllAsync())
                {
                    return Ok(oldCamp);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Threw exception while updating Camp: {ex}");
            }

            return BadRequest("Couldn't update Camp");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Deleting a Camp");

                var oldCamp = _repo.GetCamp(id);
                if (oldCamp == null)
                {
                    return NotFound($"Could not find Camp with ID of {id}");
                }

                _repo.Delete(oldCamp);
                if (await _repo.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Threw exception while deleting Camp: {ex}");
            }

            return BadRequest("Could not delete Camp");
        }
    }
}