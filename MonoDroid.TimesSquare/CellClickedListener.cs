using System;

namespace MonoDroid.TimesSquare
{
    public class CellClickedListener : IListener
    {
        private readonly CalendarPickerView _calendar;

        public CellClickedListener(CalendarPickerView calendar)
        {
            _calendar = calendar;
        }

        public void HandleClick(MonthCellDescriptor cell)
        {
            var clickedDate = cell.DateTime;

            if (!CalendarPickerView.IsBetweenDates(clickedDate, _calendar.MinCal, _calendar.MaxCal)
                || !_calendar.IsSelectable(clickedDate)) {
                if (_calendar.InvalidDateSelectedListener != null) {
                    _calendar.InvalidDateSelectedListener.OnInvalidDateSelected(clickedDate);
                }
            }
            else {
                bool wasSelected = _calendar.DoSelectDate(clickedDate, cell);
                if (wasSelected && _calendar.DateListener != null) {
                    _calendar.DateListener.OnDateSelected(clickedDate);
                }
            }
        }
    }
}
