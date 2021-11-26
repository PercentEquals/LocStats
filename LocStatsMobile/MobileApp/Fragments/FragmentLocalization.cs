using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Java.Util;
using MobileApp.Database;
using MobileApp.Managers;

namespace MobileApp.Fragments
{
    public class FragmentLocalization : AndroidX.Fragment.App.Fragment, IOnMapReadyCallback
    {
        public Button RequestLocationUpdatesButton;
        public Button RemoveLocationUpdatesButton;
        private readonly Action _requestLocationUpdatesCallback;
        private readonly Action _removeLocationUpdatesCallback;
        private LocationRepo lr;
        private GoogleMap googleMap;
        PolylineOptions polyOptions = new PolylineOptions();

        public FragmentLocalization(Action requestUpdate, Action removeUpdate)
        {
            _requestLocationUpdatesCallback = requestUpdate;
            _removeLocationUpdatesCallback = removeUpdate;
        }

        void InitializeUiSettingsOnMap()
        {
            googleMap.UiSettings.MyLocationButtonEnabled = true;
            googleMap.UiSettings.CompassEnabled = true;
            googleMap.UiSettings.ZoomControlsEnabled = true;
            googleMap.MyLocationEnabled = true;

        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            List<string> latlngs = new List<string>();
            foreach (LatLng point in polyOptions.Points)
            {
                latlngs.Add(point.Latitude + ":" + point.Longitude);
            }
            outState.PutStringArrayList("latlngs", latlngs);
            base.OnSaveInstanceState(outState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.activity_localization, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            RequestLocationUpdatesButton = View.FindViewById(Resource.Id.request_location_updates_button) as Button;
            RemoveLocationUpdatesButton = View.FindViewById(Resource.Id.remove_location_updates_button) as Button;
            RequestLocationUpdatesButton.Click += delegate { _requestLocationUpdatesCallback(); };
            RemoveLocationUpdatesButton.Click += delegate { _removeLocationUpdatesCallback(); };

            if (savedInstanceState != null)
            {
                IList<string> latlngs = savedInstanceState.GetStringArrayList("latlngs");
                foreach (string latlng in latlngs)
                {
                    int divIdx = latlng.IndexOf(":");
                    double lat = Convert.ToDouble(latlng.Substring(0, divIdx));
                    double lng = Convert.ToDouble(latlng.Substring(divIdx+1));
                    polyOptions.Add(new LatLng(lat, lng));
                }
            }
        }

        public override void OnStart()
        {
            base.OnStart();

            var mapFrag = MapFragment.NewInstance();
            Activity.FragmentManager.BeginTransaction()
                .Add(Resource.Id.map_container, mapFrag, "map_fragment")
                .Commit();

            mapFrag.GetMapAsync(this);

            View.FindViewById<TextView>(Resource.Id.textViewLoggedAs).Text = ConnectionManager.CurrentUsername;
            lr = LocationRepo.Instance;
            lr.SetFragLocal(this);
        }

        public override void OnResume()
        {
            base.OnResume();
            ((MainActivity)Activity).RestoreButtonsState();
            RequestLocationUpdatesButton.Enabled = ((MainActivity)Activity).RequestLocationUpdatesVal;
            RemoveLocationUpdatesButton.Enabled = ((MainActivity)Activity).RemoveLocationUpdatesVal;
        }

        public void OnMapReady(GoogleMap map)
        {
            googleMap = map; 
            
            InitializeUiSettingsOnMap();
            googleMap.AddPolyline(polyOptions);
        }

        public void AddMarker(double lat, double lgn, string title = "")
        {
            LatLng markerPosition = new LatLng(lat, lgn);

            var newMarker = new MarkerOptions();
            newMarker.SetPosition(markerPosition)
                .SetTitle(title);
            googleMap.AddMarker(newMarker);
        }

        public void AddPolylinePoint(double lat, double lgn)
        {
            LatLng pointPosition = new LatLng(lat, lgn);
            ClearMap();
            polyOptions.Add(pointPosition);
            googleMap.AddPolyline(polyOptions);
        }

        public void ClearMap()
        {
            googleMap.Clear();
        }
    }
}