using System;
using Android.Content;
using Android.Widget;

namespace MonoDroid.TimesSquare
{
    public class CellClickedListener: IListener
    {
        private readonly CalendarPickerView _calendar;
        private readonly Context _context;
        public CellClickedListener (Context context, CalendarPickerView calendar)
        {
            _context = context;
            _calendar = calendar;
        }

        public void HandleClick(MonthCellDescriptor cell)
        {
			if (!CalendarPickerView.IsBetweenDates(cell.DateTime, _calendar.MinCal, _calendar.MaxCal)) {
                string errorMessage = _context.Resources.GetString(Resource.String.invalid_date,
                                                                   _calendar.MinCal.ToString(),
				                                                   _calendar.MaxCal.ToString());
                Toast.MakeText(_context, errorMessage, ToastLength.Short).Show();
            }
            else {
                //De-select the currently-selected cell.
                _calendar.SelectedCell.IsSelected = false;
                //Select the new cell.
                _calendar.SelectedCell = cell;
                _calendar.SelectedCell.IsSelected = true;
                //Track the currently selected date value.
                _calendar.SelectedCell.DateTime = cell.DateTime;
                //Update the adapter.
                _calendar.MyAdapter.NotifyDataSetChanged();
            }
        }
    }
}
