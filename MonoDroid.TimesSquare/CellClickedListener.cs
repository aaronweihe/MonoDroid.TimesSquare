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
			    string errorMessage = string.Format(_context.Resources.GetString(Resource.String.invalid_date),
                                                    _calendar.MinCal.ToShortDateString(),
                                                    _calendar.MaxCal.ToShortDateString());
                Toast.MakeText(_context, errorMessage, ToastLength.Short).Show();
            }
            else {
			    var selectedDate = cell.DateTime;
                if (_calendar.IsMultiSelect) {
                    foreach (var selectedCell in _calendar.SelectedCells) {
                        if (selectedCell.DateTime.CompareTo(selectedDate) == 0) {
                            selectedCell.IsSelected = false;
                            _calendar.SelectedCells.Remove(selectedCell);
                            selectedDate = DateTime.MinValue;
                            break;
                        }
                    }
                    foreach (var cal in _calendar.SelectedCals) {
                        if (cal.CompareTo(selectedDate) == 0) {
                            _calendar.SelectedCals.Remove(cal);
                            break;
                        }
                    }
                }
                else {
                    foreach (var selectedCell in _calendar.SelectedCells) {
                        selectedCell.IsSelected = false;
                    }
                    _calendar.SelectedCells.Clear();
                    _calendar.SelectedCals.Clear();
                }

                if (selectedDate != DateTime.MinValue) {
                    _calendar.SelectedCells.Add(cell);
                    cell.IsSelected = true;
                    _calendar.SelectedCals.Add(selectedDate);
                }

                _calendar.MyAdapter.NotifyDataSetChanged();
                
                if (selectedDate!=null && _calendar.DateListener != null) {
                    _calendar.DateListener.OnDateSelected(selectedDate);
                }
            }
        }
    }
}
