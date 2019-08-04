using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NLog;
using PortfolioSecurity.Batch.Jobs;
using PortfolioSecurity.Batch.Services;
using PortfolioSecurity.DAL;
using PortfolioSecurity.DAL.Repositories;
using PortfolioSecurity.Entities.Configuration;
using PortfolioSecurity.Services;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using RabbitMQ.Client;
using SecurityDataProvider.Common.Quartz;
using System;
using System.IO;
using System.Threading;

namespace PortfolioSecurity.Batch
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration configuration = BuildConfiguration();
            IServiceCollection serviceCollection = BuildServiceCollection(configuration);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger>();

            try
            {
                using (var signal = new ManualResetEvent(false))
                {
                    var scheduler = serviceProvider.GetService<IScheduler>();

                    scheduler.WaitUntilStarted();

                    scheduler.ScheduleJob<NavDateJob>().Every(new TimeSpan(0, 1, 0));
                    scheduler.ScheduleJob<SecurityListRequestJob>().Every(new TimeSpan(0, 1, 0));
                    scheduler.ScheduleJob<SecurityListResponseJob>().Every(new TimeSpan(0, 1, 0));
                    scheduler.ScheduleJob<SecurityPriceRequestJob>().Every(new TimeSpan(0, 0, 5));
                    scheduler.ScheduleJob<SecurityPriceResponseJob>().Every(new TimeSpan(0, 1, 0));

                    signal.WaitOne();
                }
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, e);
            }

        }

        private static IConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                          .SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: false)
                          .AddJsonFile($"secrets/appsettings.secret.json", optional: false, reloadOnChange: false);

            var configuration = builder.Build();

            return configuration;
        }

        private static IServiceCollection BuildServiceCollection(IConfiguration configuration)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();
            serviceCollection.Configure<RabbitMQCredential>(configuration.GetSection(nameof(RabbitMQCredential)));
            serviceCollection.Configure<ConnectionStrings>(configuration.GetSection(nameof(ConnectionStrings)));
            serviceCollection.AddSingleton<SessionFactoryBuilder, SessionFactoryBuilder>();


            serviceCollection.AddSingleton(provider =>
            {
                var sessionFactoryBuilder = provider.GetService<SessionFactoryBuilder>();

                return sessionFactoryBuilder.Build();
            });
            serviceCollection.AddSingleton<IJobFactory, ScheduledJobFactory>();
            serviceCollection.AddSingleton<INavDateCalculator, NavDateCalculator>();
            serviceCollection.AddSingleton<INavDateRepository, NavDateRepository>();
            serviceCollection.AddSingleton<INavDateService, NavDateService>();
            serviceCollection.AddSingleton<IPortfolioNavDateRepository, PortfolioNavDateRepository>();
            serviceCollection.AddSingleton<IPortfolioRepository, PortfolioRepository>();
            serviceCollection.AddSingleton<ISecurityRepository, SecurityRepository>();
            serviceCollection.AddSingleton<IPortfolioSecurityRepository, PortfolioSecurityRepository>();
            serviceCollection.AddSingleton<IPortfolioNavDateSecurityPriceRepository, PortfolioNavDateSecurityPriceRepository>();

            serviceCollection.AddSingleton<NavDateJob, NavDateJob>();
            serviceCollection.AddSingleton<SecurityListRequestJob, SecurityListRequestJob>();
            serviceCollection.AddSingleton<SecurityListResponseJob, SecurityListResponseJob>();
            serviceCollection.AddSingleton<SecurityPriceRequestJob, SecurityPriceRequestJob>();
            serviceCollection.AddSingleton<SecurityPriceResponseJob, SecurityPriceResponseJob>();

            serviceCollection.AddSingleton<IConnectionFactory>(provider =>
            {
                IOptions<RabbitMQCredential> credential = provider.GetService<IOptions<RabbitMQCredential>>();

                return new ConnectionFactory() { HostName = credential.Value.Host, UserName = credential.Value.Username, Password = credential.Value.Password };
            });

            serviceCollection.AddSingleton<IScheduler>(provider =>
            {
                var schedulerFactory = new StdSchedulerFactory();
                var scheduler = schedulerFactory.GetScheduler().Result;
                scheduler.JobFactory = provider.GetService<IJobFactory>();

                return scheduler;
            });

            serviceCollection.AddSingleton<ILogger>(provider => new NLog.LogFactory().GetLogger("Default"));

            return serviceCollection;
        }
    }
}
