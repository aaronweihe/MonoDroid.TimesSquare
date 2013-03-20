using System;
using System.Collections.Generic;
using System.Globalization;
using Android.Content;
using Android.Util;
using Android.Widget;

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

        public readonly DateTime Today = DateTime.Now;
        public DateTime SelectedCal;
        public DateTime MinCal;
        public DateTime MaxCal;
        private DateTime _monthCounter;

        public readonly IListener Listener;

        public IOnDateSelectedListener DateListener;

        public DateTime SelectedDate
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
            base.CacheColorHint = base.Resources.GetColor(Resource.Color.calendar_bg);
            _monthNameFormat = base.Resources.GetString(Resource.String.month_name_format);
            WeekdayNameFormat = base.Resources.GetString(Resource.String.day_name_format);
            FullDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;
            Listener = new CellClickedListener(context, this);
        }

        public void Init(DateTime selectedDate, DateTime minDate, DateTime maxDate)
        {
            if (selectedDate == null | minDate == null | maxDate == null) {
                throw new ArgumentException("All dates must be non-null. " +
                                                   Debug(selectedDate, minDate, maxDate));
            }
            if (selectedDate == DateTime.MinValue || minDate == DateTime.MinValue | maxDate == DateTime.MinValue) {
                throw new ArgumentException("All dates can't be DateTime.MinValue " +
                                                   Debug(selectedDate, minDate, maxDate));
            }
            if (minDate.CompareTo(maxDate) > 0) {
                throw new ArgumentException("Min date must be before max date. " +
                                                   Debug(selectedDate, minDate, maxDate));
            }
            if (selectedDate.CompareTo(minDate) < 0 || selectedDate.CompareTo(maxDate) > 0) {
                throw new ArgumentException("Selected date must be between min date and max date. " +
                                                   Debug(selectedDate, minDate, maxDate));
            }

            //Clear previous state.
            Cells.Clear();
            Months.Clear();

            SelectedCal = selectedDate;
            MinCal = minDate;
            MaxCal = maxDate;
            SelectedCal = SetMidnight(SelectedCal);
            MinCal = SetMidnight(MinCal);
            MaxCal = SetMidnight(MaxCal);

            // maxDate is exclusive: bump back to the previous day so if maxDate is the first of a month,
            // we don't accidentally include that month in the view.
            MaxCal = MaxCal.AddMinutes(-1);

            _monthCounter = MinCal;
            int maxMonth = MaxCal.Month;
            int maxYear = MaxCal.Year;
            int selectedYear = SelectedCal.Year;
            int selectedMonth = SelectedCal.Month;
            int selectedIndex = 0;

            while (_monthCounter.Month <= maxMonth
                    || _monthCounter.Year < maxYear
                   && _monthCounter.Year < maxYear + 1) {
                var month = new MonthDescriptor(_monthCounter.Month,
                                                            _monthCounter.Year,
                                                            _monthCounter.ToString(_monthNameFormat));
                Cells.Add(GetMonthCells(month, _monthCounter, SelectedCal));
                Logr.D("Adding month {0}", month);
                if (selectedMonth == month.Month && selectedYear == month.Year) {
                    selectedIndex = Months.Count;
                }
                Months.Add(month);
                _monthCounter = _monthCounter.AddMonths(1);
            }
            MyAdapter.NotifyDataSetChanged();
            if (selectedIndex != 0) {
                SmoothScrollToPosition(selectedIndex);
            }
        }

        private static DateTime SetMidnight(DateTime date)
        {
            return date.Subtract(date.TimeOfDay);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if (Months.Count == 0) {
                throw new InvalidOperationException(
                    "Must have at least one month to display. Did you forget to call Init()?");
            }
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }

        public List<List<MonthCellDescriptor>> GetMonthCells(MonthDescriptor month, DateTime startCal, DateTime selectedDate)
        {
            var cells = new List<List<MonthCellDescriptor>>();
            var cal = new DateTime(startCal.Year, startCal.Month, 1);
            var firstDayOfWeek = (int)cal.DayOfWeek;
            cal = cal.AddDays((int)DayOfWeek.Sunday - firstDayOfWeek);
            while ((cal.Month < month.Month + 1 || cal.Year < month.Year)
                   && cal.Year <= month.Year) {
                Logr.D("Building week row starting at {0}", cal);
                var weekCells = new List<MonthCellDescriptor>();
                cells.Add(weekCells);
                for (int i = 0; i < 7; i++) {
                    DateTime date = cal;
                    bool isCurrentMonth = cal.Month == month.Month;
                    bool isSelected = isCurrentMonth && IsSameDate(cal, selectedDate);
                    bool isSelectable = isCurrentMonth && IsBetweenDates(cal, MinCal, MaxCal);
                    bool isToday = IsSameDate(cal, Today);
                    int value = cal.Day;
                    var cell =
                        new MonthCellDescriptor(date, isCurrentMonth, isSelectable, isSelected, isToday, value);
                    if (isSelected) {
                        SelectedCell = cell;
                    }
                    weekCells.Add(cell);
                    cal = cal.AddDays(1);
                }
            }
            return cells;
        }

        public static bool IsBetweenDates(DateTime date, DateTime minCal, DateTime maxCal)
        {
            return (date.Equals(minCal) || date.CompareTo(minCal) > 0) // >= minCal
                   && date.CompareTo(maxCal) < 0; // && < maxCal
        }

        public static bool IsSameDate(DateTime cal, DateTime selectedDate)
        {
            return cal.Month == selectedDate.Month
                   && cal.Year == selectedDate.Year
                   && cal.Day == selectedDate.Day;
        }

        private static string Debug(DateTime selectedDate, DateTime minDate, DateTime maxDate)
        {
            return "selectedDate: " + selectedDate + "\nminDate: " + minDate + "\nmaxDate: " + maxDate;
        }

        public void SetOnDateSelectedListener(IOnDateSelectedListener listener)
        {
            DateListener = listener;
        }
    }

    public interface IOnDateSelectedListener
    {
        void OnDateSelected(DateTime date);
    }
}