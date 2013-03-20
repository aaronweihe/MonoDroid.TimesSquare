using System;
using Android.Widget;
using Android.App;
using Android.OS;

namespace MonoDroid.TimesSquare.Sample
{
	[Activity (Label = "MonoDroid.TimesSquare.Sample", MainLauncher = true)]
	public class Home : Activity
	{
	    private const string TAG = "MonoDroid.TimesSquare.Sample";

	    protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.calendar_picker);

	        var nextYear = DateTime.Now.AddYears(1);

			var calendar = FindViewById<CalendarPickerView> (Resource.Id.calendar_view);
			calendar.Init (DateTime.Now, DateTime.Now.AddMinutes(-1), nextYear);

		    FindViewById<Button>(Resource.Id.done_button).Click += (s, o) =>
		        {
		            Logr.D(TAG, "Selected time in millis: " + calendar.SelectedDate);
		            string toast = "Selected: " + calendar.SelectedDate;
                    Toast.MakeText(this, toast, ToastLength.Short).Show();
		        };
		}
	}
}


