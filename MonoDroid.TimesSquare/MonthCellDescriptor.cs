using System;

namespace MonoDroid.TimesSquare
{
    public class MonthCellDescriptor:Java.Lang.Object
    {
        public DateTime DateTime { get; set; }
        public  int Value { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsSelected { get; set; }
        public  bool IsToday { get; set; }
        public  bool IsSelectable { get; set; }

        public MonthCellDescriptor(DateTime date, bool isCurrentMonth, bool isSelectable, bool isSelected, bool isToday, int value)
        {
            DateTime = date;
            Value = value;
            IsCurrentMonth = isCurrentMonth;
			IsSelected = isSelected;
			IsToday = isToday;
			IsSelectable = isSelectable;
        }

        public override string ToString()
        {
            return "MonthCellDescriptor{"
                   + "date=" + DateTime
                   + ", value=" + Value
                   + ", isCurrentMonth=" + IsCurrentMonth
                   + ", isSelected=" + IsSelected
                   + ", isToday=" + IsToday
                   + ", isSelectable=" + IsSelectable
                   + "}";
        }
    }
}