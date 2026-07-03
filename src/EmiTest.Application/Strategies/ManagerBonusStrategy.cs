using EmiTest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmiTest.Application.Strategies
{
    public class ManagerBonusStrategy : IBonusStrategy
    {
        public decimal Calculate(decimal salary) => salary * 0.20m; // 20% 
    }
}