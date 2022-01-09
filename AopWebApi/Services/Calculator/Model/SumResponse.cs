namespace AopWebApi.Services.Calculator.Model
{
    public class SumResponse
    {
        public int Sum { get; set; }

        public SumResponse() { }
        public SumResponse(int sum)
        {
            Sum = sum;
        }
    }
}
