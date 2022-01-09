using AopWebApi.Services.Calculator.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AopWebApi.Services
{
    public class QuotientImplementationFake : IQuotientCalculator
    {
        private Lazy<List<IQuotientCalculator>> _quotientImplementations;

        public ResultTypeEnum FakeResult { get; set; }

        public QuotientImplementationFake(ResultTypeEnum fakeResult)
        {
            FakeResult = fakeResult;
        }

        public QuotientImplementationFake(/*Lazy<List<IQuotientImplementation>> quotientImplementations*/)
        {
            //_quotientImplementations = quotientImplementations;
        }

        public Task<DivisionResponse> Divide(int dividend, int divisor)
        {
            // Here we emulate the conditions to trigger different paths in the ApmInterceptor.Intercept method.

            switch (FakeResult)
            {
                case ResultTypeEnum.SyncSuccess:
                    return Task.FromResult(new DivisionResponse(0));

                case ResultTypeEnum.AsyncSuccess:
                    Task.Delay(1000).Wait();
                    return Task.FromResult(new DivisionResponse(0));

                case ResultTypeEnum.AsyncException:
                    Task.Delay(1000).Wait();
                    return Task.FromException<DivisionResponse>(new InvalidOperationException("Fake Async exception"));

                case ResultTypeEnum.SyncException:
                    throw new InvalidOperationException("Fake Sync exception");

                case ResultTypeEnum.TaskCancelled:
                    return Task.FromCanceled<DivisionResponse>(new CancellationTokenSource(0).Token);

                default:
                    throw new NotImplementedException("Not implemented test case");
            };
        }
    }
}
