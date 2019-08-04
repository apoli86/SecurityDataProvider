using NHibernate;

namespace SecurityDataProvider.DAL.Repositories
{
    public abstract class RepositoryBase<T> where T : new()
    {
        public void SaveOrUpdate(ISession session, T obj)
        {
            session.SaveOrUpdate(obj);
            session.Flush();
        }
    }
}
