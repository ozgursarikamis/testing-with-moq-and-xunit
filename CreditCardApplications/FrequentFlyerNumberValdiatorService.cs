using System;

namespace CreditCardApplications
{
    public interface IFrequentlyFlyerNumberValidator
    {
        bool isValid(string frequentFlyerNumber);
        void isValid(string frequentFlyerNumber, out bool isValid);
        string LicenseKey { get; }
    }

    public class FrequentFlyerNumberValdiatorService : IFrequentlyFlyerNumberValidator
    {
        public bool isValid(string frequentFlyerNumber)
        {
            throw new System.NotImplementedException();
        }

        public void isValid(string frequentFlyerNumber, out bool isValid)
        {
            throw new System.NotImplementedException();
        }

        public string LicenseKey
        {
            get
            {
                throw new NotImplementedException("for demo purposes");
            }
        }
    }
}