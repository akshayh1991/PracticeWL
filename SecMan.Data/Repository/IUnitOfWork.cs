using SecMan.Data.DAL;
using SecMan.Data.SQLCipher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.Repository
{
    public interface IUnitOfWork 
    {
        IRoleRepository IRoleRepository { get; }

        IUserRepository IUserRepository { get; }
        IPasswordRepository IPasswordRepository { get; }

        Task<int> SaveChangesAync();

        int SaveChanges();
    }
}
