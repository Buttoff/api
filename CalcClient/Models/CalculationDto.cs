using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcClient.Models
{
    public class CalculationDto
    {
        public double A { get; set; }
        public double B { get; set; }
        public double Result { get; set; }
        public string Operation { get; set; }
    }
}
