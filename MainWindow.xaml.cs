using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MouseDoubleClickFixer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<TimeSpan> TimeSpans = new ObservableCollection<TimeSpan>();
        private DateTime LastLmbClick = DateTime.Now;
        private int Threshold = 80;
        public MainWindow()
        {
            InitializeComponent();
            TimeSpans.CollectionChanged += TimeSpans_CollectionChanged;
        }

        private void TimeSpans_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var ls = timeSpans_ListBox;
            ls.Items.Clear();
            foreach (var t in TimeSpans)
            {
                var tb = new TextBlock { Text = FormatTimeSpan(t) };
                if (t.TotalMilliseconds < Threshold)
                    tb.Foreground = Brushes.Red;
                ls.Items.Add(tb);
            }
            if (ls.Items.Count > 0)
                ls.ScrollIntoView(ls.Items.GetItemAt(ls.Items.Count - 1));
        }

        private void clickArea_Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var now = DateTime.Now;
            var timeSpan = now - LastLmbClick;
            LastLmbClick = now;
            TimeSpans.Add(timeSpan);
            if (timeSpan.TotalMilliseconds < Threshold)
            {
                clickArea_Border.Background = Brushes.OrangeRed;
                clickArea_TextBlock.Text = "Mouse DoubleClick Detected";
            }
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            int seconds = (int)timeSpan.TotalSeconds;
            long fractionalTicks = timeSpan.Milliseconds;

            return $"{seconds}.{fractionalTicks:D3}";
        }

        private void reset_Button_Click(object sender, RoutedEventArgs e)
        {
            ResetCounter();
        }

        private void ResetCounter()
        {
            clickArea_TextBlock.Text = "Click Me!";
            clickArea_Border.Background = Brushes.GreenYellow;
            TimeSpans.Clear();
        }

        private void script_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (App.HookID == IntPtr.Zero)
            {
                App.HookID = App.SetHook(App.Proc);
            }
        }

        private void script_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (App.HookID != IntPtr.Zero)
            {
                App.UnhookWindowsHookEx(App.HookID);
                App.HookID = IntPtr.Zero;
                Debug.WriteLine("Hook disabled.");
            }
        }
    }
}