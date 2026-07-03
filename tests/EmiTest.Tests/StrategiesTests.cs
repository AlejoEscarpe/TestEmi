using System;
using Xunit;
using EmiTest.Application.Strategies;
using EmiTest.Domain.Enums;

namespace EmiTest.Tests
{
    public class StrategiesTests
    {
        [Fact]
        public void RegularEmployeeBonusStrategy_Returns_10Percent()
        {
            var strat = new RegularEmployeeBonusStrategy();
            decimal salary = 1000m;
            var bonus = strat.Calculate(salary);
            Assert.Equal(100m, bonus);
        }

        [Fact]
        public void ManagerBonusStrategy_Returns_20Percent()
        {
            var strat = new ManagerBonusStrategy();
            decimal salary = 2000m;
            var bonus = strat.Calculate(salary);
            Assert.Equal(400m, bonus);
        }

        [Fact]
        public void BonusStrategyFactory_Returns_Expected_Strategy_Types()
        {
            var factory = new BonusStrategyFactory();

            var s1 = factory.GetStrategy(PositionType.Regular);
            Assert.IsType<RegularEmployeeBonusStrategy>(s1);

            var s2 = factory.GetStrategy(PositionType.Manager);
            Assert.IsType<ManagerBonusStrategy>(s2);

            var s3 = factory.GetStrategy(PositionType.SeniorManager);
            Assert.IsType<ManagerBonusStrategy>(s3);

            var s4 = factory.GetStrategy(PositionType.Director);
            Assert.IsType<ManagerBonusStrategy>(s4);
        }
    }
}
