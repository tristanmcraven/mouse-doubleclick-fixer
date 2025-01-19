using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseDoubleClickFixer
{
    public static class GlobalViewModel
    {
        public static ObservableCollection<TimeSpan> TimeSpans = new ObservableCollection<TimeSpan>();
    }
}
