using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using EfCoreUnitOfWork.Entities;
using EfCoreUnitOfWork.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EfCoreUnitOfWork.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonsController : ControllerBase
    {
        private readonly IPersonService _personService;

        public PersonsController(IPersonService personService)
        {
            _personService = personService;
        }

        [HttpGet("api/person/{id}")]
        [SwaggerOperation(
            Summary = "Get a person",
            Description = "Get a person",
            OperationId = "Persons_Get",
            Tags = new[] { "PersonsEndpoint" })]
        public async Task<ActionResult<PersonEntity>> GetAsync([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            Result<PersonEntity> result = await _personService.GetAsync(id, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPost("api/person")]
        [SwaggerOperation(
            Summary = "Create a person",
            Description = "Create a person",
            OperationId = "Persons_Create",
            Tags = new[] { "PersonsEndpoint" })]
        public async Task<ActionResult<PersonEntity>> Create([FromBody] string name, CancellationToken cancellationToken = default)
        {
            Result<PersonEntity> result = await _personService.AddAsync(name, cancellationToken);
            return result.ToActionResult(this);
        }
    }
}
