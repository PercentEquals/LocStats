using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using MobileApp.Managers;

namespace MobileApp.Fragments
{
    public class FragmentRegistration : AndroidX.Fragment.App.Fragment
    {
        private TextView _infoText;
        private readonly Action _registerCallback;

        public FragmentRegistration(Action registerCallback)
        {
            this._registerCallback = registerCallback;
        }
        public override void OnStart()
        {
            base.OnStart();
                
            _infoText = View.FindViewById<TextView>(Resource.Id.infoText);
            _infoText.Visibility = ViewStates.Invisible;

            View.FindViewById<Button>(Resource.Id.buttonRegister).Click += RegisterButtonClick;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.activity_register, container, false);
        }

        private void RegisterButtonClick(object sender, EventArgs e)
        {
            EditText editTextEmail = View.FindViewById<EditText>(Resource.Id.editTextEmail);
            EditText editTextPassword = View.FindViewById<EditText>(Resource.Id.editTextPassword);

            FormManager.RunForm(editTextEmail.Text, editTextPassword.Text, _registerCallback, _infoText);
        }
    }
}