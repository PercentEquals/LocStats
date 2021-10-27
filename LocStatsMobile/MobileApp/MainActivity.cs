using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using Google.Android.Material.BottomNavigation;
using MobileApp.Fragments;
using MobileApp.Services;
using System.Collections.Generic;
using Android.Support.V4.Content;
using Android;
using Android.Support.V4.App;
using Android.Util;
using Google.Android.Material.Snackbar;
using Android.Net;
using Android.Preferences;
using MobileApp.Services.Sublocation;


namespace MobileApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        private const string Tag = "MainActivity";

        private List<AndroidX.Fragment.App.Fragment> _fragments;

        // Used in checking for runtime permissions.
        private const int RequestPermissionsRequestCode = 34;

        // The BroadcastReceiver used to listen from broadcasts from the service.
        private MyReceiver _myReceiver;

        // A reference to the service used to get location updates.
        public LocationUpdatesService Service;

        // Tracks the bound state of the service.
        public bool Bound;

        // Buttons state: is button enabled
        public bool RequestLocationUpdatesVal;
        public bool RemoveLocationUpdatesVal;

        // Monitors the state of the connection to the service.
        private CustomServiceConnection _serviceConnection;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _myReceiver = new MyReceiver { Context = this };
            _serviceConnection = new CustomServiceConnection { Activity = this };
            
            if (Utils.RequestingLocationUpdates(this))
            {
                if (!CheckPermissions())
                {
                    RequestPermissions();
                }
            }

            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);
            navigation.Visibility = ViewStates.Gone;

            _fragments = new List<AndroidX.Fragment.App.Fragment>
            {
                new FragmentLogIn(LogInCallback, RegisterFragmentCallback),
                new FragmentRegistration(RegisterCallback, CancelRegistrationCallback),
                new FragmentLocalization(RequestLocationCallback, RemoveLocationCallback),
                new FragmentDataShow()
            };                 
            LoadFragment(0);
        }

        private void RegisterCallback()
        {
            LoadFragment(2);

            Android.App.AlertDialog infoBox = new Android.App.AlertDialog.Builder(this)
                .SetPositiveButton("Zamknij", (sender, args) =>
                {})
                .SetMessage("Rejestracja zakończona sukcesem!")
                .SetTitle("Rejestracja")
                .Show();

            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.Visibility = ViewStates.Visible;
        }

        private void RequestLocationCallback()
        {
            if (!CheckPermissions())
            {
                RequestPermissions();
            }
            else
            {
                Service.RequestLocationUpdates();
            }
        }

        private void RemoveLocationCallback()
        {
            Service.RemoveLocationUpdates();
        }

        protected override void OnStart()
        {
            base.OnStart();

            PreferenceManager.GetDefaultSharedPreferences(this).RegisterOnSharedPreferenceChangeListener(this);

            RestoreButtonsState();

            // Bind to the service. If the service is in foreground mode, this signals to the service
            // that since this activity is in the foreground, the service can exit foreground mode.
            BindService(new Intent(this, typeof(LocationUpdatesService)), _serviceConnection, Bind.AutoCreate);
        }

        public void RestoreButtonsState()
        {
            // Restore the state of the buttons when the activity (re)launches.
            SetButtonsState(Utils.RequestingLocationUpdates(this));
        }

        protected override void OnResume()
        {
            base.OnResume();
            LocalBroadcastManager.GetInstance(this).RegisterReceiver(_myReceiver,
                new IntentFilter(LocationUpdatesService.ActionBroadcast));
        }

        protected override void OnPause()
        {
            LocalBroadcastManager.GetInstance(this).UnregisterReceiver(_myReceiver);
            base.OnPause();
        }

        protected override void OnStop()
        {
            if (Bound)
            {
                // Unbind from the service. This signals to the service that this activity is no longer
                // in the foreground, and the service can respond by promoting itself to a foreground
                // service.
                UnbindService(_serviceConnection);
                Bound = false;
            }
            PreferenceManager.GetDefaultSharedPreferences(this)
                .UnregisterOnSharedPreferenceChangeListener(this);
            base.OnStop();
        }

        private void LoadFragment(int fragmentIndex)
        {
            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.frameLayoutMain, _fragments[fragmentIndex], _fragments[fragmentIndex].GetType().ToString())
                .Commit();
        }
        
        private void LogInCallback()
        {
            LoadFragment(2);

            Android.App.AlertDialog infoBox = new Android.App.AlertDialog.Builder(this)
                .SetPositiveButton("Zamknij", (sender, args) =>
                { })
                .SetMessage("Logowanie zakończone sukcesem!")
                .SetTitle("Logowanie")
                .Show();

            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.Visibility = ViewStates.Visible;
        }

        private void RegisterFragmentCallback()
        {
            LoadFragment(1);
        }

        private void CancelRegistrationCallback()
        {
            LoadFragment(0);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.navigation_localize:
                    LoadFragment(2);
                    return true;

                case Resource.Id.navigation_dashboard:
                    LoadFragment(3);
                    return true;
            }
            return false;
        }

        /**
	     * Returns the current state of the permissions needed.
	     */
        private bool CheckPermissions()
        {
            return PermissionChecker.PermissionGranted == ContextCompat.CheckSelfPermission(this,
                Manifest.Permission.AccessFineLocation);
        }

        private void RequestPermissions()
        {
            var shouldProvideRationale = ActivityCompat.ShouldShowRequestPermissionRationale(this,
                Manifest.Permission.AccessFineLocation);

            // Provide an additional rationale to the user. This would happen if the user denied the
            // request previously, but didn't check the "Don't ask again" checkbox.
            if (shouldProvideRationale)
            {
                Log.Info(Tag, "Displaying permission rationale to provide additional context.");
                Snackbar.Make(
                        FindViewById(Resource.Layout.activity_localization),
                        Resource.String.permission_rationale,
                        Snackbar.LengthIndefinite)
                    .SetAction(Resource.String.ok, (obj) => {
                        // Request permission
                        ActivityCompat.RequestPermissions(this,
                            new[] { Manifest.Permission.AccessFineLocation },
                            RequestPermissionsRequestCode);
                    })
                    .Show();
            }
            else
            {
                Log.Info(Tag, "Requesting permission");
                // Request permission. It's possible this can be auto answered if device policy
                // sets the permission in a given state or the user denied the permission
                // previously and checked "Never ask again".
                ActivityCompat.RequestPermissions(this,
                    new[] { Manifest.Permission.AccessFineLocation },
                    RequestPermissionsRequestCode);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
                         Android.Content.PM.Permission[] grantResults)
        {
            Log.Info(Tag, "onRequestPermissionResult");
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == RequestPermissionsRequestCode)
            {
                if (grantResults.Length <= 0)
                {
                    // If user interaction was interrupted, the permission request is cancelled and you
                    // receive empty arrays.
                    Log.Info(Tag, "User interaction was cancelled.");
                }
                else if (grantResults[0] == PermissionChecker.PermissionGranted)
                {
                    // Permission was granted.
                    Service.RequestLocationUpdates();
                }
                else
                {
                    // Permission denied.
                    SetButtonsState(false);
                    Snackbar.Make(
                            FindViewById(Resource.Layout.activity_localization),
                            Resource.String.permission_denied_explanation,
                            Snackbar.LengthIndefinite)
                            .SetAction(Resource.String.settings, (obj) => {
                                // Build intent that displays the App settings screen.
                                Intent intent = new Intent();
                                intent.SetAction(Android.Provider.Settings.ActionApplicationDetailsSettings);
                                var uri = Uri.FromParts("package", PackageName, null);
                                intent.SetData(uri);
                                intent.SetFlags(ActivityFlags.NewTask);
                                StartActivity(intent);
                            })
                            .Show();
                }
            }
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            // Update the buttons state depending on whether location updates are being requested.
            if (key.Equals(Utils.KeyRequestingLocationUpdates))
            {
                SetButtonsState(sharedPreferences.GetBoolean(Utils.KeyRequestingLocationUpdates, false));
            }
        }

        private void SetButtonsState(bool requestingLocationUpdates)
        {
            if (requestingLocationUpdates)
            {
                RequestLocationUpdatesVal = false;
                RemoveLocationUpdatesVal = true;
            }
            else
            {
                RequestLocationUpdatesVal = true;
                RemoveLocationUpdatesVal = false;
            }

            FragmentLocalization myFragment = (FragmentLocalization)SupportFragmentManager.FindFragmentByTag(typeof(FragmentLocalization).ToString());
            
            if (!(myFragment is {IsVisible: true})) return;
            
            myFragment.RequestLocationUpdatesButton.Enabled = RequestLocationUpdatesVal;
            myFragment.RemoveLocationUpdatesButton.Enabled = RemoveLocationUpdatesVal;
        }
    }
}