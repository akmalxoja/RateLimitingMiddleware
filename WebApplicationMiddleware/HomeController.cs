using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    private static readonly List<string> Items = new List<string> { "Item1", "Item2", "Item3" };

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(Items);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        if (id < 0 || id >= Items.Count)
        {
            return NotFound(new { Message = $"Item with id {id} not found." });
        }

        var item = Items[id];
        Items.RemoveAt(id);
        return Ok(new { Message = $"Item '{item}' with id {id} has been deleted." });
    }
}
