using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Util;

namespace MonoDroid.TimesSquare
{
    public class CalendarPickerView : ListView
    {
        public readonly MonthAdapter MyAdapter;
        public readonly List<MonthDescriptor> Months = new List<MonthDescriptor>();
        public readonly List<List<List<MonthCellDescriptor>>> Cells = new List<List<List<MonthCellDescriptor>>>();
        private readonly string _monthNameFormat;
        public readonly string WeekdayNameFormat;
        public readonly string FullDateFormat;
        public MonthCellDescriptor SelectedCell;

        public readonly Java.Util.Calendar Today = Java.Util.Calendar.Instance;
        public readonly Java.Util.Calendar SelectedCal = Java.Util.Calendar.Instance;
        public readonly Java.Util.Calendar MinCal = Java.Util.Calendar.Instance;
        public readonly Java.Util.Calendar MaxCal = Java.Util.Calendar.Instance;
        private readonly Java.Util.Calendar _monthCounter = Java.Util.Calendar.Instance;

        public readonly IListener Listener;

        public Java.Util.Calendar SelectedDate
        {
            get { return SelectedCal; }
        }

        public CalendarPickerView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            ResourceIdManager.UpdateIdValues();
            MyAdapter = new MonthAdapter(context, this);
			base.Adapter = MyAdapter;
            base.Divider = null;
			base.DividerHeight = 0;
            base.SetBackgroundColor(base.Resources.GetColor(Resource.Color.calendar_bg));
            base.CacheColorHint = base.Resources.GetColor (Resource.Color.calendar_bg);
            _monthNameFormat = base.Resources.GetString(Resource.String.month_name_format);
            WeekdayNameFormat = base.Resources.GetString(Resource.String.day_name_format);
            FullDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;
            Listener = new CellClickedListener(context, this);
        }

        public void Init(Date selectedDate, Date minDate, Date maxDate)
        {
            if (selectedDate == null | minDate == null | maxDate == null) {
                throw new IllegalArgumentException("All dates must be non-null. " +
                                                   Debug(selectedDate, minDate, maxDate));
            }
            if (selectedDate.Time == 0 || minDate.Time == 0 | maxDate.Time == 0) {
                throw new IllegalArgumentException("All dates must be non-zero. " +
                                                   Debug(selectedDate, minDate, maxDate));
            }
            if (minDate.After(maxDate)) {
                throw new IllegalArgumentException("Min date must be before max date. " +
                                                   Debug(selectedDate, minDate, maxDate));
            }
            if (selectedDate.Before(minDate) || selectedDate.After(maxDate)) {
                throw new IllegalArgumentException("Selected date must be between min date and max date. " +
                                                   Debug(selectedDate, minDate, maxDate));
            }

            //Clear previous state.
            Cells.Clear();
            Months.Clear();

            SelectedCal.Time = selectedDate;
            MinCal.Time = minDate;
            MaxCal.Time = maxDate;
            SetMidnight(SelectedCal);
            SetMidnight(MinCal);
            SetMidnight(MaxCal);

            // maxDate is exclusive: bump back to the previous day so if maxDate is the first of a month,
            // we don't accidentally include that month in the view.
            MaxCal.Add(CalendarField.Minute, -1);

            _monthCounter.Time = MinCal.Time;
            int maxMonth = MaxCal.Get(CalendarField.Month);
            int maxYear = MaxCal.Get(CalendarField.Year);
            int selectedYear = SelectedCal.Get(CalendarField.Year);
            int selectedMonth = SelectedCal.Get(CalendarField.Month);
            int selectedIndex = 0;

            while ((_monthCounter.Get(CalendarField.Month) <= maxMonth
                    || _monthCounter.Get(CalendarField.Year) < maxYear)
                   && _monthCounter.Get(CalendarField.Year) < maxYear + 1) {
                var time = Convert.ToDateTime(_monthCounter.Time.ToLocaleString());
                var month = new MonthDescriptor(_monthCounter.Get(CalendarField.Month),
                                                            _monthCounter.Get(CalendarField.Year),
                                                            time.ToString(_monthNameFormat));
                Cells.Add(GetMonthCells(month, _monthCounter, SelectedCal));
                Logr.D("Adding month {0}", month);
                if (selectedMonth == month.Month && selectedYear == month.Year) {
                    selectedIndex = Months.Count;
                }
                Months.Add(month);
                _monthCounter.Add(CalendarField.Month, 1);
            }
            MyAdapter.NotifyDataSetChanged();
            if (selectedIndex != 0) {
                SmoothScrollToPosition(selectedIndex);
            }
        }

