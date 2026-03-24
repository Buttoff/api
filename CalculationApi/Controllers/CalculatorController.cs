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
}