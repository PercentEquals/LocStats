using Android.OS;
using Android.Views;

namespace MobileApp.Fragments
{
    public class FragmentDataShow : AndroidX.Fragment.App.Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.activity_data_show, container, false);
        }
    }
}