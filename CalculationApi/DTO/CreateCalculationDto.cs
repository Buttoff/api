using System.ComponentModel.DataAnnotations;

namespace CalculationApi.DTO;

public class CreateCalculationDto
{
    [Required]
    [Range(-1000, 1000, ErrorMessage = "Число A должно быть в диапазоне от -1000 до 1000")]
    public double A { get; set; }

    [Required]
    [Range(-1000, 1000, ErrorMessage = "Число B должно быть в диапазоне от -1000 до 1000")]
    public double B { get; set; }
}