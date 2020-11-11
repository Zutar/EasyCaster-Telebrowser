using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace EasyCaster_Telebrowser
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        const int GWL_STYLE = (-16);
        const uint WS_SIZEBOX = 0x00040000;
        const int WS_CAPTION = 0x00C00000;

        bool[] enable = { false, false, false };
        Process[] proc = { new Process(), new Process(), new Process() };

        DispatcherTimer check_timer = null;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void SetLanguageDictionary()
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (Properties.Settings.Default.lang)
            {
                case "UKR":
                    dict.Source = new Uri("..\\UKR.xaml", UriKind.Relative);
                    break;
                case "ENG":
                    dict.Source = new Uri("..\\ENG.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("..\\RUS.xaml", UriKind.Relative);
                    break;
            }
            this.Resources.MergedDictionaries.Add(dict);
        }

        private void form_Loaded(object sender, RoutedEventArgs e)
        {
            check_timer = new DispatcherTimer();
            check_timer.Tick += new EventHandler(CheckTimer_Tick);
            check_timer.Interval = new TimeSpan(0, 0, 1);

            SetLanguageDictionary();

            /* Launch encoder */
            StartProcess(0);
            
            /* Launch alphaPro */
            DispatcherTimer alphaProTimer = new DispatcherTimer();
            alphaProTimer.Tick += new EventHandler(StartAlphaPro_Tick);
            alphaProTimer.Interval = new TimeSpan(0, 0, 5);
            alphaProTimer.IsEnabled = true;

            void StartAlphaPro_Tick(object s1, EventArgs e1)
            {
                alphaProTimer.Stop();
                StartProcess(1);
                intoMainWindow(0, true);
            }
            /* Launch TELE */
            DispatcherTimer teleTimer = new DispatcherTimer();
            teleTimer.Tick += new EventHandler(StartTele_Tick);
            teleTimer.Interval = new TimeSpan(0, 0, 10);
            teleTimer.IsEnabled = true;

            void StartTele_Tick(object s1, EventArgs e1)
            {
                teleTimer.Stop();
                //StartProcess(2);
                intoMainWindow(1, true);
                //intoMainWindow(2);
                check_timer.IsEnabled = true;
            }
        }
        private void CheckTimer_Tick(object s1, EventArgs e1)
        {
            for(int i = 0; i < proc.Length; i++)
            {
                if (!ProcessExists(i))
                {
                    if (i != 2)
                    {
                        //KillProcess(i);
                        //StartProcess(i);
                    }
                }
            }
        }
        private bool ProcessExists(int id)
        {
            return Process.GetProcesses().Any(x => x.Id == id);
        }
        private void KillProcess(int id)
        {
            BrushConverter bc = new BrushConverter();
            if (id == 0)
            {
                m_encoder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFDDDDDD");
            }else if (id == 1)
            {
                m_alphapro.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFDDDDDD");
            }
            else
            {
                m_tele.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFDDDDDD");
            }
            try
            {
                KillProcessAndChildrens(proc[id].Id);
            }
            catch (Exception)
            {

            }
            proc[id] = null;
        }
        private void StartProcess(int id)
        {
            String name = "";
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            proc[id] = new Process();
            if (id == 0)
            {
                startInfo.Arguments = "/C " + "\"C:\\Program Files (x86)\\Easycaster\\Easycaster TV Encoder\\Easycaster TV Encoder.exe\"";
                name = "Easycaster TV Encoder";

                BrushConverter bc = new BrushConverter();
                m_encoder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#C2FF80");
            }
            else if(id == 1)
            {
                startInfo.Arguments = "/C " + "\"C:\\Program Files (x86)\\EasyCaster\\Easycaster MpegTS Player Install\\Easycaster MpegTS Player.exe\"";
                name = "Easycaster MpegTS Player";

                BrushConverter bc = new BrushConverter();
                m_alphapro.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#C2FF80");
            }
            else
            {
                startInfo.Arguments = "/C " + "\"C:\\Program Files (x86)\\Easycaster\\Easycaster Restreamer DEMO\\Easycaster Restreamer DEMO.exe\"";
                name = "Easycaster Restreamer DEMO";

                BrushConverter bc = new BrushConverter();
                m_tele.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#C2FF80");
            }

            proc[id].StartInfo = startInfo;
            proc[id].Start();
            enable[id] = true;

            DispatcherTimer win_timer = new DispatcherTimer();
            win_timer.Tick += new EventHandler(StartTimer1_Tick);
            win_timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            win_timer.IsEnabled = true;


            void StartTimer1_Tick(object sender, EventArgs e)
            {
                ManagementObjectSearcher processSearcher = new ManagementObjectSearcher
                ("Select * From Win32_Process Where ParentProcessID=" + proc[id].Id);
                ManagementObjectCollection processCollection = processSearcher.Get();

                if (processCollection != null)
                {
                    Process p = new Process();
                    foreach (ManagementObject mo in processCollection)
                    {
                        try
                        {
                            p = Process.GetProcessById(Convert.ToInt32(mo["ProcessID"]));
                            intoMainWindow(id, false);
                            if (p.MainWindowHandle.ToString() != "0" && p.ProcessName == name)
                            {
                                proc[id] = p;
                                win_timer.Stop();
                            }
                        }
                        catch (Exception) { }
                    }

                }
            }
        }
        private void RestartProcess(int id)
        {
            KillProcess(id);
            StartProcess(id);
        }
        private void intoMainWindow(int id, bool first)
        {
            int x = 0;
            int height = 0;
            int width = 0;
            if (id == 0)
            {
                x = Convert.ToInt32(this.Width) / 2 - 375;
                height = Convert.ToInt32(this.Height);
            }
            else if(id == 1)
            {
                width = Convert.ToInt32(this.Width);
                height = Convert.ToInt32(this.Height);
            }
            else
            {
                width = Convert.ToInt32(this.Width);
                height = Convert.ToInt32(this.Height);
            }

            if(first && id != 0)
            {
                x = -5000;
            }else if(first && id == 0)
            {
                x = Convert.ToInt32(this.Width) / 2 - 375;
            }

            int style = (int)GetWindowLong(proc[id].MainWindowHandle, GWL_STYLE);
            SetWindowLong(proc[id].MainWindowHandle, GWL_STYLE, (uint)(style & ~(WS_CAPTION | WS_SIZEBOX)));
            SetParent(proc[id].MainWindowHandle, new WindowInteropHelper(Application.Current.MainWindow).Handle);
            MoveWindow(proc[id].MainWindowHandle, x, 25, width, height, true);
        }

        private void m_l_rus_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.lang = "RUS";
            SetLanguageDictionary();
        }

        private void m_l_ukr_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.lang = "UKR";
            SetLanguageDictionary();
        }

        private void m_l_eng_Click_1(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.lang = "ENG";
            SetLanguageDictionary();
        }

        private void m_encoder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MoveWindow(proc[1].MainWindowHandle, -5000, 20, 320, 180, true);
            }
            catch (Exception) { }

            try
            {
                MoveWindow(proc[2].MainWindowHandle, -5000, 20, 320, 180, true);
            }
            catch (Exception) { }

            if (proc[0] != null && enable[0] == true)
            {
                intoMainWindow(0, false);
            }
            else
            {
                //KillProcess(encoder.Id, "encoder");
            }
        }

        private void m_alphapro_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MoveWindow(proc[0].MainWindowHandle, -5000, 20, 320, 180, true);
            }
            catch (Exception) { }

            try
            {
                MoveWindow(proc[2].MainWindowHandle, -5000, 20, 320, 180, true);
            }
            catch (Exception) { }

            if (proc[1] != null && enable[1] == true)
            {
                intoMainWindow(1, false);
            }
            else
            {
                //KillProcess(alphaPro.Id, "alphaPro");
            }
        }
        private void KillProcessAndChildrens(int pid)
        {
            ManagementObjectSearcher processSearcher = new ManagementObjectSearcher
              ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection processCollection = processSearcher.Get();
            try
            {
                Process proc = Process.GetProcessById(pid);
                if (!proc.HasExited) proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }

            if (processCollection != null)
            {
                foreach (ManagementObject mo in processCollection)
                {
                    KillProcessAndChildrens(Convert.ToInt32(mo["ProcessID"])); //kill child processes(also kills childrens of childrens etc.)
                }
            }
        }
        private void form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                KillProcessAndChildrens(proc[0].Id);
                KillProcessAndChildrens(proc[1].Id);
                KillProcessAndChildrens(proc[2].Id);
            }
            catch (Exception)
            {

            }
        }

        private void m_tele_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MoveWindow(proc[0].MainWindowHandle, -5000, 20, 320, 180, true);
            }
            catch (Exception) { }

            try
            {
                MoveWindow(proc[1].MainWindowHandle, -5000, 20, 320, 180, true);
            }
            catch (Exception) { }

            if (proc[2] != null && enable[2] == true)
            {
                intoMainWindow(2, false);
            }
            else
            {
                //KillProcess(2);
            }
        }
    }
}
