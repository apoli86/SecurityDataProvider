using System;

namespace PortfolioSecurity.Batch.Services
{
    public class NavDateCalculator : INavDateCalculator
    {
        public DateTime CalculateNavDate(DateTime date)
        {
            DateTime navDate = date.AddDays(-1);

            if (navDate.DayOfWeek == DayOfWeek.Sunday)
            {
                navDate = navDate.AddDays(-2);
            }

            return navDate;
        }
    }
}
