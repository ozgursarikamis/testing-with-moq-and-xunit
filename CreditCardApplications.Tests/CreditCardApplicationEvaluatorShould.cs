using System;
using System.Collections.Generic;
using Moq;
using Moq.Protected;
using Xunit;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationEvaluatorShould
    {
        [Fact]
        public void AcceptHighIncomeApplications()
        {
            var mockValidator =
                new Mock<IFrequentlyFlyerNumberValidator>();

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication{ GrossAnnualIncome = 100_000 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }

        [Fact]
        public void ReferYoungApplications()
        {
            var mockValidator =
                new Mock<IFrequentlyFlyerNumberValidator>();

            mockValidator.DefaultValue = DefaultValue.Mock;

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication { Age = 19 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplications()
        {
            Mock<IFrequentlyFlyerNumberValidator> mockValidator =
                new Mock<IFrequentlyFlyerNumberValidator>();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");

            // mockValidator.Setup(x => x.isValid("x")).Returns(true);
            //mockValidator.Setup(x => x.isValid(It.IsAny<string>())).Returns(true);
            //mockValidator
            //    .Setup(x => x.isValid(It.Is<string>(number => number.StartsWith("y"))))
            //    .Returns(true);
            //mockValidator
            //    .Setup(x => x.isValid(It.IsInRange("a", "z", Range.Inclusive)))
            //    .Returns(true);
            //mockValidator
            //    .Setup(x => x.isValid(It.IsIn("z", "y", "x")))
            //    .Returns(true);

            mockValidator
                .Setup(x => x.isValid(It.IsRegex("[a-z]")))
                .Returns(true);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
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
            var mockValidator =
                new Mock<IFrequentlyFlyerNumberValidator>();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");

            // MockBehavior.Strict obligates to setup a method:

            mockValidator.Setup(x => x.isValid(It.IsAny<string>()))
                .Returns(false);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication();

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplicationsOutDemo()
        {
            var mockValidator =
                new Mock<IFrequentlyFlyerNumberValidator>();

            bool isValid = true;
            mockValidator.Setup(x => x.isValid(It.IsAny<string>(), out isValid));

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
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

            var mockValidator = new Mock<IFrequentlyFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns(GetLicenseKeyExpiryString);
            mockValidator.Setup(x => x.isValid(It.IsAny<string>()))
                .Returns(true);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

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
            var mockValidator = new Mock<IFrequentlyFlyerNumberValidator>();
            
            // SetUpProperty tells our mock object to start tracking that property.
            // mockValidator.SetupProperty(x => x.ValidationMode);
            mockValidator.SetupAllProperties();

            mockValidator
                .Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");
            
            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication{ Age = 30 };
            sut.Evaluate(application);

            Assert.Equal(ValidationMode.Detailed, mockValidator.Object.ValidationMode);
        }

        [Fact]
        public void ValidateFrequentFlyerNumberForLowIncomeApplications()
        {
            var mockValidator = new Mock<IFrequentlyFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication();
            
            sut.Evaluate(application);

             // is isValid method called?
             mockValidator.Verify(x => x.isValid(null), 
                 "this is an error message when test fails");
        }

        [Fact]
        public void VerifyNotCalledMethod()
        {
            var mockValidator = new Mock<IFrequentlyFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication
            {
                GrossAnnualIncome = 99_000
            };
            sut.Evaluate(application);

            mockValidator.Verify(x => x.isValid(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ValidateIfPropertyIsSet()
        {
            var mockValidator = new Mock<IFrequentlyFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication
            {
                GrossAnnualIncome = 99_000
            };
            sut.Evaluate(application);

            mockValidator.VerifyGet(x => x.ServiceInformation.License.LicenseKey);
        }

        [Fact]
        public void ValidateIfPropertySetterIsSet()
        {
            var mockValidator = new Mock<IFrequentlyFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication
            {
                Age = 30
            };
            sut.Evaluate(application);

            // mockValidator.VerifySet(x => x.ValidationMode = ValidationMode.Detailed);
            mockValidator.VerifySet(x => x.ValidationMode = It.IsAny<ValidationMode>());

            mockValidator.VerifyNoOtherCalls();
        }

        [Fact]
        public void ThrowingExceptions()
        {
            Mock<IFrequentlyFlyerNumberValidator> validator =
                new Mock<IFrequentlyFlyerNumberValidator>();

            validator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");
            validator.Setup(x => x.isValid(It.IsAny<string>()))
                .Throws(
                    new Exception("Custom test message"));

            var sut = new CreditCardApplicationEvaluator(validator.Object);
            var application = new CreditCardApplication {Age = 42};

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void TestingEvents()
        {
            var validator = new Mock<IFrequentlyFlyerNumberValidator>();
            validator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");
            validator.Setup(x => x.isValid(It.IsAny<string>()))
                .Returns(true)
                .Raises(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);

            var sut = new CreditCardApplicationEvaluator(validator.Object);
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
            var validator = new Mock<IFrequentlyFlyerNumberValidator>();
            validator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");
            validator.SetupSequence(x => x.isValid(It.IsAny<string>()))
                .Returns(false)
                .Returns(true);

            var sut = new CreditCardApplicationEvaluator(validator.Object);
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
            var validator = new Mock<IFrequentlyFlyerNumberValidator>();
            validator.Setup(x => x.ServiceInformation.License.LicenseKey)
                .Returns("OK");
            
            var frequentFlyerNumbersPassed = new List<string>();
            validator.Setup(x => x.isValid(Capture.In(frequentFlyerNumbersPassed)));

            var sut = new CreditCardApplicationEvaluator(validator.Object);
            
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
            var validator = new Mock<IFrequentlyFlyerNumberValidator>();
            var mockFraudLookup = new Mock<FraudLookup>();
            //mockFraudLookup.Setup(x => x.IsFraudRisk(It.IsAny<CreditCardApplication>()))
            //    .Returns(true);

            mockFraudLookup.Protected()
                .Setup<bool>("CheckApplication", ItExpr.IsAny<CreditCardApplication>())
                .Returns(true);

            var sut = new CreditCardApplicationEvaluator(
                validator.Object, mockFraudLookup.Object);

            var application = new CreditCardApplication();

            CreditCardApplicationDecision decision = sut.Evaluate(application);

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
            IFrequentlyFlyerNumberValidator validator =
                Mock.Of<IFrequentlyFlyerNumberValidator>(x =>
                    x.ServiceInformation.License.LicenseKey == "OK" &&
                    x.isValid(It.IsAny<string>()));

            var sut = new CreditCardApplicationEvaluator(validator);
            var application = new CreditCardApplication { Age = 25 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }
    }
}
