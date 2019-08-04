using System;

namespace PortfolioSecurity.Batch.Services
{
    public interface INavDateCalculator
    {
        DateTime CalculateNavDate(DateTime date);
    }
}