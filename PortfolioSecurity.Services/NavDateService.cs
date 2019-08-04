using NHibernate;
using NLog;
using PortfolioSecurity.Batch.Services;
using PortfolioSecurity.DAL.Repositories;
using PortfolioSecurity.Entities;
using PortfolioSecurity.Entities.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PortfolioSecurity.Services
{
    public class NavDateService : INavDateService
    {
        private readonly INavDateCalculator navDateCalculator;
        private readonly INavDateRepository navDateRepository;
        private readonly ILogger logger;

        public NavDateService(INavDateCalculator navDateCalculator, INavDateRepository navDateRepository, ILogger logger)
        {
            this.navDateCalculator = navDateCalculator;
            this.navDateRepository = navDateRepository;
            this.logger = logger;
        }

        public NavDate GetCurrentNavDate(ISession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            DateTime navDate = navDateCalculator.CalculateNavDate(DateTime.Today);

            var currentNavDate = navDateRepository.GetNavDate(session, navDate);

            return currentNavDate;
        }

        public NavDate CreateNavDateIfNotExists(ISession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            var currentNavDate = GetCurrentNavDate(session);

            if (currentNavDate == null)
            {
                DateTime navDate = navDateCalculator.CalculateNavDate(DateTime.Today);

                currentNavDate = new NavDate() { Date = navDate, RefreshSecurityStaticDataStatus = RefreshSecurityStaticDataStatus.ToDo };
                navDateRepository.SaveOrUpdate(session, currentNavDate);

                logger.Log(LogLevel.Info, $"NavDate {navDate} has been created");
            }

            return currentNavDate;
        }

        public void FixStatusOfPreviousNavDate(ISession session, NavDate currentNavDate)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (currentNavDate == null)
            {
                throw new ArgumentNullException(nameof(currentNavDate));
            }

            IList<NavDate> navDateInErrorList = navDateRepository.GetNavDateByRefreshSecurityStaticDataStatus(session, RefreshSecurityStaticDataStatus.Error);
            IList<NavDate> navDateInProgressList = navDateRepository.GetNavDateByRefreshSecurityStaticDataStatus(session, RefreshSecurityStaticDataStatus.InProgress);
            IList<NavDate> navDateToDoList = navDateRepository.GetNavDateByRefreshSecurityStaticDataStatus(session, RefreshSecurityStaticDataStatus.ToDo);

            IList<NavDate> pendingNavDateList = navDateInErrorList.Concat(navDateInProgressList).Concat(navDateToDoList).ToList();

            logger.Log(LogLevel.Info, $"Pending NavDate retrieved: {pendingNavDateList.Count}");

            foreach (var pendingNavDate in pendingNavDateList)
            {
                if (pendingNavDate.NavDateId == currentNavDate.NavDateId)
                {
                    continue;
                }

                logger.Log(LogLevel.Info, $"NavDate [{pendingNavDate.NavDateId}, {pendingNavDate.Date}]: RefreshSecurityStaticDataStatus {pendingNavDate.RefreshSecurityStaticDataStatus} will be set to Expired");

                pendingNavDate.RefreshSecurityStaticDataStatus = RefreshSecurityStaticDataStatus.Expired;
                navDateRepository.SaveOrUpdate(session, pendingNavDate);

                logger.Log(LogLevel.Info, $"NavDate [{pendingNavDate.NavDateId}, {pendingNavDate.Date}]: RefreshSecurityStaticDataStatus {pendingNavDate.RefreshSecurityStaticDataStatus} has been set to Expired");
            }
        }

        public void UpdateNavDate(ISession session, NavDate navDate)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (navDate == null)
            {
                throw new ArgumentNullException(nameof(navDate));
            }

            navDateRepository.SaveOrUpdate(session, navDate);
        }

        public void UpdateNavDate(IStatelessSession session, NavDate navDate)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (navDate == null)
            {
                throw new ArgumentNullException(nameof(navDate));
            }

            navDateRepository.Update(session, navDate);
        }
    }
}
