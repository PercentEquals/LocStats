using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocStatsBackendAPI.Entities.Responses
{
    public class AuthSuccessResponse : AuthResult
    {
        public AuthSuccessResponse()
        {
            base.Success = true;
        }
    }
}
