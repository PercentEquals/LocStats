using Android.OS;

namespace MobileApp.Services.Sublocation
{
    /**
	 * Class used for the client Binder.  Since this service runs in the same process as its
	 * clients, we don't need to deal with IPC.
	 */
    internal class LocationUpdatesServiceBinder : Binder
    {
        private readonly LocationUpdatesService _service;

        public LocationUpdatesServiceBinder(LocationUpdatesService service)
        {
            _service = service;
        }

        public LocationUpdatesService GetLocationUpdatesService()
        {
            return _service;
        }
    }
}