namespace CreditCardApplications
{
    public class FraudLookup
    {
        public virtual bool IsFraudRisk(CreditCardApplication application)
        {
            return application.LastName == "Smith";
        }
    }
}