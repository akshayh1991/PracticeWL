using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.Devices;
using Microsoft.VisualBasic.Logging;
using SecMan.Data.DAL;
using SecMan.Data.SQLCipher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Db _context;
        private GenericRepository<User> _userRepository;
        private GenericRepository<SQLCipher.Role> _roleRepository;

        private RoleRepository _iRoleRepository;
        private UserRepository _iUserRepository;


        public UnitOfWork(Db context)
        {
            _context = context;
        }

        public GenericRepository<User> UserRepository
        {
            get
            {
                return _userRepository ??= new GenericRepository<User>(_context);
            }
        }

        public GenericRepository<SQLCipher.Role> RoleRepository
        {
            get
            {
                return _roleRepository ??= new GenericRepository<SQLCipher.Role>(_context);
            }
        }

        public IRoleRepository IRoleRepository
        {
            get
            {
                return _iRoleRepository ??= new RoleRepository(_context);
            }
        }

        public IUserRepository IUserRepository
        {
            get
            {
                return _iUserRepository ??= new UserRepository(_context);
            }
        }

        public IPasswordRepository IPasswordRepository => new PasswordRepository(_context);

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }


        public async Task<int> SaveChangesAync()
        {
            return await _context.SaveChangesAsync();
        }

    }
}
