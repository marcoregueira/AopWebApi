using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AopWebApi;
using Test.TestTools;

// ADAPTED FROM: https://www.codingame.com/playgrounds/35462/creating-web-api-in-asp-net-core-2-0/part-3---integration-tests
// Added: autofac middleware to be able of replacing instances from tests

namespace WideWorldImporters.API.IntegrationTests
{
    public class WebApiServerFixture<TStartup> : IDisposable where TStartup : class
    {
        public TestServer TestServer;

        public HttpClient Client { get; }
        public WebApiServerFixture() : this(Path.Combine("")) { }

        public void Dispose()
        {
            Client.Dispose();
            TestServer.Dispose();
        }

        public static string GetProjectPath(string projectRelativePath, Assembly startupAssembly)
        {
            var projectName = startupAssembly.GetName().Name;
            var applicationBasePath = AppContext.BaseDirectory;
            var directoryInfo = new DirectoryInfo(applicationBasePath);

            do
            {
                directoryInfo = directoryInfo.Parent;

                var projectDirectoryInfo = new DirectoryInfo(Path.Combine(directoryInfo.FullName, projectRelativePath));

                if (projectDirectoryInfo.Exists)
                    if (new FileInfo(Path.Combine(projectDirectoryInfo.FullName, projectName, $"{projectName}.csproj")).Exists)
                        return Path.Combine(projectDirectoryInfo.FullName, projectName);
            }
            while (directoryInfo.Parent != null);

            throw new Exception($"Project root could not be located using the application root {applicationBasePath}.");
        }

        protected virtual void InitializeServices(IServiceCollection services)
        {
            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;

            var manager = new ApplicationPartManager
            {
                ApplicationParts =
                {
                    new AssemblyPart(startupAssembly)
                },
                FeatureProviders =
                {
                    new ControllerFeatureProvider(),
                    new ViewComponentFeatureProvider()
                }
            };

            services.AddSingleton(manager);
        }

        protected WebApiServerFixture(string relativeTargetProjectParentDir)
        {
            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;
            var contentRoot = GetProjectPath(relativeTargetProjectParentDir, startupAssembly);

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(contentRoot)
                .AddJsonFile("appsettings.Test.json");

            var host = Host.CreateDefaultBuilder()
                .UseContentRoot(contentRoot)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    RegisterMiddleware(builder);
                    builder.RegisterModule(new StartartupDependencies());
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .UseStartup<TStartup>()
                        .ConfigureServices(InitializeServices)
                        .UseConfiguration(configurationBuilder.Build())
                        .UseEnvironment("Test");
                })
                .Build();

            host.StartAsync();
            TestServer = host.GetTestServer();

            Client = TestServer.CreateClient();
            Client.BaseAddress = new Uri("http://localhost:5001");
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private void RegisterMiddleware(ContainerBuilder builder)
        {
            builder.ComponentRegistryBuilder.Registered += (sender, args) =>
            {
                args.ComponentRegistration.PipelineBuilding += (sender2, pipeline) =>
                {
                    pipeline.Use(new DependencyReplacement());
                };
            };
        }
    }
}