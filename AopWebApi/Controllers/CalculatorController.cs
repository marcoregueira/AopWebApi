using AopWebApi.Services;
using AopWebApi.Services.Calculator.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AopWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculatorController : ControllerBase
    {
        private readonly ICalculatorService _calculator;
        private readonly ILogger<CalculatorController> _logger;

        public CalculatorController(
            ICalculatorService calculator,
            ILogger<CalculatorController> logger)
        {
            _calculator = calculator;
            _logger = logger;
        }

        [HttpPost("division")]
        public async Task<DivisionResponse> Division([FromBody] DivisionRequest request)
        {
            return await _calculator.Quotient(request.Dividend, request.Divisor);
        }
    }
}
