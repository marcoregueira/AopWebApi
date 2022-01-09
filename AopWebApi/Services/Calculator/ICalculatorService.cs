using AopWebApi.Services.Calculator.Model;
using System.Threading;
using System.Threading.Tasks;

namespace AopWebApi.Services
{
    public interface ICalculatorService
    {
        Task<DivisionResponse> Quotient(int number, int times);
    }
}