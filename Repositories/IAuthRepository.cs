using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSign.Models;

namespace VSign.Repositories
{
    public interface IAuthRepository
    {
        Client FindClient(string clientId);
        Task<AuthenticationResult> AuthenticateUser(string UserName, string Password);
    }
}
