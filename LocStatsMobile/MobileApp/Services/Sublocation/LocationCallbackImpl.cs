using Android.Gms.Location;

namespace MobileApp.Services.Sublocation
{
    internal class LocationCallbackImpl : LocationCallback
    {
        public LocationUpdatesService Service { get; set; }
        public override void OnLocationResult(LocationResult result)
        {
            base.OnLocationResult(result);
            Service.OnNewLocation(result.LastLocation);
        }
    }
}