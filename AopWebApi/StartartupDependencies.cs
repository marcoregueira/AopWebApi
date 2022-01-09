using AopWebApi.Interception;
using AopWebApi.Services;

using Autofac;
using Autofac.Extras.DynamicProxy;

namespace AopWebApi
{
    public class StartartupDependencies : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(this.ThisAssembly)
                .PublicOnly()
                .Where(x => x.Name.EndsWith("Service"))
                .AsImplementedInterfaces()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(ApmInterceptor))
                .InstancePerLifetimeScope();

            builder.RegisterType<QuotientImplementation>().AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<ApmInterceptor>().AsSelf();
        }
    }
}
