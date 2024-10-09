using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Interfaces.DAL
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();

        Task<IEnumerable<T>> GetAll(params Expression<Func<T, object>>[] includes);

        Task<T> GetById(object id);

        Task<T> GetById(object id, params Expression<Func<T, object>>[] includes);

        T Add(T entity);

        void Update(T entity);

        Task<bool> Delete(object id);
    }
}
