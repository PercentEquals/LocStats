using Android.Views;
using Android.Widget;
using System;
using System.Threading.Tasks;
using System.Net.Http;

namespace MobileApp.Managers
{
    static class FormManager
    {
        public static async void RunForm(Action callback, TextView info, string password, string username,
            Func<string, string, string, Task<(bool success, string token, string errorMessage)>> func, string email = null)
        {
            string message = "";
            if (ValidationManager.CheckUserInput(ref message, username, password, email))
            {
                var response = await func(email, username, password);

                if (response.success)
                {
                    callback();
                }
                else
                {
                    info.Text = response.errorMessage;
                    info.Visibility = ViewStates.Visible;
                }
                
            }
            else
            {
                info.Text = message;
                info.Visibility = ViewStates.Visible;
            }
        }
    }
}