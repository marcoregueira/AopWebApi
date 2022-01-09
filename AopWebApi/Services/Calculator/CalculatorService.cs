using AopWebApi.Services.Calculator.Model;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AopWebApi.Services
{
    public class CalculatorService : ICalculatorService
    {
        private readonly IQuotientCalculator quotientImplementation;

        private IValidationService _validationService { get; set; }

        public CalculatorService(IValidationService validationService, IQuotientCalculator quotientImplementation)
        {
            _validationService = validationService;
            this.quotientImplementation = quotientImplementation;
        }

        public Task<DivisionResponse> Quotient(int dividend, int divisor)
        {
            _validationService.IsValidDivision(dividend, divisor);
            //if (!operandIsValid )
            //    return Task.FromResult(DivisionResponse.NotANumber());

            //if (divisor == 999)
            //    return Task.FromException<DivisionResponse>(new InvalidOperationException("Async exception"));
            //
            //if (divisor == 998)
            //{
            //    var source = new CancellationTokenSource();
            //    source.Cancel();
            //    return Task.FromCanceled<DivisionResponse>(source.Token);
            //}
            //
            //if (divisor == 997)
            //    throw new InvalidOperationException("Sync exception");

            return quotientImplementation.Divide(dividend, divisor);
            //return Task.FromResult(new DivisionResponse(dividend / divisor));
        }
    }
}
