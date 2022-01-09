using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Castle.DynamicProxy;

using Elastic.Apm;
using Elastic.Apm.Api;

namespace AopWebApi.Interception
{
    //[DebuggerStepThrough]
    public class ApmInterceptor : IInterceptor
    {
        public ApmInterceptor()
        {
        }

        public void Intercept(IInvocation invocation)
        {
            //for async see https://github.com/castleproject/Core/issues/107
            // https://stackoverflow.com/a/39784559/2982757

            var className = invocation.InvocationTarget.GetType().Name;
            var method = invocation.Method.Name;

            // in ASPNET Core exceptions are captured by the middleware layer of the APM agent and assigned to the transaction
            // we must choose between
            // - recording exceptions when they appear and being able to capture silenced exceptions but to get duplicates in the errors collection
            // - or record only the exceptions that made the global transaction fail
            var notInTransaction = Agent.Tracer.CurrentTransaction == null;

            var span =
                Agent.Tracer.CurrentSpan?.StartSpan($"{className}.{method}", "call") ??
                Agent.Tracer.CurrentTransaction?.StartSpan($"{className}.{method}", "call") ??
                Agent.Tracer.StartTransaction($"{className}.{method}", "scope") as IExecutionSegment;

            try
            {
                invocation.Proceed();
            }
            catch (Exception ex)
            {
                // SYNC EXCEPTION
                span.Outcome = Outcome.Failure;

                if (notInTransaction)
                    span.CaptureException(ex);

                span.End();
                throw;
            }

            var returnType = invocation.Method.ReturnType;
            bool isAsync = invocation.ReturnValue != null && typeof(Task).IsAssignableFrom(invocation.Method.ReturnType);

            if (isAsync)
            {
                var task = (Task)invocation.ReturnValue;
                task.ContinueWith(t =>
                {
                    if (t.IsCanceled)
                    {
                        //ASYNC Cancelled
                        span.Outcome = Outcome.Unknown;
                        span.SetLabel("AsyncTask", "Cancelled");
                    }
                    else if (t.IsFaulted)
                    {
                        //ASYNC Exception
                        span.Outcome = Outcome.Failure;
                        span.SetLabel("AsyncTask", "Faulted");

                        if (notInTransaction)
                            span.CaptureException(t.Exception);
                    }
                    else
                    {
                        //ASYNC Success
                        span.Outcome = Outcome.Success;
                    }

                    span.End();
                }, TaskContinuationOptions.ExecuteSynchronously);
            }
            else
            {
                //SYNC Success
                span.Outcome = Outcome.Success;
                span.End();
            }
        }
    }
}
