using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        public bool IsMultiSelect { get; set; }
        public List<MonthCellDescriptor> SelectedCells = new List<MonthCellDescriptor>(); 

        public readonly DateTime Today = DateTime.Now;
        public List<DateTime> SelectedCals = new List<DateTime>(); 
        public DateTime MinCal;
        public DateTime MaxCal;
        private DateTime _monthCounter;

        public readonly IListener Listener;

        public IOnDateSelectedListener DateListener;

        public IEnumerable<DateTime> SelectedDates
        {
            get
            {
                var lstSelectedDates = SelectedCals.ToList();
                lstSelectedDates.Sort();
                return lstSelectedDates;
            }
        }

        public DateTime SelectedDate
        {
            get { return SelectedCals.Count > 0 ? SelectedCals[0] : DateTime.MinValue; }
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

        public void Init(DateTime minDate, DateTime maxDate)
        {
            Initialize(null, minDate, maxDate);
        }

        public void Init(DateTime selectedDate, DateTime minDate, DateTime maxDate)
        {
            IsMultiSelect = false;
            Initialize(new List<DateTime> {selectedDate}, minDate, maxDate);
        }

        public void Init(IEnumerable<DateTime> selectedDates, DateTime minDate, DateTime maxDate)
        {
            IsMultiSelect = true;
            Initialize(selectedDates, minDate, maxDate);
        }

        public void Initialize(IEnumerable<DateTime> selectedDates, DateTime minDate, DateTime maxDate)
        {
            if ( minDate == null || maxDate == null) {
                throw new ArgumentException("All dates must be non-null. " +
                                                   Debug(selectedDates, minDate, maxDate));
            }
            if (minDate == DateTime.MinValue || maxDate == DateTime.MinValue) {
                throw new ArgumentException("All dates must be greater than DateTime.MinValue. " +
                                                   Debug(selectedDates, minDate, maxDate));
            }
            if (minDate.CompareTo(maxDate) > 0) {
                throw new ArgumentException("Min date must be before max date. " +
                                                   Debug(selectedDates, minDate, maxDate));
            }

            SelectedCals.Clear();
            SelectedCells.Clear();
            if (selectedDates != null) {
                //Prevent from possible multiple enmuration of IEnumerable
                var lstSelectedDates = selectedDates.ToList();
                foreach (var t in lstSelectedDates) {
                    var date = t;
                    if (date == DateTime.MinValue) {
                        throw new ArgumentException("Selected date must be greater than DateTime.MinValue. " +
                                                    Debug(lstSelectedDates, minDate, maxDate));
                    }
                    if (date.CompareTo(minDate) < 0 || date.CompareTo(maxDate) > 0) {
                        throw new ArgumentException("Selected date must be between minDate and maxDate. " +
                                                    Debug(lstSelectedDates, minDate, maxDate));
                    }
                    //Need to test if the value is changed
                    date = SetMidnight(date);
                }
            }

            //Clear previous state.
            Cells.Clear();
            Months.Clear();

            MinCal = minDate;
            MaxCal = maxDate;
            MinCal = SetMidnight(MinCal);
            MaxCal = SetMidnight(MaxCal);

            // maxDate is exclusive: bump back to the previous day so if maxDate is the first of a month,
            // we don't accidentally include that month in the view.
            MaxCal = MaxCal.AddMinutes(-1);

            _monthCounter = MinCal;
            int maxMonth = MaxCal.Month;
            int maxYear = MaxCal.Year;
            int selectedIndex = 0;
            int todayIndex = 0;
            var today = DateTime.Now;

            while (_monthCounter.Month <= maxMonth
                    || _monthCounter.Year < maxYear
                   && _monthCounter.Year < maxYear + 1) {
                var month = new MonthDescriptor(_monthCounter.Month,
                                                            _monthCounter.Year,
                                                            _monthCounter.ToString(_monthNameFormat));
                Cells.Add(GetMonthCells(month, _monthCounter));
                Logr.D("Adding month {0}", month);
                if (selectedIndex == 0) {
                    if (SelectedCals.Any(cal => cal.Month == month.Month && cal.Year == month.Year)) {
                        selectedIndex = Months.Count;
                    }
                    if (selectedIndex == 0 && todayIndex == 0
                        && today.Month == month.Month && today.Year == month.Year) {
                        todayIndex = Months.Count;
                    }
                }
                Months.Add(month);
                _monthCounter = _monthCounter.AddMonths(1);
            }
            MyAdapter.NotifyDataSetChanged();
            if (selectedIndex != 0 || todayIndex != 0) {
                SmoothScrollToPosition(selectedIndex != 0 ? selectedIndex : todayIndex);
            }
        }

        public List<List<MonthCellDescriptor>> GetMonthCells(MonthDescriptor month, DateTime startCal)
        {
            var cells = new List<List<MonthCellDescriptor>>();
            var cal = new DateTime(startCal.Year, startCal.Month, 1);
            var firstDayOfWeek = (int)cal.DayOfWeek;
            cal = cal.AddDays((int) CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - firstDayOfWeek);
            while ((cal.Month < month.Month + 1 || cal.Year < month.Year)
                   && cal.Year <= month.Year) {
                Logr.D("Building week row starting at {0}", cal);
                var weekCells = new List<MonthCellDescriptor>();
                cells.Add(weekCells);
                for (int i = 0; i < 7; i++) {
                    DateTime date = cal;
                    bool isCurrentMonth = cal.Month == month.Month;
                    bool isSelected = isCurrentMonth && ContatinsDate(SelectedCals, cal);
                    bool isSelectable = isCurrentMonth && IsBetweenDates(cal, MinCal, MaxCal);
                    bool isToday = IsSameDate(cal, Today);
                    int value = cal.Day;
                    var cell =
                        new MonthCellDescriptor(date, isCurrentMonth, isSelectable, isSelected, isToday, value);
                    if (isSelected) {
                        SelectedCells.Add(cell);
                    }
                    weekCells.Add(cell);
                    cal = cal.AddDays(1);
                }
            }
            return cells;
        }

        public bool SetSelectedDate(DateTime date)
        {
            var monthCellWithMonthIndex = GetMonthCellWithIndexByDate(date);
            if (monthCellWithMonthIndex == null) {
                return false;
            }

            SelectedCells.Clear();
            monthCellWithMonthIndex.Cell.IsSelected = true;
            SelectedCells.Add(monthCellWithMonthIndex.Cell);
            SelectedCals.Clear();
            SelectedCals.Add(monthCellWithMonthIndex.Cell.DateTime);
            if (monthCellWithMonthIndex.MonthIndex != 0) {
                ScrolltoSelectedMonth(monthCellWithMonthIndex.MonthIndex);
            }

            MyAdapter.NotifyDataSetChanged();
            return true;
        }

        private void ScrolltoSelectedMonth(int selectedIndex)
        {
            SmoothScrollToPosition(selectedIndex);
        }
        
        private MonthCellWithMonthIndex GetMonthCellWithIndexByDate(DateTime date)
        {
            int index = 0;

            foreach (var monthCell in Cells) {
                foreach (
                    var actCell in
                        from weekCell in monthCell
                        from actCell in weekCell
                        where IsSameDate(actCell.DateTime, date)
                        select actCell) {
                    return new MonthCellWithMonthIndex(actCell, index);
                }
                index++;
            }
            return null;
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if (Months.Count == 0) {
                throw new InvalidOperationException(
                    "Must have at least one month to display. Did you forget to call Init()?");
            }
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }

        private static DateTime SetMidnight(DateTime date)
        {
            return date.Subtract(date.TimeOfDay);
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

        public static bool ContatinsDate(IEnumerable<DateTime> selectedCals, DateTime cal)
        {
            return selectedCals.Any(selectedCal => IsSameDate(cal, selectedCal));
        }

        private static string Debug(IEnumerable<DateTime> selectedDates, DateTime minDate, DateTime maxDate)
        {
            var debugMessage = "minDate: " + minDate + "\nmaxDate: " + maxDate;
            if (selectedDates == null) {
                debugMessage += "\nselectedDate: null";
            }
            else {
                debugMessage += "\nselectedDates: ";
                debugMessage = selectedDates.Aggregate(debugMessage, (current, date) => current + (date + "; "));
            }

            return debugMessage;
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

    class MonthCellWithMonthIndex
    {
        public MonthCellDescriptor Cell;
        public int MonthIndex;

        public MonthCellWithMonthIndex(MonthCellDescriptor cell, int monthIndex)
        {
            Cell = cell;
            MonthIndex = monthIndex;
        }
    }
}