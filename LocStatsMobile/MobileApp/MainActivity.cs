using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.BottomNavigation;
using MobileApp.Fragments;
using System.Collections.Generic;


namespace MobileApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        List<AndroidX.Fragment.App.Fragment> fragments;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

           
            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);
            navigation.Visibility = ViewStates.Gone;

            fragments = new List<AndroidX.Fragment.App.Fragment>();

            fragments.Add(new FragmentLogIn(logInCallback));
            fragments.Add(new FragmentRegistration(registerCallback));
            fragments.Add(new FragmentLocalization());
            fragments.Add(new FragmentDataShow());

            loadFragment(0);
        }

        private void loadFragment(int fragmentIndex)
        {
            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.frameLayoutMain, fragments[fragmentIndex])
                .Commit();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        
        private void logInCallback()
        {
            loadFragment(2);

            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.Visibility = ViewStates.Visible;
        }

        private void registerCallback()
        {
            loadFragment(0);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.navigation_localize:
                    loadFragment(2);
                    return true;

                case Resource.Id.navigation_dashboard:
                    loadFragment(3);
                    return true;
            }
            return false;
        }
    }
}