        private static void SetMidnight(Java.Util.Calendar cal)
        {
            cal.Set(CalendarField.HourOfDay, 0);
            cal.Set(CalendarField.Minute, 0);
            cal.Set(CalendarField.Second, 0);
            cal.Set(CalendarField.Millisecond, 0);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if (Months.Count == 0) {
                throw new IllegalStateException(
                    "Must have at least one month to display. Did you forget to call Init()?");
            }
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }

        public List<List<MonthCellDescriptor>> GetMonthCells(MonthDescriptor month, Java.Util.Calendar startCal, Java.Util.Calendar selectedDate)
        {
            Java.Util.Calendar cal = Java.Util.Calendar.Instance;
            cal.Time = startCal.Time;
            var cells = new List<List<MonthCellDescriptor>>();
            cal.Set(CalendarField.DayOfMonth, 1);
            int firstDayOfWeek = cal.Get(CalendarField.DayOfWeek);
            cal.Add(CalendarField.Date, Java.Util.Calendar.Sunday - firstDayOfWeek);
            while ((cal.Get(CalendarField.Month) < month.Month + 1 || cal.Get(CalendarField.Year) < month.Year)
                   && cal.Get(CalendarField.Year) <= month.Year) {
                Logr.D("Building week row starting at {0}", cal.Time);
                var weekCells = new List<MonthCellDescriptor>();
                cells.Add(weekCells);
                for (int i = 0; i < 7; i++) {
                    Date date = cal.Time;
                    bool isCurrentMonth = cal.Get(CalendarField.Month) == month.Month;
                    bool isSelected = isCurrentMonth && IsSameDate(cal, selectedDate);
                    bool isSelectable = isCurrentMonth && IsBetweenDates(cal, MinCal, MaxCal);
                    bool isToday = IsSameDate(cal, Today);
                    int value = cal.Get(CalendarField.DayOfMonth);
                    var cell =
                        new MonthCellDescriptor(date, isCurrentMonth, isSelectable, isSelected, isToday, value);
                    if (isSelected) {
                        SelectedCell = cell;
                    }
                    weekCells.Add(cell);
                    cal.Add(CalendarField.Date, 1);
                }
            }
            return cells;
        }

        public static bool IsBetweenDates(Date date, Java.Util.Calendar minCal, Java.Util.Calendar maxCal)
        {
            Date min = minCal.Time;
            return (date.Equals(min) || date.After(min)) // >= minCal
                   && date.Before(maxCal.Time); // && < maxCal
        }

        public static bool IsBetweenDates(Java.Util.Calendar cal, Java.Util.Calendar minCal, Java.Util.Calendar maxCal)
        {
            Date date = cal.Time;
            return IsBetweenDates(date, minCal, maxCal);
        }

        public bool IsSameDate(Java.Util.Calendar cal, Java.Util.Calendar selectedDate)
        {
            return cal.Get(CalendarField.Month) == selectedDate.Get(CalendarField.Month)
                   && cal.Get(CalendarField.Year) == selectedDate.Get(CalendarField.Year)
                   && cal.Get(CalendarField.DayOfMonth) == selectedDate.Get(CalendarField.DayOfMonth);
        }

        private static string Debug(Date selectedDate, Date minDate, Date maxDate)
        {
            return "selectedDate: " + selectedDate + "\nminDate: " + minDate + "\nmaxDate: " + maxDate;
        }



    }
}