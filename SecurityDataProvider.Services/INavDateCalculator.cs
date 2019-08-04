using System;

namespace SecurityDataProvider.Batch.Services
{
    public interface INavDateCalculator
    {
        DateTime CalculateNavDate(DateTime date);
    }
}