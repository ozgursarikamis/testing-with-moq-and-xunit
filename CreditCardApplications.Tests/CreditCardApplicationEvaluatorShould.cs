using Moq;
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
                new Mock<IFrequentlyFlyerNumberValidator>(MockBehavior.Strict);

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
    }
}
