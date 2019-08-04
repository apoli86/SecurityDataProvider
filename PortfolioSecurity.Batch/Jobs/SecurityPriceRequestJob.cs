using NHibernate;
using NLog;
using PortfolioSecurity.Batch.Services;
using PortfolioSecurity.DAL.Repositories;
using PortfolioSecurity.Entities;
using PortfolioSecurity.Entities.Enum;
using Quartz;
using RabbitMQ.Client;
using SecurityDataProvider.Common.RabbitMq;
using SecurityDataProvider.Entities.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioSecurity.Batch.Jobs
{
    [DisallowConcurrentExecution]
    public class SecurityPriceRequestJob : IJob
    {
        private readonly ILogger logger;
        private readonly ISessionFactory sessionFactory;
        private readonly INavDateRepository navDateRepository;
        private readonly IPortfolioRepository portfolioRepository;
        private readonly IPortfolioNavDateRepository portfolioNavDateRepository;
        private readonly IPortfolioSecurityRepository portfolioSecurityRepository;
        private readonly IPortfolioNavDateSecurityPriceRepository portfolioNavDateSecurityPriceRepository;
        private readonly IConnectionFactory connectionFactory;
        private readonly INavDateCalculator navDateCalculator;

        public SecurityPriceRequestJob(ILogger logger, ISessionFactory sessionFactory, INavDateRepository navDateRepository, IPortfolioRepository portfolioRepository, IPortfolioNavDateRepository portfolioNavDateRepository, IPortfolioSecurityRepository portfolioSecurityRepository, IPortfolioNavDateSecurityPriceRepository portfolioNavDateSecurityPriceRepository, IConnectionFactory connectionFactory, INavDateCalculator navDateCalculator)
        {
            this.logger = logger;
            this.sessionFactory = sessionFactory;
            this.navDateRepository = navDateRepository;
            this.portfolioRepository = portfolioRepository;
            this.portfolioNavDateRepository = portfolioNavDateRepository;
            this.portfolioSecurityRepository = portfolioSecurityRepository;
            this.portfolioNavDateSecurityPriceRepository = portfolioNavDateSecurityPriceRepository;
            this.connectionFactory = connectionFactory;
            this.navDateCalculator = navDateCalculator;
        }

        public Task Execute(IJobExecutionContext context)
        {
            logger.Log(LogLevel.Info, "Begin");

            try
            {
                using (var session = sessionFactory.OpenSession())
                {
                    DateTime navDate = navDateCalculator.CalculateNavDate(DateTime.Today);
                    NavDate currentNavDate = navDateRepository.GetNavDate(session, navDate);

                    if (currentNavDate == null)
                    {
                        return Task.CompletedTask;
                    }

                    logger.Log(LogLevel.Info, "[CreatePortfolioNavDates]: Begin");
                    CreatePortfolioNavDates(session, currentNavDate);
                    logger.Log(LogLevel.Info, "[CreatePortfolioNavDates]: End");

                    logger.Log(LogLevel.Info, "[CreatePortfolioNavDateSecurityPrices]: Begin");
                    CreatePortfolioNavDateSecurityPrices(session, currentNavDate);
                    logger.Log(LogLevel.Info, "[CreatePortfolioNavDateSecurityPrices]: End");

                    logger.Log(LogLevel.Info, "[CreateRequestForPortfolioNavDateSecurityPrices]: Begin");
                    CreateRequestForPortfolioNavDateSecurityPrices(session, currentNavDate);
                    logger.Log(LogLevel.Info, "[CreateRequestForPortfolioNavDateSecurityPrices]: End");
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, ex.Message);
            }

            logger.Log(LogLevel.Info, "End");

            return Task.CompletedTask;
        }

        private void CreateRequestForPortfolioNavDateSecurityPrices(ISession session, NavDate currentNavDate)
        {
            using (IConnection connection = connectionFactory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    channel.QueueDeclare(Queues.SecurityPriceRequestQueue, true, false, false);

                    IList<PortfolioNavDate> portfolioNavDateList = portfolioNavDateRepository.GetPortfolioNavDateListByNavDate(session, currentNavDate);

                    foreach (var portfolioNavDate in portfolioNavDateList)
                    {
                        logger.Log(LogLevel.Info, $"Portfolio {portfolioNavDate.Portfolio.PortfolioId} for NavDate {portfolioNavDate.NavDate.Date}: Creating Security Price Requests");

                        IList<PortfolioNavDateSecurityPrice> portfolioNavDateSecurityPriceList = portfolioNavDateSecurityPriceRepository.GetPortfolioNavDateSecurityPriceListByPortfolioNavDate(session, portfolioNavDate)
                            .Where(p => p.PriceStatus == PriceStatus.ToBeRequested).ToList();

                        foreach (var portfolioNavDateSecurityPrice in portfolioNavDateSecurityPriceList)
                        {
                            SendSecurityPriceRequest(channel, session, portfolioNavDateSecurityPrice);
                        }

                        logger.Log(LogLevel.Info, $"Portfolio {portfolioNavDate.Portfolio.PortfolioId} for NavDate {portfolioNavDate.NavDate.Date}: {portfolioNavDateSecurityPriceList.Count} Security Price Requests created");
                    }
                }
            }
        }

        private void SendSecurityPriceRequest(IModel channel, ISession session, PortfolioNavDateSecurityPrice portfolioNavDateSecurityPrice)
        {
            var securityPriceRequest = new SecurityPriceRequest()
            {
                Symbol = portfolioNavDateSecurityPrice.PortfolioSecurity.Security.Symbol,
                NavDate = portfolioNavDateSecurityPrice.PortfolioNavDate.NavDate.Date,
                MessageId = Guid.NewGuid().ToString()
            };

            channel.PublishMessageOnQueue(securityPriceRequest, Queues.SecurityPriceRequestQueue);

            logger.Log(LogLevel.Info, $"SecurityPriceRequest has been sent: Symbol {securityPriceRequest.Symbol}, NavDate {securityPriceRequest.NavDate.Date}, MessageId {securityPriceRequest.MessageId}");

            portfolioNavDateSecurityPrice.PriceStatus = PriceStatus.Requested;
            portfolioNavDateSecurityPrice.UpdateDate = DateTime.Now;

            portfolioNavDateSecurityPriceRepository.SaveOrUpdate(session, portfolioNavDateSecurityPrice);
        }

        private void CreatePortfolioNavDateSecurityPrices(ISession session, NavDate currentNavDate)
        {
            IList<PortfolioNavDate> portfolioNavDateList = portfolioNavDateRepository.GetPortfolioNavDateListByNavDate(session, currentNavDate);

            foreach (var portfolioNavDate in portfolioNavDateList)
            {
                logger.Log(LogLevel.Info, $"[PortfolioNavDate: PortfolioId {portfolioNavDate.Portfolio.PortfolioId}, NavDate {portfolioNavDate.NavDate.Date}]: Creating PortfolioNavDateSecurityPrices");

                IList<Entities.PortfolioSecurity> portfolioSecurityList = portfolioSecurityRepository.GetPortfolioSecurityList(session, portfolioNavDate.Portfolio);
                logger.Log(LogLevel.Info, $"[PortfolioNavDate: PortfolioId {portfolioNavDate.Portfolio.PortfolioId}, NavDate {portfolioNavDate.NavDate.Date}]: {portfolioSecurityList.Count} PortfolioSecurity Count");

                IList<PortfolioNavDateSecurityPrice> portfolioNavDateSecurityPriceList = portfolioNavDateSecurityPriceRepository.GetPortfolioNavDateSecurityPriceListByPortfolioNavDate(session, portfolioNavDate);

                IDictionary<long, Entities.PortfolioSecurity> portfolioSecurityDictionary = portfolioSecurityList.ToDictionary(x => x.PortfolioSecurityId);
                IDictionary<long, PortfolioNavDateSecurityPrice> portfolioNavDateSecurityPriceDictionary = portfolioNavDateSecurityPriceList.ToDictionary(x => x.PortfolioSecurity.PortfolioSecurityId);

                IList<long> missingPortfolioSecurityList = portfolioSecurityDictionary.Keys.Except(portfolioNavDateSecurityPriceDictionary.Keys).ToList();

                logger.Log(LogLevel.Info, $"[PortfolioNavDate: PortfolioId {portfolioNavDate.Portfolio.PortfolioId}, NavDate {portfolioNavDate.NavDate.Date}]: {portfolioNavDateSecurityPriceList.Count}/{portfolioSecurityList.Count} PortfolioSecurityPrices already been created");
                logger.Log(LogLevel.Info, $"[PortfolioNavDate: PortfolioId {portfolioNavDate.Portfolio.PortfolioId}, NavDate {portfolioNavDate.NavDate.Date}]: {missingPortfolioSecurityList.Count} PortfolioSecurityPrices need to be created");

                using (var transaction = session.BeginTransaction())
                {
                    foreach (var portfolioSecurityId in missingPortfolioSecurityList)
                    {
                        Entities.PortfolioSecurity portfolioSecurity = portfolioSecurityDictionary[portfolioSecurityId];

                        PortfolioNavDateSecurityPrice portfolioNavDateSecurityPrice = new PortfolioNavDateSecurityPrice()
                        {
                            PortfolioNavDate = portfolioNavDate,
                            PortfolioSecurity = portfolioSecurity,
                            PriceStatus = PriceStatus.ToBeRequested,
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now,
                            Currency = portfolioSecurity.Security.Currency
                        };

                        portfolioNavDateSecurityPriceRepository.SaveOrUpdate(session, portfolioNavDateSecurityPrice);
                    }

                    transaction.Commit();

                    logger.Log(LogLevel.Info, $"[PortfolioNavDate: PortfolioId {portfolioNavDate.Portfolio.PortfolioId}, NavDate {portfolioNavDate.NavDate.Date}]: {missingPortfolioSecurityList.Count} PortfolioSecurityPrices created");
                }

            }


        }

        private void CreatePortfolioNavDates(ISession session, NavDate currentNavDate)
        {
            using (var transaction = session.BeginTransaction())
            {
                IList<Portfolio> portfolioList = portfolioRepository.GetPortfolioList(session);
                logger.Log(LogLevel.Info, $"Portfolio count: {portfolioList.Count}");

                IList<PortfolioNavDate> portfolioNavDateList = portfolioNavDateRepository.GetPortfolioNavDateListByNavDate(session, currentNavDate);
                logger.Log(LogLevel.Info, $"Portfolio with NavDate {currentNavDate.Date} count: {portfolioNavDateList.Count}");

                foreach (var portfolio in portfolioList)
                {
                    if (portfolioNavDateList.Any(p => p.Portfolio.PortfolioId == portfolio.PortfolioId))
                    {
                        continue;
                    }

                    PortfolioNavDate portfolioNavDate = new PortfolioNavDate()
                    {
                        NavDate = currentNavDate,
                        Portfolio = portfolio,
                        CreateDate = DateTime.Now
                    };

                    portfolioNavDateRepository.SaveOrUpdate(session, portfolioNavDate);

                    logger.Log(LogLevel.Info, $"Created NavDate {currentNavDate.Date} for Portfolio {portfolio.PortfolioId}");
                }

                transaction.Commit();
            }
        }
    }
}
