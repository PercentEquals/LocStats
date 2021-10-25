using Android.OS;
using Android.Views;
using Android.Widget;
using MobileApp.Managers;
using System;

namespace MobileApp.Fragments
{
    public class FragmentLogIn : AndroidX.Fragment.App.Fragment
    {
        private TextView _infoText;
        private readonly Action _logInCallback;

        public FragmentLogIn(Action logInCallback)
        {
            this._logInCallback = logInCallback;
        }

        public override void OnStart()
        {
            base.OnStart();

            Button button = View.FindViewById<Button>(Resource.Id.buttonLogIn);
            button.Click += LogInButtonClick;

            _infoText = View.FindViewById<TextView>(Resource.Id.infoText);
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.activity_log_in, container, false);
        }

        private void LogInButtonClick(object sender, EventArgs e)
        {
            EditText editTextEmail = View.FindViewById<EditText>(Resource.Id.editTextEmail);
            EditText editTextPassword = View.FindViewById<EditText>(Resource.Id.editTextPassword);
            
            FormManager.RunForm(editTextEmail.Text, editTextPassword.Text, _logInCallback, _infoText);
        }
    }
}