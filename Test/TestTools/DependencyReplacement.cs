using System;
using Autofac.Core.Resolving.Pipeline;
using System.Collections.Generic;
using Autofac.Core;

namespace Test.TestTools
{
    /// <summary>
    /// This middleware allows registering a resolving lambda for each type.
    /// When a instance of that type is requested, if there is a matching lambda it will called to get an the new instance
    /// This instance could be a substitute in order to improve tests
    /// </summary>
    public class DependencyReplacement : IResolveMiddleware
    {
        private static Dictionary<Type, Func<object>> types = new();
        public PipelinePhase Phase => PipelinePhase.Activation;

        public static void SetResolver<T>(Func<object> instanceFactory) => types[typeof(T)] = instanceFactory;

        public static void ResetInstances() => types.Clear();

        public static Func<object> GetInstanceFactory(Type type)
        {
            types.TryGetValue(type, out var factory);
            return factory;
        }

        public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
        {
            var serviceRequest = context.Service as TypedService;
            var factory = GetInstanceFactory(serviceRequest?.ServiceType);
            if (factory == null)
            {
                next.Invoke(context);
                return;
            }

            context.Instance = factory.Invoke();
        }
    }
}