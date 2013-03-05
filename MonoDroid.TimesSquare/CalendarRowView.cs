using System;
using Android.Content;
using Android.Util;
using Android.Views;

namespace MonoDroid.TimesSquare
{
    public class CalendarRowView : ViewGroup, View.IOnClickListener
    {
        public bool IsHeaderRow { get; set; }
        private IListener _listener;
        private int _cellSize;
        private int _oldWidthMeasureSpec;
        private int _oldHeightMeasureSpec;

        public CalendarRowView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public override void AddView(View child, int index, LayoutParams @params)
        {
            child.SetOnClickListener(this);
            base.AddView(child, index, @params);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if (_oldWidthMeasureSpec == widthMeasureSpec && _oldHeightMeasureSpec == heightMeasureSpec) {
                Logr.D("Skip Row.OnMeasure");
                SetMeasuredDimension(MeasuredWidth, MeasuredHeight);
                return;
            }

            long start = DateTime.Now.Millisecond;
            int totalWidth = MeasureSpec.GetSize(widthMeasureSpec);
            _cellSize = totalWidth / 7;
            int cellWidthSpec = MeasureSpec.MakeMeasureSpec(_cellSize, MeasureSpecMode.Exactly);
            int cellHeightSpec = IsHeaderRow ? MeasureSpec.MakeMeasureSpec(_cellSize, MeasureSpecMode.AtMost) : cellWidthSpec;
            int rowHeight = 0;
            for (int c = 0; c < ChildCount; c++) {
                View child = GetChildAt(c);
                child.Measure(cellWidthSpec, cellHeightSpec);
                //The row height is the height of the tallest cell.
                if (child.MeasuredHeight > rowHeight) {
                    rowHeight = child.MeasuredHeight;
                }
            }
            int widthWithPadding = totalWidth + PaddingLeft + PaddingRight;
            int heightWithPadding = rowHeight + PaddingTop + PaddingBottom;
            SetMeasuredDimension(widthWithPadding, heightWithPadding);
            Logr.D("Row.OnMeasure {0} ms", DateTime.Now.Millisecond - start);
            _oldHeightMeasureSpec = widthMeasureSpec;
            _oldWidthMeasureSpec = heightMeasureSpec;
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            long start = DateTime.Now.Millisecond;
            int cellHeight = b - t;
            for (int c = 0; c < ChildCount; c++) {
                View child = GetChildAt(c);
                child.Layout(c * _cellSize, 0, (c + 1) * _cellSize, cellHeight);
            }
            Logr.D("Row.OnLayout {0} ms", DateTime.Now.Millisecond - start);
        }

        public void OnClick(View v)
        {
            if(_listener!=null){
				_listener.HandleClick((MonthCellDescriptor) v.Tag); 
			}
        }

        public void SetListener(IListener listener)
        {
			_listener = listener;
        }
    }
}