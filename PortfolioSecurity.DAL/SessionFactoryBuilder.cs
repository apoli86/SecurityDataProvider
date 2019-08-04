using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Options;
using NHibernate;
using PortfolioSecurity.DAL.Mapping;
using PortfolioSecurity.Entities.Configuration;

namespace PortfolioSecurity.DAL
{
    public class SessionFactoryBuilder
    {
        private readonly IOptions<ConnectionStrings> connectionStrings;

        public SessionFactoryBuilder(IOptions<ConnectionStrings> connectionStrings)
        {
            this.connectionStrings = connectionStrings;
        }

        public ISessionFactory Build()
        {
            return Fluently.Configure()
                    .Database(MsSqlConfiguration.MsSql2012.ConnectionString(connectionStrings.Value.PortfolioSecurityConnectionString))
                    .Mappings(m => m.FluentMappings.AddFromAssemblyOf<NavDateMap>())
                    .CurrentSessionContext("call").BuildSessionFactory();
        }
    }
}
