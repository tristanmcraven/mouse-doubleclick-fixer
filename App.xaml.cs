using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using NAudio.Wave;

namespace MouseDoubleClickFixer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IntPtr HookID = IntPtr.Zero;
        public static LowLevelMouseProc Proc = HookCallback;
        private static DateTime _lastLMBClick = DateTime.MinValue;
        private static DateTime _lastMouse5Click = DateTime.MinValue;
        private const int DoubleClickThresholdMs = 60;

        public static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
            
        }

        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT mouseStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

                // LMB
                if (wParam == (IntPtr)WM_LBUTTONDOWN)
                {
                    DateTime now = DateTime.Now;
                    var timeSpan = now - _lastLMBClick;
                    GlobalViewModel.TimeSpans.Add(timeSpan);
                    if (timeSpan.TotalMilliseconds <= DoubleClickThresholdMs)
                    {
                        PlayBeepSound();
                        return (IntPtr)1; // block event
                    }
                    _lastLMBClick = now;
                }

                // mouse5
                if (wParam == (IntPtr)WM_XBUTTONDOWN && (int)(mouseStruct.mouseData >> 16) == XBUTTON2)
                {
                    DateTime now = DateTime.Now;
                    if ((now - _lastMouse5Click).TotalMilliseconds <= DoubleClickThresholdMs)
                    {
                        PlayBeepSound();
                        Debug.WriteLine("mouse5");
                        return (IntPtr)1; // block event

                    }
                    _lastMouse5Click = now;
                    Debug.WriteLine("mouse5");
                }
            }

            return CallNextHookEx(HookID, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_XBUTTONDOWN = 0x020B;
        private const int XBUTTON2 = 0x0001; // mouse5 (go back)

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //HookID = SetHook(Proc);
            var tb = (TaskbarIcon)FindResource("TaskbarIcon");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            UnhookWindowsHookEx(HookID);
            base.OnExit(e);
        }

        private static void PlayBeepSound()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "beep.wav");

            var audioFile = new AudioFileReader(path) { Volume = 0.26f };
            var outputDevice = new WaveOutEvent();

            outputDevice.Init(audioFile);
            outputDevice.Play();

            // Clean up after playback finished (async style)
            outputDevice.PlaybackStopped += (s, e) =>
            {
                outputDevice.Dispose();
                audioFile.Dispose();
            };
        }

        private void exit_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void settings_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void open_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenMainWindow();
        }

        private void OpenMainWindow()
        {
            var mw = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mw == null)
            {
                new MainWindow().Show();
            }
        }
    }
}
