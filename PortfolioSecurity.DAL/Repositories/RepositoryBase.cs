using NHibernate;

namespace PortfolioSecurity.DAL.Repositories
{
    public abstract class RepositoryBase<T> where T : new()
    {
        public void SaveOrUpdate(ISession session, T obj)
        {
            session.SaveOrUpdate(obj);
            session.Flush();
        }

        public void Insert(IStatelessSession session, T obj)
        {
            session.Insert(obj);
        }

        public void Update(IStatelessSession session, T obj)
        {
            session.Update(obj);
        }
    }
}
