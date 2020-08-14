using System;

namespace CreditCardApplications
{
    public interface IFrequentlyFlyerNumberValidator
    {
        bool isValid(string frequentFlyerNumber);
        void isValid(string frequentFlyerNumber, out bool isValid);
        // string LicenseKey { get; }
        IServiceInformation ServiceInformation { get; }
    }

    public interface ILicenseData
    {
        string LicenseKey { get; }
    }

    public interface IServiceInformation
    {
        ILicenseData License { get; }
    }

    public class FrequentFlyerNumberValdiatorService : IFrequentlyFlyerNumberValidator
    {
        public bool isValid(string frequentFlyerNumber)
        {
            throw new NotImplementedException();
        }

        public void isValid(string frequentFlyerNumber, out bool isValid)
        {
            throw new NotImplementedException();
        }

        //public string LicenseKey
        //{
        //    get
        //    {
        //        throw new NotImplementedException("for demo purposes");
        //    }
        //}

        public IServiceInformation ServiceInformation => throw new NotImplementedException();
    }
}