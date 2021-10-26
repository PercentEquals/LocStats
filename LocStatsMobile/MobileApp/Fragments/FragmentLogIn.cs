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
        private readonly Action _registerCallback;

        public FragmentLogIn(Action logInCallback, Action registerCallback)
        {
            this._logInCallback = logInCallback;
            this._registerCallback = registerCallback;
        }

        public override void OnStart()
        {
            base.OnStart();

            Button button = View.FindViewById<Button>(Resource.Id.buttonLogIn);
            button.Click += LogInButtonClick;

            button = View.FindViewById<Button>(Resource.Id.buttonRegister);
            button.Click += RegisterButtonClick;

            _infoText = View.FindViewById<TextView>(Resource.Id.infoText);
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.activity_log_in, container, false);
        }

        private void LogInButtonClick(object sender, EventArgs e)
        {
            EditText editTextUsername = View.FindViewById<EditText>(Resource.Id.editTextUsername);
            EditText editTextPassword = View.FindViewById<EditText>(Resource.Id.editTextPassword);

            FormManager.RunForm(_logInCallback, _infoText, editTextPassword.Text, editTextUsername.Text, ConnectionManager.Register);
        }

        private void RegisterButtonClick(object sender, EventArgs e)
        {
            _registerCallback();
        }
    }
}