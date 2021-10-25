using Android.OS;
using Android.Views;
using Android.Widget;
using MobileApp.Managers;
using System;
using AndroidX.Fragment.App;

namespace MobileApp.Fragments
{
    public class FragmentRegistration : AndroidX.Fragment.App.Fragment
    {
        private TextView _infoText;
        private readonly Action _registerCallback;
        private readonly Action _cancelCallback;

       

        public FragmentRegistration(Action registerCallback, Action cancelCallback)
        {
            _registerCallback = registerCallback;
            _cancelCallback = cancelCallback;
        }

        public override void OnStart()
        {
            base.OnStart();
                
            _infoText = View.FindViewById<TextView>(Resource.Id.infoText);
            _infoText.Visibility = ViewStates.Invisible;


            View.FindViewById<Button>(Resource.Id.buttonRegister).Click += RegisterButtonClick;
            View.FindViewById<Button>(Resource.Id.buttonCancel).Click += CancelButtonClick;
        }

        private void CancelButtonClick(object sender, EventArgs e)
        {
            _cancelCallback();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.activity_register, container, false);
        }

        private void RegisterButtonClick(object sender, EventArgs e)
        {
            EditText editTextEmail = View.FindViewById<EditText>(Resource.Id.editTextEmail);
            EditText editTextPassword = View.FindViewById<EditText>(Resource.Id.editTextPassword);
            EditText editTextUsername = View.FindViewById<EditText>(Resource.Id.editTextUsername);

            FormManager.RunForm(_registerCallback, _infoText, editTextPassword.Text, editTextUsername.Text, editTextEmail.Text);
        }

        
    }
}