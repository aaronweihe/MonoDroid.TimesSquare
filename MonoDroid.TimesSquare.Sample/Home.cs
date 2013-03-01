using System;
using Java.Util;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace MonoDroid.TimesSquare.Sample
{
	[Activity (Label = "MonoDroid.TimesSquare.Sample", MainLauncher = true)]
	public class Home : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.calendar_picker);

			var nextYear = Calendar.Instance;
			nextYear.Add (CalendarField.Year, 1);

			var calendar = FindViewById<CalendarPickerView> (Resource.Id.calendar_view);
			calendar.Init (new Date (), new Date (), nextYear.Time);
			//FindViewById<Button>(Resource.Id.done_button) .Click +=

		}
	}
}


