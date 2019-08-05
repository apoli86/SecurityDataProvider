using NHibernate;
using SecurityDataProvider.DAL.Repositories;
using SecurityDataProvider.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecurityDataProvider.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly ISecurityRepository securityRepository;
        private readonly ISessionFactory dbSessionFactory;

        public SecurityService(ISecurityRepository securityRepository, ISessionFactory dbSessionFactory)
        {
            this.securityRepository = securityRepository;
            this.dbSessionFactory = dbSessionFactory;
        }

        public void AddSecurityList(ISession session, IList<Entities.Security> securityList)
        {
            IList<Entities.Security> securityListFromDb = securityRepository.GetSecurityListByRequestDate(session, DateTime.Today);

            IDictionary<string, Entities.Security> securityFromDbDictionary = securityListFromDb.ToDictionary(s => s.Symbol?.ToUpperInvariant());
            ILookup<string, Entities.Security> securityFromRequestDictionary = securityList.ToLookup(s => s.Symbol?.ToUpperInvariant());

            IList<string> securityFromRequestKeys = securityFromRequestDictionary.Select(x => x.Key).ToList();
            IList<string> newSymbolList = securityFromRequestKeys.Except(securityFromDbDictionary.Keys).ToList();

            using (var ssession = dbSessionFactory.OpenStatelessSession())
            using (var transaction = ssession.BeginTransaction())
            {
                foreach (string symbol in newSymbolList)
                {
                    Entities.Security security = securityFromRequestDictionary[symbol].FirstOrDefault();

                    securityRepository.Insert(ssession, security);
                }

                transaction.Commit();
            }

        }

        public IList<Security> GetSecurityListByRequestDate(ISession session, DateTime requestDate)
        {
            IList<Security> securityList = securityRepository.GetSecurityListByRequestDate(session, requestDate);

            return securityList;
        }

        public Security GetSecurityBySymbol(ISession session, string symbol)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            string sanitizedSymbol = symbol.ToUpper();

            return securityRepository.GetLastSecurityBySymbol(session, sanitizedSymbol);
        }
    }
}
