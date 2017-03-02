using System;
using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AutoMapper;
using MyCodeCamp.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Cors;
using MyCodeCamp.Filters;
using Microsoft.AspNetCore.Authorization;

namespace MyCodeCamp.Controllers
{
    [Authorize]
    [EnableCors("AnyGET")]
    [Route("api/[controller]")]
    [ValidateModel]
    public class CampsController : BaseController
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

        [HttpGet("{moniker}", Name = "CampGet")]
        public IActionResult Get(string moniker, bool includeSpeakers = false)
        {
            try
            {
                Camp camp = null;

                if (includeSpeakers) camp = _repo.GetCampByMonikerWithSpeakers(moniker);
                else camp = _repo.GetCampByMoniker(moniker);

                if (camp == null) return NotFound($"Camp {moniker} was not found");

                return Ok(_mapper.Map<CampModel>(camp));
            }
            catch
            {
            }

            return BadRequest();
        }

        [EnableCors("Wildermuth")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CampModel model)
        {
            try
            {
                _logger.LogInformation("Creating a new Code Camp");

                var camp = _mapper.Map<Camp>(model);

                _repo.Add(camp);

                if (await _repo.SaveAllAsync()){
                    var newUri = Url.Link("CampGet", new { moniker = model.Moniker });
                    return Created(newUri, _mapper.Map<CampModel>(camp));
                }else
                {
                    var msg = "Could not save Camp to the database";
                    _logger.LogError(msg);
                    return StatusCode(500, msg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Threw exception while saving Camp: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{moniker}")]
        public async Task<IActionResult> Put(string moniker, [FromBody] CampModel model)
        {
            try
            {
                _logger.LogInformation("Updating an existing Code Camp");

                var oldCamp = _repo.GetCampByMoniker(moniker);
                if (oldCamp == null)
                {
                    return NotFound($"Could not find a camp with moniker of {moniker}");
                }

                _mapper.Map(model, oldCamp);

                if (await _repo.SaveAllAsync())
                {
                    return Ok(_mapper.Map<CampModel>(oldCamp));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Threw exception while updating Camp: {ex}");
            }

            return BadRequest("Couldn't update Camp");
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                _logger.LogInformation("Deleting a Camp");

                var oldCamp = _repo.GetCampByMoniker(moniker);
                if (oldCamp == null)
                {
                    return NotFound($"Could not find Camp with moniker of {moniker}");
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