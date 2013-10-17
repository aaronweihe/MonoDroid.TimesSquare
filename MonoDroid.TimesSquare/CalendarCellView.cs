using System;
using System.Collections.Generic;
using System.Text;

using Android.Content;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Widget;
using Java.Awt.Font;

namespace MonoDroid.TimesSquare
{
    public class CalendarCellView : TextView
    {
        private static readonly int[] StateSelectable = {Resource.Attribute.state_selectable};
        private static readonly int[] StateCurrentMonth = {Resource.Attribute.state_current_month};
        private static readonly int[] StateToday = {Resource.Attribute.state_today};
        private static readonly int[] StateRangeFirst = {Resource.Attribute.state_range_first};
        private static readonly int[] StateRangeMiddle = {Resource.Attribute.state_range_middle};
        private static readonly int[] StateRangeLast = {Resource.Attribute.state_range_last};

        private bool _isSelectable;
        private bool _isCurrentMonth;
        private bool _isToday;
        private RangeState _rangeState = RangeState.None;

        public CalendarCellView(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        public CalendarCellView(Context context) : base(context)
        {
        }

        public CalendarCellView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public CalendarCellView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
        }

        public bool Selectable
        {
            set
            {
                _isSelectable = value;
                RefreshDrawableState();
            }
        }

        public bool IsCurrentMonth
        {
            set
            {
                _isCurrentMonth = value;
                RefreshDrawableState();
            }
        }

        public bool IsToday
        {
            set
            {
                _isToday = value;
                RefreshDrawableState();
            }
        }

        public RangeState RangeState
        {
            set
            {
                _rangeState = value;
                RefreshDrawableState();
            }
        }

        protected override int[] OnCreateDrawableState(int extraSpace)
        {
            int[] drawableState = base.OnCreateDrawableState(extraSpace + 4);

            if (_isSelectable) {
                MergeDrawableStates(drawableState, StateSelectable);
            }

            if (_isCurrentMonth) {
                MergeDrawableStates(drawableState, StateCurrentMonth);
            }

            if (_isToday) {
                MergeDrawableStates(drawableState, StateToday);
            }

            switch (_rangeState) {
                case RangeState.First:
                    MergeDrawableStates(drawableState, StateRangeFirst);
                    break;
                case RangeState.Middle:
                    MergeDrawableStates(drawableState, StateRangeMiddle);
                    break;
                case RangeState.Last:
                    MergeDrawableStates(drawableState, StateRangeLast);
                    break;
            }
            return drawableState;
        }
    }
}
