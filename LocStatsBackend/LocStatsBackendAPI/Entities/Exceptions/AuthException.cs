using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace LocStatsBackendAPI.Entities.Exceptions
{
    [Serializable]
    public class AuthException : BaseException
    {
        public List<string> Errors { get; set; }

        public AuthException()
        {
        }

        public AuthException(string message) : base(message)
        {
            Errors = new List<string> { message };
        }

        public AuthException(string message, IEnumerable<string> errors, bool includeMessage = false) : base(message)
        {
            if (includeMessage)
            {
                errors = errors.Prepend(message);
            }

            Errors = errors.ToList();
        }

        public AuthException(string message, Exception inner) : base(message, inner)
        {
            Errors = new List<string> { message };
        }

        protected AuthException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
