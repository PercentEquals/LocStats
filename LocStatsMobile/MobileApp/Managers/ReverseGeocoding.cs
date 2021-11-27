using System;
using System.Linq;
using Android.Util;
using MobileApp.Database;
using Xamarin.Essentials;
using System.Threading.Tasks;
using Android.Gms.Maps.Model;

namespace MobileApp.Managers
{
    class ReverseGeocoding
    {
        public static async Task<(bool success, LatLng coordinates, string url, Placemark pm, string errors)> GetReverseGeocodingAsync(DateTime selectedDateFrom, DateTime selectedDateTo)
        {
            var mostFrequentLocationResult = await ConnectionManager.GetMostFrequentLocation(selectedDateFrom, selectedDateTo);
            if (!mostFrequentLocationResult.success)
            {
                return (false, null, null, null, mostFrequentLocationResult.errors);
            }

            MostFrequentLocationModel mflm = mostFrequentLocationResult.mostFrequentLocation;
            LatLng geocoordinates = new LatLng(mflm.Latitude, mflm.Longitude);
            Log.Info("Load Most Frequent Location", $"{mflm.Latitude} : {mflm.Longitude} : {mflm.URL}");

            try
            {
                var placemarks = await Geocoding.GetPlacemarksAsync(mflm.Latitude, mflm.Longitude);
                var placemark = placemarks?.FirstOrDefault();
                if (placemark != null)
                {
                    var geocodeAddress =
                        $"AdminArea:       {placemark.AdminArea}\n" +
                        $"CountryCode:     {placemark.CountryCode}\n" +
                        $"CountryName:     {placemark.CountryName}\n" +
                        $"FeatureName:     {placemark.FeatureName}\n" +
                        $"Locality:        {placemark.Locality}\n" +
                        $"PostalCode:      {placemark.PostalCode}\n" +
                        $"SubAdminArea:    {placemark.SubAdminArea}\n" +
                        $"SubLocality:     {placemark.SubLocality}\n" +
                        $"SubThoroughfare: {placemark.SubThoroughfare}\n" +
                        $"Thoroughfare:    {placemark.Thoroughfare}\n";

                    Console.WriteLine(geocodeAddress);
                    return (true, geocoordinates, mflm.URL, placemark, "");
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                return (false, geocoordinates, mflm.URL, null, fnsEx.Message);
            }
            catch (Exception ex)
            {
                return (false, geocoordinates, mflm.URL, null, ex.Message);
            }
            return (false, geocoordinates, mflm.URL, null, "");
        }
    }
}