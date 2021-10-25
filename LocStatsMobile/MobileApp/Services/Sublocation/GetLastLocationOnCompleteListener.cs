using Android.Gms.Tasks;
using Android.Locations;
using Android.Util;
using Java.Lang;

namespace MobileApp.Services.Sublocation
{
    internal class GetLastLocationOnCompleteListener : Object, IOnCompleteListener
    {
        public LocationUpdatesService Service { get; set; }

        public void OnComplete(Task task)
        {
            if (task.IsSuccessful && task.Result != null)
            {
                Service.Location = (Location)task.Result;
            }
            else
            {
                Log.Warn(Service.Tag, "Failed to get location.");
            }
        }
    }
}