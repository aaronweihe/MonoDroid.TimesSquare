using System;
using Android.Content.PM;
using Android.Widget;
using Android.App;
using Android.OS;

namespace MonoDroid.TimesSquare.Sample
{
    [Activity(Label = "MonoDroid.TimesSquare.Sample", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden)]
    public class Home : Activity
    {
        private const string TAG = "MonoDroid.TimesSquare.Sample";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.calendar_picker);

            var nextYear = DateTime.Now.AddYears(1);

            var calendar = FindViewById<CalendarPickerView>(Resource.Id.calendar_view);
            calendar.Init(DateTime.Now, DateTime.Now.AddMinutes(-1), nextYear);

            var btnSingle = FindViewById<Button>(Resource.Id.button_single);
            var btnMulti = FindViewById<Button>(Resource.Id.button_multi);
            var btnRange = FindViewById<Button>(Resource.Id.button_range);
            var btnDialog = FindViewById<Button>(Resource.Id.button_dialog);

            btnSingle.Click += (s, o) =>
            {
                btnSingle.Enabled = false;
                btnMulti.Enabled = true;
                btnRange.Enabled = true;
                btnDialog.Enabled = true;

                calendar.Init(DateTime.Now, DateTime.Now.AddMinutes(-1), nextYear);
            };

            FindViewById<Button>(Resource.Id.done_button).Click += (s, o) =>
            {
                calendar = FindViewById<CalendarPickerView>(Resource.Id.calendar_view);
                Logr.D(TAG, "Selected time in millis: " + calendar.SelectedDate);
                string toast = "Selected: " + calendar.SelectedDate;
                Toast.MakeText(this, toast, ToastLength.Short).Show();
            };
        }
    }
}


