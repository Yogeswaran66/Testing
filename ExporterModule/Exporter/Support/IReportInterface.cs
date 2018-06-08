using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Support
{
    public interface IFLoadTestReporter
    {
        void SetCurrentItem(String Appdata1);
        void SetCount();
        void PreparetoProcessItem();
        void CompletedItem();
        Boolean CanContinueProcessing();
    }
}
