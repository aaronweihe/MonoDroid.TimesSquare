using System;
using System.Collections.Generic;
using Android.Content;
using Android.Content.PM;
using Android.Widget;
using Android.App;
using Android.OS;
using Java.Sql;

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

            var btnSingle = FindViewById<Button>(Resource.Id.button_single);
            var btnMulti = FindViewById<Button>(Resource.Id.button_multi);
            var btnRange = FindViewById<Button>(Resource.Id.button_range);
            var btnDialog = FindViewById<Button>(Resource.Id.button_dialog);

            btnSingle.Click += (s, o) =>
            {
                btnSingle.Enabled = false;
                btnMulti.Enabled = true;
                btnRange.Enabled = true;

                calendar.Init(DateTime.Now, nextYear)
                    .InMode(CalendarPickerView.SelectionMode.Single)
                    .WithSelectedDate(DateTime.Now);
            };

            btnMulti.Click += (s, o) =>
            {
                btnSingle.Enabled = true;
                btnMulti.Enabled = false;
                btnRange.Enabled = true;

                var dates = new List<DateTime>();
                for (int i = 0; i < 5; i++) {
                    dates.Add(DateTime.Now.AddDays(3*i));
                }
                calendar.Init(DateTime.Now, nextYear)
                    .InMode(CalendarPickerView.SelectionMode.Multi)
                    .WithSelectedDates(dates);
            };

            btnRange.Click += (s, o) =>
            {
                btnSingle.Enabled = true;
                btnMulti.Enabled = true;
                btnRange.Enabled = false;

                var dates = new List<DateTime>() {DateTime.Now.AddDays(3), DateTime.Now.AddDays(5)};
                calendar.Init(DateTime.Now, nextYear)
                    .InMode(CalendarPickerView.SelectionMode.Range)
                    .WithSelectedDates(dates);
            };

            btnDialog.Click += (s, o) =>
            {
                var dialogView =
                    (CalendarPickerView) LayoutInflater.Inflate(Resource.Layout.dialog, null, false);
                dialogView.Init(DateTime.Now, nextYear);
                new AlertDialog.Builder(this).SetTitle("I'm a dialog!")
                    .SetView(dialogView)
                    .SetNeutralButton("Dismiss",
                        (sender, args) => { }).Create().Show();
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


