using Microsoft.AspNetCore.Mvc;
using CalculationApi.Models;
using CalculationApi.DTO;
using Microsoft.EntityFrameworkCore;

namespace CalculationApi.Controllers;

[Route("api/[controller]")] // Адрес: /api/calculator
[ApiController]
public class CalculatorController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly int _variant = 1;

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


    [HttpPost("calculate-dto")]
    public async Task<ActionResult<CalculationDto>> CalculateWithDto(CalcRequest request)
    {
        double result = CalculateOperation(request.A, request.B);

        var calculation = new Calculation
        {
            A = request.A,
            B = request.B,
            Result = result,
            CreatedAt = DateTime.Now
        };

        _context.Calculations.Add(calculation);
        await _context.SaveChangesAsync();

        // Возвращаем DTO вместо полной модели
        var dto = new CalculationDto
        {
            A = request.A,
            B = request.B,
            Result = result,
            Operation = GetOperationName()
        };

        return Ok(dto);
    }

    [HttpGet("route/{a}/{b}")]
    public IActionResult CalculateFromRoute(double a, double b)
    {
        double result = CalculateOperation(a, b);

        var dto = new CalculationDto
        {
            A = a,
            B = b,
            Result = result,
            Operation = GetOperationName()
        };

        return Ok(dto);
    }

    [HttpGet("query")]
    public IActionResult CalculateFromQuery([FromQuery] double a, [FromQuery] double b)
    {
        double result = CalculateOperation(a, b);

        var dto = new CalculationDto
        {
            A = a,
            B = b,
            Result = result,
            Operation = GetOperationName()
        };

        return Ok(dto);
    }

    [HttpGet("dto/{id}")]
    public async Task<ActionResult<CalculationDetailsDto>> GetCalculationDto(int id)
    {
        var calc = await _context.Calculations.FindAsync(id);
        if (calc == null)
            return NotFound();

        var dto = new CalculationDetailsDto
        {
            Id = calc.Id,
            A = calc.A,
            B = calc.B,
            Result = calc.Result,
            CreatedAt = calc.CreatedAt,
            Operation = GetOperationName()
        };

        return Ok(dto);
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<CalculationDto>>> GetList()
    {
        var calculations = await _context.Calculations
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CalculationDto
            {
                A = c.A,
                B = c.B,
                Result = c.Result,
                Operation = GetOperationName()
            })
            .ToListAsync();

        return Ok(calculations);
    }

    [HttpGet("filter")]
    public async Task<ActionResult<List<CalculationDto>>> FilterByDate(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var query = _context.Calculations.AsQueryable();

        if (from.HasValue)
            query = query.Where(c => c.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(c => c.CreatedAt <= to.Value);

        var result = await query
            .Select(c => new CalculationDto
            {
                A = c.A,
                B = c.B,
                Result = c.Result,
                Operation = GetOperationName()
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStatistics()
    {
        var totalCount = await _context.Calculations.CountAsync();
        var averageResult = await _context.Calculations.AverageAsync(c => c.Result);
        var lastCalculation = await _context.Calculations
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        // Возвращаем анонимный объект (не DTO, а прямо в ответе)
        return Ok(new
        {
            TotalCalculations = totalCount,
            AverageResult = Math.Round(averageResult, 2),
            LastCalculationDate = lastCalculation?.CreatedAt,
            LastResult = lastCalculation?.Result,
            Operation = GetOperationName()
        });
    }

    // Вспомогательные методы
    private double CalculateOperation(double a, double b)
    {
        return _variant switch
        {
            1 => a + b,
            2 => a * b,
            3 => b == 0 ? throw new DivideByZeroException() : a / b,
            4 => Math.Pow(a, b),
            _ => 0
        };
    }

    private string GetOperationName()
    {
        return _variant switch
        {
            1 => "Сложение",
            2 => "Умножение",
            3 => "Деление",
            4 => "Возведение в степень",
            _ => "Неизвестно"
        };
    }
    [HttpPost]
    public async Task<ActionResult<Calculation>> Create(CreateCalculationDto dto)
    {
        // 1. Валидация (если атрибуты не сработали)
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 2. Вычисление результата согласно варианту
        double result;
        try
        {
            result = CalculateOperation(dto.A, dto.B);
        }
        catch (DivideByZeroException)
        {
            return BadRequest(new { error = "Деление на ноль запрещено" });
        }

        // 3. Создание модели для БД
        var calculation = new Calculation
        {
            A = dto.A,
            B = dto.B,
            Result = result,
            CreatedAt = DateTime.Now
        };

        // 4. Сохранение в БД
        _context.Calculations.Add(calculation);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),           // Имя метода для получения ресурса
            new { id = calculation.Id }, // Параметры маршрута
            calculation                 // Само созданное тело ответа
        );
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCalculationDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { error = "ID в маршруте и теле запроса не совпадают" });

        var calculation = await _context.Calculations.FindAsync(id);
        if (calculation == null)
            return NotFound();

        // Обновляем поля
        calculation.A = dto.A;
        calculation.B = dto.B;
        calculation.Result = CalculateOperation(dto.A, dto.B);
        // CreatedAt не обновляем - оставляем оригинальную дату создания

        await _context.SaveChangesAsync();
        return NoContent();
    }
}