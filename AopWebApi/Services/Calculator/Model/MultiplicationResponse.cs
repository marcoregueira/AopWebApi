namespace AopWebApi.Services.Calculator.Model
{
    public class MultiplicationResponse
    {
        public int Product { get; set; }

        public MultiplicationResponse() { }
        public MultiplicationResponse(int product)
        {
            Product = product;
        }
    }
}
