using System;
using EmiTest.Domain.Entities;
using EmiTest.Domain.Enums;

namespace EmiTest.Application.Strategies
{
    public interface IBonusStrategyFactory
    {
        IBonusStrategy GetStrategy(PositionType positionType);
    }

    public class BonusStrategyFactory : IBonusStrategyFactory
    {
        public IBonusStrategy GetStrategy(PositionType positionType)
        {
            return positionType switch
            {
                PositionType.Regular => new RegularEmployeeBonusStrategy(),
                PositionType.Manager => new ManagerBonusStrategy(),
                PositionType.SeniorManager => new ManagerBonusStrategy(), 
                PositionType.Director => new ManagerBonusStrategy(),      
                _ => throw new ArgumentException("Posición no válida para el cálculo de bono.")
            };
        }
    }
}