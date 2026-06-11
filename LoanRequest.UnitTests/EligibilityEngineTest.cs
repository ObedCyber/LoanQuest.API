using FluentAssertions;
using LoanRequestDomain.Entities;
using LoanRequestInfrastructure.Services.Loans;

namespace LoanRequest.UnitTests
{
    public class EligibilityEngineTest
    {
        [Fact]
        public void RunCalculation_ShouldReject_WhenDisposableIncomeIsZeroOrNegative()
        {
            // Arrange
            decimal netMonthlyIncome = 200000m;
            decimal grossSalary = 250000m;
            decimal monthlyObligations = 220000m;
            decimal requestedAmount = 500000m;
            int requestedTenor = 12;
            DateTime dateOfBirth = DateTime.Today.AddYears(-30);

            var dummyProduct = new LoanProducts
            {
                MinAmount = 50000m,
                MaxAmount = 2000000m,
                InterestRatePercent = 15m,
                MaxDSRPercent = 40m
            };
            // Act
            var result = EligibilityEngine.RunCalculation(
                netMonthlyIncome, grossSalary, monthlyObligations,
                requestedAmount, requestedTenor, dateOfBirth, dummyProduct);

            // Assert
            Assert.False(result.IsEligible);
            result.RejectionReasons.Should().ContainSingle(r => r.Code == "INSUFFICIENT_DISPOSABLE_INCOME");
        }

        [Fact]
        public void RunCalculation_ShouldReject_WhenRequestedTenorIsAboveMonthsToRetirement()
        {
            // Arrange
            decimal netMonthlyIncome = 200000m;
            decimal grossSalary = 250000m;
            decimal monthlyObligations = 50000m;
            decimal requestedAmount = 500000m;
            int requestedTenor = 48;
            DateTime dateOfBirth = DateTime.Today.AddYears(-58);
            var dummyProduct = new LoanProducts
            {
                MinAmount = 50000m,
                MaxAmount = 2000000m,
                InterestRatePercent = 15m,
                MaxDSRPercent = 40m
            };
            // Act
            var result = EligibilityEngine.RunCalculation(
                netMonthlyIncome, grossSalary, monthlyObligations,
                requestedAmount, requestedTenor, dateOfBirth, dummyProduct);
            // Assert
            Assert.False(result.IsEligible);
            result.RejectionReasons.Should().ContainSingle(r => r.Code == "TENOR_EXCEEDS_RETIREMENT_AGE");
        }

        [Fact]
        public void RunCalculation_ShouldCapMaxEligibleAmount_WhenLtiMultiplierIsApplied()
        {
            // Arrange
            decimal netMonthlyIncome = 500000m;
            decimal grossSalary = 100000m;
            decimal monthlyObligations = 0m;
            decimal requestedAmount = 1000000m;
            int requestedTenor = 24;
            DateTime dateOfBirth = DateTime.Today.AddYears(-30);

            var product = new LoanProducts
            {
                MinAmount = 20000m,
                MaxAmount = 5000000m,
                InterestRatePercent = 10m,
                MaxDSRPercent = 50m,
                MaxLTIMultiplier = 0.5m
            };

            // Act
            var result = EligibilityEngine.RunCalculation(
                netMonthlyIncome, grossSalary, monthlyObligations,
                requestedAmount, requestedTenor, dateOfBirth, product);

            // Assert          
            result.MaxEligibleAmount.Should().BeLessThanOrEqualTo(600000m);
            result.IsEligible.Should().BeFalse(); // Rejects because requested (1M) > max allowed (600k)
            result.RejectionReasons.Should().Contain(r => r.Code == "REQUESTED_AMOUNT_EXCEEDS_ELIGIBLE");
        }

        [Fact]
        public void RunCalculation_ShouldReject_WhenCalculatedMaxAmountIsBelowProductMinimum()
        {
            // Arrange
            decimal netMonthlyIncome = 60000m;
            decimal monthlyObligations = 50000m;
            decimal grossSalary = 70000m;
            decimal requestedAmount = 150000m;
            int requestedTenor = 3;
            DateTime dateOfBirth = DateTime.Today.AddYears(-25);

            var product = new LoanProducts
            {
                MinAmount = 100000m,
                MaxAmount = 500000m,
                InterestRatePercent = 12m,
                MaxDSRPercent = 30m
            };

            // Act
            var result = EligibilityEngine.RunCalculation(
                netMonthlyIncome, grossSalary, monthlyObligations,
                requestedAmount, requestedTenor, dateOfBirth, product);

            // Assert
            result.IsEligible.Should().BeFalse();
            result.MaxEligibleAmount.Should().Be(0);
            result.RecommendedAmount.Should().Be(0);
            result.MinEligibleAmount.Should().Be(100000m);
            result.RejectionReasons.Should().ContainSingle(r => r.Code == "AMOUNT_BELOW_PRODUCT_MINIMUM");
        }

