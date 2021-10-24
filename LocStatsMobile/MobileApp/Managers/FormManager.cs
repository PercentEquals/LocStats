using Android.Views;
using Android.Widget;
using System;

namespace MobileApp.Managers
{
    static class FormManager
    {
        public static void RunForm(string email, string password, Action callback, TextView info)
        {
            string message = "";
            if (ValidationManager.CheckUserInput(ref message, email, password))
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