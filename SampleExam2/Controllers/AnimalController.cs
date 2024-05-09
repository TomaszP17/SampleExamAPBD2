using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SampleExam2.DTOs;
using SampleExam2.Services;

namespace SampleExam2.Controllers;


[ApiController]
[Route("/api/animals")]
public class AnimalController(IDbService db) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAnimal(int id)
    {
        var result = await db.GetAnimalByIdAsync(id);
        if (result == null) return NotFound($"The animal with id: {id} is not exists");
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddAnimal(AddAnimalDTO animalDto, IValidator<AddAnimalDTO> validator)
    {
        var validate = await validator.ValidateAsync(animalDto);
        if (!validate.IsValid)
        {
            return ValidationProblem();
        }
        
        if (!await db.IsOwnerExists(animalDto.OwnerId))
        {
            return NotFound("Owner does not exists");
        }

        if (animalDto.Procedures != null && animalDto.Procedures.Count > 0 )
        {
            foreach (var procedure in animalDto.Procedures)
            {
                if (!await db.IsProcedureExists(procedure.ProcedureId))
                {
                    return NotFound($"Procedure with that id: {procedure.ProcedureId} does not exists");
                }
            }
        }

        await db.AddAnimalAsync(animalDto);
        return Created($"/api/animals/{animalDto}", animalDto);
    }
}