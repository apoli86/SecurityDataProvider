using System;

namespace PortfolioSecurity.Batch.Services
{
    public class NavDateCalculator : INavDateCalculator
    {
        public DateTime CalculateNavDate(DateTime date)
        {
            DateTime navDate = DateTime.Today;

            if (navDate.DayOfWeek == DayOfWeek.Sunday)
            {
                navDate = navDate.AddDays(-2);
            }
            else if (navDate.DayOfWeek == DayOfWeek.Monday)
            {
                navDate = navDate.AddDays(-3);
            }
            else
            {
                navDate = navDate.AddDays(-1);
            }

            return navDate;
        }
    }
}
