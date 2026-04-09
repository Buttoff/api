using System;

namespace CalcClient.Models;

public class Calculation
{
    public int Id { get; set; }
    public double A { get; set; }
    public double B { get; set; }
    public double Result { get; set; }
    public DateTime CreatedAt { get; set; }
}