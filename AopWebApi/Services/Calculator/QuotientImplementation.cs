using AopWebApi.Services.Calculator.Model;

using System.Threading.Tasks;

namespace AopWebApi.Services
{
    public class QuotientImplementation : IQuotientCalculator
    {
        public Task<DivisionResponse> Divide(int dividend, int divisor)
        {
            return Task.FromResult(new DivisionResponse(dividend / divisor));
        }
    }
}