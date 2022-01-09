using AopWebApi.Services.Calculator.Model;

using System.Threading;
using System;
using System.Threading.Tasks;

namespace AopWebApi.Services
{
    public interface IQuotientCalculator
    {
        Task<DivisionResponse> Divide(int dividend, int divisor);
    }
}