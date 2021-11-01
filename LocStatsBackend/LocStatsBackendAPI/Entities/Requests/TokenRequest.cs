using System.ComponentModel.DataAnnotations;

namespace LocStatsBackendAPI.Entities.Requests
{
    public class TokenRequest
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}