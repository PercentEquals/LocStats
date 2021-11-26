using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Util;
using MobileApp.Database;
using MobileApp.Managers;

namespace MobileApp.Fragments
{
    public class FragmentLocalization : AndroidX.Fragment.App.Fragment, IOnMapReadyCallback
    {
        private DateTime selectedDateFrom = DateTime.Now;
        private DateTime selectedDateTo = DateTime.Now;
        private Button selectedDateToBtn;
        private Button selectedDateFromBtn;
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
        }

        public override void OnStart()
        {
            base.OnStart();

            selectedDateFromBtn = View.FindViewById<Button>(Resource.Id.buttonSelectDateFrom);
            selectedDateFromBtn.Click += _buttonClickSelectDateFrom;
            selectedDateToBtn = View.FindViewById<Button>(Resource.Id.buttonSelectDateTo);
            selectedDateToBtn.Click += _buttonClickSelectDateTo;

            selectedDateFromBtn.Text = selectedDateFrom.ToString("dd'-'MM'-'yyyy");
            selectedDateToBtn.Text = selectedDateTo.ToString("dd'-'MM'-'yyyy");

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

            ClearMap();
            ClearPolyLines();
            InitializeUiSettingsOnMap();
            LoadPolyLines();
        }

        private void LoadPolyLinesFromDB()
        {
            IEnumerable<PolyLinesModel> plms = lr.GetAllPolyLines();
            AddPolylinePoints(plms);
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

        public void AddPolylinePoints(IEnumerable<PolyLinesModel> plms)
        {
            foreach (PolyLinesModel plm in plms)
            {
                AddPolylinePoint(plm.Latitude, plm.Longitude);
            }
        }

        public void ClearPolyLines()
        {
            polyOptions = new PolylineOptions();
        }

        public void ClearMap()
        {
            googleMap.Clear();
        }

        private void _buttonClickSelectDateFrom(object sender, EventArgs e)
        {
            new DatePickerFragment(delegate (DateTime time)
                {
                    selectedDateFrom = time;
                    selectedDateFromBtn.Text = selectedDateFrom.ToString("dd'-'MM'-'yyyy");
                    LoadPolyLines();

                }, selectedDateFrom)
                .Show(FragmentManager, DatePickerFragment.TAG);
        }

        private void _buttonClickSelectDateTo(object sender, EventArgs e)
        {
            new DatePickerFragment(delegate (DateTime time)
                {
                    selectedDateTo = time;
                    selectedDateToBtn.Text = selectedDateTo.ToString("dd'-'MM'-'yyyy");
                    LoadPolyLines();

                }, selectedDateTo)
                .Show(FragmentManager, DatePickerFragment.TAG);
        }

        private async void LoadPolyLines()
        {
            if (selectedDateFrom <= selectedDateTo)
            {
                var GPSresults = await ConnectionManager.GetGPSData(selectedDateFrom, selectedDateTo);
                if (GPSresults.success)
                {
                    lr.DeleteAllPolyLines();
                    lr.AddPolyLines(GPSresults.polyLines);
                    AddPolylinePoints(GPSresults.polyLines);
                }
                else
                {
                    Log.Error("Błąd Polylines response", GPSresults.errors);
                }
            }
            else
            {
                Log.Error("Polylines date error", "Dates are not in order");
            }
        }
    }
}