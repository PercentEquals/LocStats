using System.Text.RegularExpressions;

namespace MobileApp.Managers
{
    public static class ValidationManager
    {
        public static bool CheckUserInput(ref string message, string email, string password)
        {
            Regex correctEmail = new Regex(@"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@" 
                + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$");

            Match emailMatch = correctEmail.Match(email);

            if (!emailMatch.Success)
            {
                message = "Niepoprawny adres email";
                return false;
            }

            if (password.Length < 8)
            {
                message = "Niepoprawne hasło";
                return false;
            }

            return true;
        }

    }
}