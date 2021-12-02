using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using MobileApp.Managers;
using System;
using System.Net;

namespace MobileApp.Fragments
{
    public class FragmentMostFrequentLocation : AndroidX.Fragment.App.Fragment
    {
        private DateTime selectedDateFrom = DateTime.Now;
        private DateTime selectedDateTo = DateTime.Now;

        private Button selectedDateToBtn;
        private Button selectedDateFromBtn;

        private Action<string, string> infoBoxCallback;

        public FragmentMostFrequentLocation(Action<string, string> infoBoxCallback)
        {
            this.infoBoxCallback = infoBoxCallback;
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

            LoadMostFrequentLocation();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.activity_most_frequent_location, container, false);
        }

        private void _buttonClickSelectDateFrom(object sender, EventArgs e)
        {
            new DatePickerFragment(delegate (DateTime time)
            {
                selectedDateFrom = time;
                selectedDateFromBtn.Text = selectedDateFrom.ToString("dd'-'MM'-'yyyy");

                LoadMostFrequentLocation();
                

            }, selectedDateFrom)
          .Show(FragmentManager, DatePickerFragment.TAG);
        }

        private void _buttonClickSelectDateTo(object sender, EventArgs e)
        {
            new DatePickerFragment(delegate (DateTime time)
            {
                selectedDateTo = time;
                selectedDateToBtn.Text = selectedDateTo.ToString("dd'-'MM'-'yyyy");


                LoadMostFrequentLocation();

            }, selectedDateTo)
          .Show(FragmentManager, DatePickerFragment.TAG);
        }

        private async void LoadMostFrequentLocation()
        {
            if (selectedDateFrom <= selectedDateTo)
            {
                var revGeo = await ReverseGeocoding.GetReverseGeocodingAsync(selectedDateFrom, selectedDateTo);

                if (revGeo.success)
                {
                    string info = "";

                    info += revGeo.pm.CountryName?.Trim() != "" ? $"{revGeo.pm.CountryCode}\n" : "";
                    info += revGeo.pm.AdminArea?.Trim() != "" ? $"{revGeo.pm.AdminArea}\n" : "";
                    info += revGeo.pm.SubAdminArea?.Trim() != "" ? $"{revGeo.pm.SubAdminArea}\n" : "";
                    info += revGeo.pm.Locality?.Trim() != "" ? $"{revGeo.pm.Locality}\n" : "";
                    info += revGeo.pm.SubLocality?.Trim() != "" ? $"{revGeo.pm.SubLocality}\n" : "";

                    View.FindViewById<TextView>(Resource.Id.textInfoLocation).Text = info;



                    View.FindViewById<ImageView>(Resource.Id.imageView).SetImageBitmap(GetImageBitmapFromUrl(revGeo.url));
                }
                else if (revGeo.coordinates == null)
                {
                    View.FindViewById<TextView>(Resource.Id.textInfoLocation).Text = "";
                    View.FindViewById<ImageView>(Resource.Id.imageView).SetImageBitmap(null);
                }
            }
            else
            {
                infoBoxCallback("Błąd", "Podane przedział datowy jest nieprawidłowy!");

                View.FindViewById<TextView>(Resource.Id.textInfoLocation).Text = "";
                View.FindViewById<ImageView>(Resource.Id.imageView).SetImageBitmap(null);
            }
        }

        private Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("User-Agent: Other");
                try
                {
                    var imageBytes = webClient.DownloadData(url);
                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                    }
                }
                catch(WebException e)
                {
                    infoBoxCallback("Błąd", e.Message);
                }
            }
            return imageBitmap;
        }
    }
}