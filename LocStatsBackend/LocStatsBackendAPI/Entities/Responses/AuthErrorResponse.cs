using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocStatsBackendAPI.Entities.Responses
{
    public class AuthErrorResponse : AuthResult
    {
        public AuthErrorResponse(string error)
        {
            base.Token = null;
            base.RefreshToken = null;
            base.Success = false;
            base.Errors = new List<string> { error };
        }

        public AuthErrorResponse(IEnumerable<string> error)
        {
            base.Token = null;
            base.RefreshToken = null;
            base.Success = false;
            base.Errors = error.ToList();
        }
    }
}
