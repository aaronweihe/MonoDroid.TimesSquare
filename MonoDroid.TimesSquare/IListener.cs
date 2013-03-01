using System;

namespace MonoDroid.TimesSquare
{
    public interface IListener
    {
        void HandleClick(MonthCellDescriptor cell);
    }

}
