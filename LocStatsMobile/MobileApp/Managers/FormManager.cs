using Android.Views;
using Android.Widget;
using System;

namespace MobileApp.Managers
{
    static class FormManager
    {
        public static void RunForm(Action callback, TextView info, string password, string username, string email = null)
        {
            string message = "";
            if (ValidationManager.CheckUserInput(ref message, username, password, email))
            {
                callback();
            }
            else
            {
                info.Text = message;
                info.Visibility = ViewStates.Visible;
            }
        }
    }
}