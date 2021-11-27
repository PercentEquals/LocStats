using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Text.Format;
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
        private long lastTimestamp, polyLineTimeOffset = 10 * 60;
        private LocationRepo lr;
        private GoogleMap googleMap;
        List<PolylineOptions> polyOptions = new List<PolylineOptions>();
        List<MarkerOptions> markers = new List<MarkerOptions>();

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

            MarkerOptions newMarker = new MarkerOptions();
            newMarker.SetPosition(markerPosition)
                .SetTitle(title);

            markers.Add(newMarker);
            foreach (MarkerOptions m in markers)
            {
                googleMap.AddMarker(m);
            }
        }

        public void InsertNextPolyLinePoint(double lat, double lgn)
        {
            LatLng pointPosition = new LatLng(lat, lgn);
            ClearMap();
            PolylineOptions lastPolyOption = polyOptions[^1];
            lastPolyOption.Add(pointPosition);
            foreach (PolylineOptions po in polyOptions)
            {
                googleMap.AddPolyline(po);
            }
        }

        private string GetText(long timestamp)
        {
            DateTime ReadTime = new DateTime(timestamp * 10000000 + DateTime.UnixEpoch.Ticks, DateTimeKind.Local);
            return ReadTime.ToString("r");
        }

        public void AddPolyLinePoint(PolyLinesModel plm)
        {
            if (plm.Timestamp - lastTimestamp > polyLineTimeOffset)
            {
                Log.Info("NewPolyLine", $"{plm.Timestamp}, {lastTimestamp}, {GetText(polyLineTimeOffset)}");
                polyOptions.Add(new PolylineOptions());
                lastTimestamp = plm.Timestamp;
            }

            InsertNextPolyLinePoint(plm.Latitude, plm.Longitude);
            AddMarker(plm.Latitude, plm.Longitude, GetText(plm.Timestamp));
        }

        public void AddPolylinePoints(IEnumerable<PolyLinesModel> plms)
        {
            polyOptions.Add(new PolylineOptions());
            foreach (PolyLinesModel plm in plms)
            {
                AddPolyLinePoint(plm);
            }
        }

        public void ClearPolyLines()
        {
            polyOptions = new List<PolylineOptions>();
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
                    markers = new List<MarkerOptions>();
                    lastTimestamp = 0;
                    lr.DeleteAllPolyLines();
                    ClearPolyLines();
                    lr.AddPolyLines(GPSresults.polyLines);
                    AddPolylinePoints(GPSresults.polyLines);
                    LoadMostFrequentLocation();
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
        private async void LoadMostFrequentLocation()
        {
            var mostFrequentLocationResult = await ConnectionManager.GetMostFrequentLocation(selectedDateFrom, selectedDateTo);
            if (mostFrequentLocationResult.success)
            {
                MostFrequentLocationModel mflm = mostFrequentLocationResult.mostFrequentLocation;
                Log.Info("Load Most Frequent Location", $"{mflm.Latitude} : {mflm.Longitude} : {mflm.URL}");
            }
            else
            {
                Log.Error("Błąd MostFrequentLocation response", mostFrequentLocationResult.errors);
            }
        }
    }
}