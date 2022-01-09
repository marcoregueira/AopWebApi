using System.Collections.Generic;
using Elastic.Apm.Api;
using Elastic.Apm;

namespace Test.Tools
{
    public static class ApmResult
    {
        // This class is not to be used outside the testing environment
        // It is assumed that only one test will run at the same time
        public static List<ITransaction> Transactions { get; set; } = new();
        public static List<ISpan> Spans { get; set; } = new();
        public static List<IError> Errors { get; set; } = new();

        static ApmResult()
        {
            Agent.AddFilter((transaction) =>
            {
                Transactions.Add(transaction); return transaction;
            });
            Agent.AddFilter((span) =>
            {
                Spans.Add(span); return span;
            });
            Agent.AddFilter((error) =>
            {
                Errors.Add(error); return error;
            });
        }

        public static void Clear()
        {
            Transactions.Clear();
            Spans.Clear();
            Errors.Clear();
        }
    }
}