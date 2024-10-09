using SecMan.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Interfaces.BL
{
    public interface ICurrentUserServices
    {
        List<AppPermissions>? AppPermissions { get; }
        bool IsLoggedIn { get; }
        UserDetails? UserDetails { get; }
        ulong UserId { get; }
        string UserName { get; }
    }
}
