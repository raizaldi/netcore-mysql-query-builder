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

        var builder = new QueryBuilder()
            .Table("Products p")
            .Limit(10)
            .Offset(0);

            var (sql, param) = builder.BuildSelect();
            var products = await _koneksi.SqlDynamicQuery<Product>(sql, param);

            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex);
        }
    }

}