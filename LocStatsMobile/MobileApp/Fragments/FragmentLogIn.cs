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

namespace MobileApp.Fragments
{
    public class FragmentLogIn : AndroidX.Fragment.App.Fragment
    {
        private Action logInCallback;

        public FragmentLogIn(Action logInCallback)
        {
            this.logInCallback = logInCallback;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

           
        }

        public override void OnStart()
        {
            base.OnStart();

            Button button = View.FindViewById<Button>(Resource.Id.buttonLogIn);
            button.Click += logInButtonClick;
        }

        private void logInButtonClick(object sender, EventArgs e)
        {
            EditText editTextEmail = View.FindViewById<EditText>(Resource.Id.editTextEmail);
            EditText editTextPassword = View.FindViewById<EditText>(Resource.Id.editTextPassword);

            logInCallback();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            
            return inflater.Inflate(Resource.Layout.activity_log_in, container, false);

        }
    }
}