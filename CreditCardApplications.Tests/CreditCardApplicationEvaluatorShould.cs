using System;
using System.Collections.Generic;
using Moq;
using Moq.Protected;
using Xunit;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationEvaluatorShould
    {
        private Mock<IFrequentlyFlyerNumberValidator> validator;
        private CreditCardApplicationEvaluator sut;

        public CreditCardApplicationEvaluatorShould()
        {
            validator = new Mock<IFrequentlyFlyerNumberValidator>();
            validator.SetupAllProperties();
            validator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            validator.Setup(x => x.isValid(It.IsAny<string>())).Returns(true);

            sut = new CreditCardApplicationEvaluator(validator.Object);
        }

        [Fact]
        public void AcceptHighIncomeApplications()
        {
            var application = new CreditCardApplication{ GrossAnnualIncome = 100_000 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }

        [Fact]
        public void ReferYoungApplications()
        {
            validator.DefaultValue = DefaultValue.Mock;

            var application = new CreditCardApplication { Age = 19 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplications()
        {
            validator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");

            // validator.Setup(x => x.isValid("x")).Returns(true);
            //validator.Setup(x => x.isValid(It.IsAny<string>())).Returns(true);
            //validator
            //    .Setup(x => x.isValid(It.Is<string>(number => number.StartsWith("y"))))
            //    .Returns(true);
            //validator
            //    .Setup(x => x.isValid(It.IsInRange("a", "z", Range.Inclusive)))
            //    .Returns(true);
            //validator
            //    .Setup(x => x.isValid(It.IsIn("z", "y", "x")))
            //    .Returns(true);

            validator
                .Setup(x => x.isValid(It.IsRegex("[a-z]")))
                .Returns(true);

            var application = new CreditCardApplication
            {
                GrossAnnualIncome = 19_999,
                Age = 42,
                FrequentFlyerNumber = "2"
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        public void ReferInvalidFrequentFlyerApplications()
        {
            validator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");

            // MockBehavior.Strict obligates to setup a method:

            validator.Setup(x => x.isValid(It.IsAny<string>()))
                .Returns(false);

            var sut = new CreditCardApplicationEvaluator(validator.Object);
            var application = new CreditCardApplication();

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplicationsOutDemo()
        {
            bool isValid = true;
            validator.Setup(x => x.isValid(It.IsAny<string>(), out isValid));

            var sut = new CreditCardApplicationEvaluator(validator.Object);
            var application = new CreditCardApplication
            {
                GrossAnnualIncome = 19_999,
                Age = 42
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        public void ReferenceLicenseKeyExpired()
        {
            //var mockLicenseData = new Mock<ILicenseData>();
            //mockLicenseData.Setup(x => x.LicenseKey).Returns("EXPIRED");

            //var mockServiceInfo = new Mock<IServiceInformation>();
            //mockServiceInfo.Setup(x => x.License).Returns(mockLicenseData.Object);

            validator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns(GetLicenseKeyExpiryString);
            validator.Setup(x => x.isValid(It.IsAny<string>()))
                .Returns(true);

            var application = new CreditCardApplication{ Age = 42 };

            CreditCardApplicationDecision desicion = sut.Evaluate(application);
            Assert.Equal(
                CreditCardApplicationDecision.ReferredToHuman, desicion
                );
        }

        private string GetLicenseKeyExpiryString()
        {
            return "EXPIRED";
        }

        [Fact]
        public void UseDetailedLookupForOlderApplications()
        {
            // SetUpProperty tells our mock object to start tracking that property.
            // validator.SetupProperty(x => x.ValidationMode);
            validator.SetupAllProperties();

            validator
                .Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");
            
            var application = new CreditCardApplication{ Age = 30 };
            sut.Evaluate(application);

            Assert.Equal(ValidationMode.Detailed, validator.Object.ValidationMode);
        }

        [Fact]
        public void ValidateFrequentFlyerNumberForLowIncomeApplications()
        {
            validator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");

            var application = new CreditCardApplication();
            
            sut.Evaluate(application);

             // is isValid method called?
             validator.Verify(x => x.isValid(null), 
                 "this is an error message when test fails");
        }

        [Fact]
        public void VerifyNotCalledMethod()
        {
            validator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");

            var application = new CreditCardApplication
            {
                GrossAnnualIncome = 99_000
            };
            sut.Evaluate(application);

            validator.Verify(x => x.isValid(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ValidateIfPropertyIsSet()
        {
            validator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");
            var application = new CreditCardApplication
            {
                GrossAnnualIncome = 99_000
            };
            sut.Evaluate(application);

            validator.VerifyGet(x => x.ServiceInformation.License.LicenseKey);
        }

        [Fact]
        public void ValidateIfPropertySetterIsSet()
        {
            validator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");

            var application = new CreditCardApplication
            {
                Age = 30
            };
            sut.Evaluate(application);

            // validator.VerifySet(x => x.ValidationMode = ValidationMode.Detailed);
            validator.VerifySet(x => x.ValidationMode = It.IsAny<ValidationMode>());

            validator.VerifyNoOtherCalls();
        }

        [Fact]
        public void ThrowingExceptions()
        {
            validator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");
            validator.Setup(x => x.isValid(It.IsAny<string>()))
                .Throws(
                    new Exception("Custom test message")); 
            var application = new CreditCardApplication {Age = 42};

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void TestingEvents()
        {
            validator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");
            validator.Setup(x => x.isValid(It.IsAny<string>()))
                .Returns(true)
                .Raises(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);

            var application = new CreditCardApplication
            {
                FrequentFlyerNumber = "x",
                Age = 25
            };

            sut.Evaluate(application);
            // validator.Raise(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);
            
            Assert.Equal(1, sut.ValidatorLookupCount);
        }

        [Fact]
        public void DifferentResultForSequentialCalls()
        {
            validator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");
            validator.SetupSequence(x => x.isValid(It.IsAny<string>()))
                .Returns(false)
                .Returns(true);

            var application = new CreditCardApplication
            {
                Age = 25
            };

            CreditCardApplicationDecision firstDecision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, firstDecision);

            CreditCardApplicationDecision secondDecision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, secondDecision);
        }

        [Fact]
        public void MultipleCallSequence()
        {
            validator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");
            
            var frequentFlyerNumbersPassed = new List<string>();
            validator.Setup(x => x.isValid(Capture.In(frequentFlyerNumbersPassed)));

           var application1 = new CreditCardApplication {
                Age = 25, FrequentFlyerNumber = "aa"
            };
            var application2 = new CreditCardApplication {
                Age = 25, FrequentFlyerNumber = "bb"
            };
            var application3 = new CreditCardApplication {
                Age = 25, FrequentFlyerNumber = "cc"
            };

            sut.Evaluate(application1);
            sut.Evaluate(application2);
            sut.Evaluate(application3);

            // Assert that isValid was called 3 times for aa, bb, cc values.
            Assert.Equal(new List<string> {"aa", "bb", "cc"}, frequentFlyerNumbersPassed);
        }

        [Fact]
        public void TestingConcreteClass()
        {
            var mockFraudLookup = new Mock<FraudLookup>();
            //mockFraudLookup.Setup(x => x.IsFraudRisk(It.IsAny<CreditCardApplication>()))
            //    .Returns(true);

            mockFraudLookup.Protected()
                .Setup<bool>("CheckApplication", ItExpr.IsAny<CreditCardApplication>())
                .Returns(true);

            var _sut = new CreditCardApplicationEvaluator(
                validator.Object, mockFraudLookup.Object);

            var application = new CreditCardApplication();

            CreditCardApplicationDecision decision = _sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHumanFraudRisk, decision);
        }

        [Fact]
        public void LinqToMocks()
        {
            // // Classic setup
            //var validator = new Mock<IFrequentlyFlyerNumberValidator>();
            //validator.Setup(x => x.ServiceInformation.License.LicenseKey)
            //    .Returns("OK");
            //validator.Setup(x => x.isValid(It.IsAny<string>()))
            //    .Returns(true);

            // LinqToMocks:
            IFrequentlyFlyerNumberValidator _validator =
                Mock.Of<IFrequentlyFlyerNumberValidator>(x =>
                    x.ServiceInformation.License.LicenseKey == "OK" &&
                    x.isValid(It.IsAny<string>()));

            sut = new CreditCardApplicationEvaluator(_validator);
            var application = new CreditCardApplication { Age = 25 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }
    }
}
