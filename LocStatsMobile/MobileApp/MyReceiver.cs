using Android.Content;
using Android.Locations;
using Android.Widget;
using MobileApp.Services;

namespace MobileApp
{
	/**
	 * Receiver for broadcasts sent by {@link LocationUpdatesService}.
	 */
    internal class MyReceiver : BroadcastReceiver
    {
        public Context Context { get; set; }
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.GetParcelableExtra(LocationUpdatesService.ExtraLocation) is Location location)
            {
                Toast.MakeText(Context, Utils.GetLocationText(location), ToastLength.Short).Show();
            }

        }
    }
}