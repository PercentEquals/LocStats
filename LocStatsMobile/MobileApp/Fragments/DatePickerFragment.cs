using Android.App;
using Android.OS;
using Android.Widget;
using System;

namespace MobileApp.Fragments
{
    public class DatePickerFragment : AndroidX.Fragment.App.DialogFragment, DatePickerDialog.IOnDateSetListener
    {
        public static readonly string TAG = "X:" + typeof(DatePickerFragment).Name.ToUpper();

        Action<DateTime> _dateSelectedHandler = delegate { };

        DateTime _selectedTime;

        public DatePickerFragment(Action<DateTime> onDateSelected, DateTime selectedTime)
        {
            _dateSelectedHandler = onDateSelected;
            _selectedTime = selectedTime;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            
            DatePickerDialog dialog = new DatePickerDialog(Activity,
                this,
                _selectedTime.Year,
                _selectedTime.Month - 1,
                _selectedTime.Day);

            return dialog;
        }

        public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth)
        {
            DateTime selectedDate = new DateTime(year, monthOfYear + 1, dayOfMonth);
            _dateSelectedHandler(selectedDate);
        }
    }
}