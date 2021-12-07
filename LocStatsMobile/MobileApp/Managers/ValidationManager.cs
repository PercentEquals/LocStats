using System.Text.RegularExpressions;

namespace MobileApp.Managers
{
    public static class ValidationManager
    {
        public static bool CheckUserInput(ref string message, string username, string password, string email = null)
        {
            if (email != null)
            {
                Regex correctEmail = new Regex(@"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@"
                    + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$");

                Match emailMatch = correctEmail.Match(email);

                if (!emailMatch.Success)
                {
                    message = "Invalid email";
                    return false;
                }
            }

            if (username.Length == 0 || username.Length > 16)
            {
                message = "Invalid username";
                return false;
            }

            Regex correctPassword = new Regex(@"^(?=.*?[A-Z])(?=(.*[a-z]){1,})(?=(.*[\d]){1,})(?=(.*[\W]){1,})(?!.*\s).{8,}$");

            Match passwordMatch = correctPassword.Match(password);

            if (password.Length > 16 || !passwordMatch.Success)
            {
                message = "Invalid password";
                return false;
            }
            return true;
        }

    }
}