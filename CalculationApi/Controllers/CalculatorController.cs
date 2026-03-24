using Microsoft.AspNetCore.Mvc;
using CalculationApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CalculationApi.Controllers;

[Route("api/[controller]")] // Адрес: /api/calculator
[ApiController]
public class CalculatorController : ControllerBase
{
    private readonly AppDbContext _context;

    public CalculatorController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<double>> Calculate(CalcRequest request)
    {
        double result = 0;

        result = request.A + request.B; 

        // Создаем запись для БД
        var calculation = new Calculation
        {
            A = request.A,
            B = request.B,
            Result = result,
            CreatedAt = DateTime.Now
        };

        // Сохраняем в базу
        _context.Calculations.Add(calculation);
        await _context.SaveChangesAsync();

        // Возвращаем результат
        return Ok(result);
    }

    // Метод для получения истории (GET: api/calculator)
    [HttpGet]
    public async Task<ActionResult<List<Calculation>>> GetAll()
    {
        return await _context.Calculations.ToListAsync();
    }


[HttpGet("{id}")]
    public async Task<ActionResult<Calculation>> GetById(int id)
    {
        var calculation = await _context.Calculations.FindAsync(id);
        if (calculation == null)
            return NotFound();
        return calculation;
    }

    [HttpGet("add/{a}/{b}")]
    public ActionResult<double> AddFromUrl(double a, double b)
    {
        double result = a + b;
        return Ok(result);
    }

    [HttpGet("calculate-quick")]
    public ActionResult<double> CalculateQuick(double a, double b)
    {
        double result = a + b; 
        return Ok(result);
    }

    // 6. DELETE: удалить запись по ID (НОВЫЙ МЕТОД)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var calculation = await _context.Calculations.FindAsync(id);
        if (calculation == null)
            return NotFound();

        _context.Calculations.Remove(calculation);
        await _context.SaveChangesAsync();

        return NoContent(); 
    }
}