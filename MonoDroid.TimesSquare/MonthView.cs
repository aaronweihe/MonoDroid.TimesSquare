using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace MonoDroid.TimesSquare
{
    public class MonthView : LinearLayout
    {
        private TextView _title;
        private CalendarGridView _grid;
        private ClickHandler _clickHandler;

        public MonthView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public static MonthView Create(ViewGroup parent, LayoutInflater inflater, string weekdayNameFormat,
            DateTime today, ClickHandler handler, int dividerColor, int dayBackgroundResId,
            int dayTextColorResId, int titleTextColor, int headerTextColor)
        {
            var view = (MonthView) inflater.Inflate(Resource.Layout.month, parent, false);
            view.setDividerColor(dividerColor);
            view.setDayTextColor(dayTextColorResId);
            view.setTitleTextColor(titleTextColor);
            view.setHeaderTextColor(headerTextColor);

            if (dayBackgroundResId != 0) {
                view.setDayBackground(dayBackgroundResId);
            }

            var originalDay = today;

            var firstDayOfWeek = (int) CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;

            var headerRow = (CalendarRowView) view._grid.GetChildAt(0);

            for (int i = 0; i < 7; i++) {
                var offset = firstDayOfWeek - (int) today.DayOfWeek + i;
                today = today.AddDays(offset);
                var textView = (TextView) headerRow.GetChildAt(i);
                textView.Text = today.ToString(weekdayNameFormat);
                today = originalDay;
            }
            view._clickHandler = handler;
            return view;
        }

        public void Init(MonthDescriptor month, List<List<MonthCellDescriptor>> cells)
        {
            Logr.D("Initializing MonthView ({0:d}) for {1}", GetHashCode(), month);
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            _title.Text = month.Label;

            int numOfRows = cells.Count;
            _grid.NumRows = numOfRows;
            for (int i = 0; i < 6; i++) {
                var weekRow = (CalendarRowView)_grid.GetChildAt(i + 1);
                weekRow.ClickHandler = _clickHandler;
                if (i < numOfRows) {
                    weekRow.Visibility = ViewStates.Visible;
                    var week = cells[i];
                    for (int c = 0; c < week.Count; c++) {
                        var cell = week[c];
                        var cellView = (CalendarCellView)weekRow.GetChildAt(c);
                        cellView.Text = cell.Value.ToString();
                        cellView.Enabled = cell.IsCurrentMonth;

                        cellView.Selectable = cell.IsSelectable;
                        cellView.Selected = cell.IsSelected;
                        cellView.IsCurrentMonth = cell.IsCurrentMonth;
                        cellView.IsToday = cell.IsToday;
                        cellView.IsHighlighted = cell.IsHighlighted;
                        cellView.RangeState = cell.RangeState;
                        cellView.Tag = cell;
                    }
                }
                else {
                    weekRow.Visibility = ViewStates.Gone;
                }
            }
            stopWatch.Stop();
            Logr.D("MonthView.Init took {0} ms", stopWatch.ElapsedMilliseconds);
        }

        public void setDividerColor(int color)
        {
            _grid.SetDividerColor(color);
        }

        public void setDayBackground(int resId)
        {
            _grid.SetDayBackground(resId);
        }

        public void setDayTextColor(int resId)
        {
            _grid.SetDayTextColor(resId);
        }

        public void setTitleTextColor(int color)
        {
            _title.SetTextColor(base.Resources.GetColor(color));
        }

        public void setHeaderTextColor(int color)
        {
            _grid.SetHeaderTextColor(color);
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            _title = FindViewById<TextView>(Resource.Id.title);
            _grid = FindViewById<CalendarGridView>(Resource.Id.calendar_grid);
        }
    }
}