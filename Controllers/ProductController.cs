using Dapper;
using Latihan_dotnet.Data;
using Latihan_dotnet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Latihan_dotnet.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly Koneksi _koneksi;

    public ProductController(Koneksi koneksi)
    {
        _koneksi = koneksi;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        try
        {

            var builder = new QueryBuilder<Product>(_koneksi, "p");

            var result = await builder
                .Select(p => new { p.Id, p.Name, p.Price, p.Stock })
                .OrderBy("Name", descending: true)
                .Limit(10)
                .Offset(0)
                .BuildSelect();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex);
        }
    }

}