namespace AopWebApi.Services.Calculator.Model
{
    public class DivisionResponse
    {
        public int? Quotient { get; set; }

        public DivisionResponse() { }
        public DivisionResponse(int quotient)
        {
            Quotient = quotient;
        }

        public static DivisionResponse NotANumber()
        {
            return new DivisionResponse() { Quotient = null };
        }
    }
}
