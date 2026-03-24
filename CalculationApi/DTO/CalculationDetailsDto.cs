namespace CalculationApi.DTO;

public class CalculationDetailsDto
{
    public int Id { get; set; }
    public double A { get; set; }
    public double B { get; set; }
    public double Result { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Operation { get; set; }
}