using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileApp.Managers;

namespace MobileApp.Fragments
{
    public class FragmentRegistration : AndroidX.Fragment.App.Fragment
    {
        private TextView infoText;
        private Action registerCallback;

        public FragmentRegistration(Action registerCallback)
        {
            this.registerCallback = registerCallback;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            
            // Create your fragment here
        }

        public override void OnStart()
        {
            base.OnStart();

            infoText = View.FindViewById<TextView>(Resource.Id.infoText);
            infoText.Visibility = ViewStates.Invisible;

            View.FindViewById<Button>(Resource.Id.buttonRegister).Click += register;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            
            return inflater.Inflate(Resource.Layout.activity_register, container, false);

        }

        private void register(object sender, EventArgs e)
        {
            EditText editTextEmail = View.FindViewById<EditText>(Resource.Id.editTextEmail);
            EditText editTextPassword = View.FindViewById<EditText>(Resource.Id.editTextPassword);

            string message = "";
            if (ValidationManager.checkUserInput(ref message, editTextEmail.Text, editTextPassword.Text))
            {
                registerCallback();
            }
            else
            {
                infoText.Text = message;
                infoText.Visibility = ViewStates.Visible;
            }
            
        }
    }
}