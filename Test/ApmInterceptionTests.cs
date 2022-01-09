using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using AopWebApi;
using AopWebApi.Services;

using Elastic.Apm.Api;

using FluentAssertions;

using Test.TestTools;
using Test.Tools;

using WideWorldImporters.API.IntegrationTests;

using Xunit;

namespace Test
{
    public class ApmInterceptionTests : IClassFixture<WebApiServerFixture<Startup>>
    {
        private const string CALCULATOR_URL = "/Calculator/division";

        public HttpClient ApiClient;

        public ApmInterceptionTests(WebApiServerFixture<Startup> fixture)
        {
            ApiClient = fixture.Client;
            System.Threading.Thread.Sleep(8000); //apm boot time
        }


        [Fact]
        public async Task Interceptor_Captures_Sync_Success()
        {
            ApmResult.Clear();
            DependencyReplacement.ResetInstances();

            // Act
            var response = await ApiClient.PostAsync(CALCULATOR_URL, ContentHelper.GetStringContent(new { dividend = 1, divisor = 100 }));
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            System.Threading.Thread.Sleep(2000); // wait for APM's flush interval 

            // Assert
            responseBody.Should().Be("{\"quotient\":0}");
            ApmResult.Transactions.Should().HaveCount(1);
            ApmResult.Transactions.First().Name.Should().Be("POST Calculator/Division");
            ApmResult.Spans.Should().HaveCount(2);
            ApmResult.Spans[0].Name.Should().Be("ValidationService.IsValidDivision");
            ApmResult.Spans[0].Outcome.Should().Be(Outcome.Success);

            ApmResult.Spans[1].Name.Should().Be("CalculatorService.Quotient");
            ApmResult.Spans[1].Outcome.Should().Be(Outcome.Success);
            ApmResult.Errors.Should().HaveCount(0);
        }

        [Fact]
        public async Task Interceptor_Captures_Async_Success()
        {
            ApmResult.Clear();
            DependencyReplacement.SetResolver<IQuotientCalculator>(() => new QuotientImplementationFake(ResultTypeEnum.SyncSuccess));

            // Act
            var response = await ApiClient.PostAsync(CALCULATOR_URL, ContentHelper.GetStringContent(new { dividend = 1, divisor = 100 }));
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            System.Threading.Thread.Sleep(2000);

            // Assert
            responseBody.Should().Be("{\"quotient\":0}");
            ApmResult.Transactions.Should().HaveCount(1);
            ApmResult.Transactions.First().Name.Should().Be("POST Calculator/Division");
            ApmResult.Spans.Should().HaveCount(2);
            ApmResult.Spans[0].Name.Should().Be("ValidationService.IsValidDivision");
            ApmResult.Spans[0].Outcome.Should().Be(Outcome.Success);

            ApmResult.Spans[1].Name.Should().Be("CalculatorService.Quotient");
            ApmResult.Spans[1].Outcome.Should().Be(Outcome.Success);
            ApmResult.Errors.Should().HaveCount(0);
        }
        [Fact]
        public async Task Interceptor_Captures_Async_Exception()
        {
            ApmResult.Clear();
            DependencyReplacement.SetResolver<IQuotientCalculator>(() => new QuotientImplementationFake(ResultTypeEnum.AsyncException));

            // Act
            var response = await ApiClient.PostAsync(CALCULATOR_URL, ContentHelper.GetStringContent(new { dividend = 1, divisor = 999 }));
            response.StatusCode.Should().Be(500);
            var responseBody = await response.Content.ReadAsStringAsync();
            System.Threading.Thread.Sleep(12000);

            // Assert
            responseBody.Should().Contain("System.InvalidOperationException: Fake Async exception");
            ApmResult.Transactions.Should().HaveCount(1);
            ApmResult.Transactions.First().Name.Should().Be("POST Calculator/Division");
            ApmResult.Spans.Should().HaveCount(2);

            ApmResult.Spans[1].Name.Should().Be("CalculatorService.Quotient");
            ApmResult.Spans[1].Outcome.Should().Be(Outcome.Failure);
            ApmResult.Spans[1].TryGetLabel<string>("AsyncTask", out var label).Should().BeTrue();
            label.Should().Be("Faulted");

            ApmResult.Errors.Should().HaveCount(1);
            ApmResult.Errors.First().Exception.Type.Should().Be("System.InvalidOperationException");
        }

        [Fact]
        public async Task Interceptor_Captures_Async_Cancellation()
        {
            ApmResult.Clear();
            DependencyReplacement.SetResolver<IQuotientCalculator>(() => new QuotientImplementationFake(ResultTypeEnum.TaskCancelled));

            // Act
            var response = await ApiClient.PostAsync(CALCULATOR_URL, ContentHelper.GetStringContent(new { dividend = 1, divisor = 998 }));
            response.StatusCode.Should().Be(500);
            var responseBody = await response.Content.ReadAsStringAsync();
            System.Threading.Thread.Sleep(12000);

            // Assert
            responseBody.Should().Contain("System.Threading.Tasks.TaskCanceledException: A task was canceled.");
            ApmResult.Transactions.Should().HaveCount(1);
            ApmResult.Transactions.First().Name.Should().Be("POST Calculator/Division");
            ApmResult.Spans.Should().HaveCount(2);

            ApmResult.Spans[1].Name.Should().Be("CalculatorService.Quotient");
            ApmResult.Spans[1].Outcome.Should().Be(Outcome.Unknown);
            ApmResult.Spans[1].TryGetLabel<string>("AsyncTask", out var label).Should().BeTrue();
            label.Should().Be("Cancelled");

            ApmResult.Errors.Should().HaveCount(1);
            ApmResult.Errors.First().Exception.Type.Should().Be("System.Threading.Tasks.TaskCanceledException");
        }


        [Fact]
        public async Task Interceptor_Captures_Sync_Exception()
        {
            ApmResult.Clear();
            DependencyReplacement.SetResolver<IQuotientCalculator>(() => new QuotientImplementationFake(ResultTypeEnum.SyncException));

            // Act
            var response = await ApiClient.PostAsync(CALCULATOR_URL, ContentHelper.GetStringContent(new { dividend = 1, divisor = 9 }));
            response.StatusCode.Should().Be(500);
            var responseBody = await response.Content.ReadAsStringAsync();
            System.Threading.Thread.Sleep(2000);

            // Assert
            responseBody.Should().Contain("System.InvalidOperationException: Fake Sync exception");

            ApmResult.Transactions.Should().HaveCount(1);
            ApmResult.Transactions.First().Name.Should().Be("POST Calculator/Division");

            ApmResult.Spans.Should().HaveCount(2);

            ApmResult.Spans[1].Name.Should().Be("CalculatorService.Quotient");
            ApmResult.Spans[1].Outcome.Should().Be(Outcome.Failure);

            ApmResult.Errors.Should().HaveCount(1);
            ApmResult.Errors.First().Exception.Type.Should().Be("System.InvalidOperationException");
        }
    }
}
