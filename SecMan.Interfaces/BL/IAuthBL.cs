using SecMan.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Interfaces.BL
{
    public interface IAuthBL
    {
        Task<ServiceResponse<LoginServiceResponse>> LoginAsync(LoginRequest model);

        Task<ServiceResponse<LoginServiceResponse>> ValidateSessionAsync(string ssoSessionId);
    }
}
