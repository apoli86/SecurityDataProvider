using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Options;
using NHibernate;
using SecurityDataProvider.DAL.Mapping;
using SecurityDataProvider.Entities.Configuration;

namespace SecurityDataProvider.DAL
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
                    .Database(MsSqlConfiguration.MsSql2012.ConnectionString(connectionStrings.Value.SecurityDataProviderConnectionString))
                    .Mappings(m => m.FluentMappings.AddFromAssemblyOf<RequestMap>())
                    .CurrentSessionContext("call").BuildSessionFactory();
        }
    }
}
