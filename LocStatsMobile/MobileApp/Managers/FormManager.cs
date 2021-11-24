using Android.Views;
using Android.Widget;
using System;
using System.Threading.Tasks;

namespace MobileApp.Managers
{
    static class FormManager
    {
        public static async void RunForm(Action callback, TextView info, string password, string username,
            Func<string, string, string, Task<(bool success, string errorMessage)>> func, Button button1, Button button2, string email = null)
        {
            string message = "";
            if (ValidationManager.CheckUserInput(ref message, username, password, email))
            {
                button1.Enabled = false;
                button2.Enabled = false;

                var response = await func(email, username, password);

                button1.Enabled = true;
                button2.Enabled = true;

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

        public static async void RunForm(Action callback, TextView info, string password, string username,
            Func<string, string, Task<(bool success, string errorMessage)>> func, Button button1, Button button2, string email = null)
        {
            string message = "";
            if (ValidationManager.CheckUserInput(ref message, username, password, email))
            {
                button1.Enabled = false;
                button2.Enabled = false;

                var response = await func(username, password);

                button1.Enabled = true;
                button2.Enabled = true;

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