using Android.Content;
using Android.OS;
using Java.Lang;

namespace MobileApp.Services.Sublocation
{
    internal class CustomServiceConnection : Object, IServiceConnection
    {
        public MainActivity Activity { get; set; }
        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            LocationUpdatesServiceBinder binder = (LocationUpdatesServiceBinder)service;
            Activity.Service = binder.GetLocationUpdatesService();
            Activity.Bound = true;
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            Activity.Service = null;
            Activity.Bound = false;
        }
    }
}