        [Fact]
        public void RunCalculation_ShouldBeIneligibleButProvideRecommendations_WhenRequestedAmountExceedsCapacity()
        {
            // Arrange
            decimal netMonthlyIncome = 300000m;
            decimal grossSalary = 350000m;
            decimal monthlyObligations = 50000m;
            decimal requestedAmount = 4000000m;
            int requestedTenor = 12;
            DateTime dateOfBirth = DateTime.Today.AddYears(-35);

            var product = new LoanProducts
            {
                MinAmount = 100000m,
                MaxAmount = 5000000m,
                InterestRatePercent = 15m,
                MaxDSRPercent = 40m
            };

            // Act
            var result = EligibilityEngine.RunCalculation(
                netMonthlyIncome, grossSalary, monthlyObligations,
                requestedAmount, requestedTenor, dateOfBirth, product);

            // Assert
            result.IsEligible.Should().BeFalse();
            result.RejectionReasons.Should().ContainSingle(r => r.Code == "REQUESTED_AMOUNT_EXCEEDS_ELIGIBLE");
            result.MaxEligibleAmount.Should().BeLessThan(requestedAmount);
            result.RecommendedAmount.Should().Be(result.MaxEligibleAmount);
        }

        [Fact]
        public void RunCalculation_ShouldReturnSuccessfulEligibility_WhenApplicantMeetsAllCriteria()
        {
            // Arrange
            decimal netMonthlyIncome = 500000m;
            decimal grossSalary = 600000m;
            decimal monthlyObligations = 50000m;
            decimal requestedAmount = 500000m;
            int requestedTenor = 12;
            DateTime dateOfBirth = DateTime.Today.AddYears(-28);

            var product = new LoanProducts
            {
                MinAmount = 50000m,
                MaxAmount = 2000000m,
                InterestRatePercent = 12m,
                MaxDSRPercent = 45m
            };

            // Act
            var result = EligibilityEngine.RunCalculation(
                netMonthlyIncome, grossSalary, monthlyObligations,
                requestedAmount, requestedTenor, dateOfBirth, product);

            // Assert
            result.IsEligible.Should().BeTrue();
            result.RejectionReasons.Should().BeEmpty();
            result.RecommendedAmount.Should().Be(requestedAmount);
            result.MaxEligibleAmount.Should().BeGreaterThanOrEqualTo(requestedAmount);
            result.RiskRating.Should().NotBeNullOrEmpty();
            result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }

        [Theory]
        [InlineData(500000, 20000, "Low")]    // DSR = 4%   (Well below 40% of the 50% limit, which is 20%)
        [InlineData(500000, 120000, "Medium")] // DSR = 24%  (Between 20% and 40% DSR)
        [InlineData(500000, 225000, "High")]   // DSR = 45%  (Between 40% and 50% DSR, near the limit)
        public void RunCalculation_ShouldReturnCorrectRiskRating_WhenApplicantIsEligible(
            decimal netMonthlyIncome,
            decimal monthlyObligations,
            string expectedRiskRating)
        {
            // Arrange
            decimal grossSalary = netMonthlyIncome * 1.2m;
            decimal requestedAmount = 100000m;
            int requestedTenor = 12;
            DateTime dateOfBirth = DateTime.Today.AddYears(-30);

            var product = new LoanProducts
            {
                MinAmount = 20000m,
                MaxAmount = 2000000m,
                InterestRatePercent = 10m,
                MaxDSRPercent = 50m,
                MaxLTIMultiplier = 4m
            };

            // Act
            var result = EligibilityEngine.RunCalculation(
                netMonthlyIncome,
                grossSalary,
                monthlyObligations,
                requestedAmount,
                requestedTenor,
                dateOfBirth,
                product);

            // Assert
            result.IsEligible.Should().BeTrue();
            result.RejectionReasons.Should().BeEmpty();
            result.RiskRating.Should().Be(expectedRiskRating);
        }
    }
}
