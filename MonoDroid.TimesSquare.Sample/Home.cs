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
            calendar.Init(DateTime.Now, nextYear)
                .InMode(CalendarPickerView.SelectionMode.Single)
                .WithSelectedDate(DateTime.Now);
        }
    }
}


