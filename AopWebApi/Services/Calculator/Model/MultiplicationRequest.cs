using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AopWebApi.Services.Calculator.Model
{
    public class MultiplicationRequest
    {
        public int Multiplicand { get; set; }
        public int Multiplier { get; set; }
    }
}
