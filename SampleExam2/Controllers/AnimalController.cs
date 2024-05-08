using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> AddAnimal()
    {
        return Created();
    }
}