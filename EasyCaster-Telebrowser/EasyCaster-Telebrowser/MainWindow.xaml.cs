using Microsoft.Win32;
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
        [DllImportAttribute("User32.dll")]
        private static extern IntPtr SetForegroundWindow(int hWnd);

        const int GWL_STYLE = (-16);
        const uint WS_SIZEBOX = 0x00040000;
        const int WS_CAPTION = 0x00C00000;

        const string name = "EasyCaster-T2-Encoder";
        bool[] enable = { false, false, false };
        Process[] proc = { new Process(), new Process(), new Process() };

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
            SetLanguageDictionary();
            autostart.IsChecked = Properties.Settings.Default.autostart;
            BrushConverter bc = new BrushConverter();
            /* Launch encoder */
            Process[] p = Process.GetProcessesByName("Easycaster TV Encoder");
            if (p.Length == 0)
            {
                StartProcess(0, true);
            }
            else
            {
                proc[0] = p[0];
                enable[0] = true;
                m_encoder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#5eb5eb");
                intoMainWindow(0, true);
            }

            /* Launch alphaPro */
            DispatcherTimer alphaProTimer = new DispatcherTimer();
            alphaProTimer.Tick += new EventHandler(StartAlphaPro_Tick);
            alphaProTimer.Interval = new TimeSpan(0, 0, 5);
            alphaProTimer.IsEnabled = true;

            void StartAlphaPro_Tick(object s1, EventArgs e1)
            {
                alphaProTimer.Stop();
                p = Process.GetProcessesByName("alphapro");
                if (p.Length == 0)
                {
                    StartProcess(1, true);
                }
                else
                {
                    proc[1] = p[0];
                    enable[1] = true;
                    m_alphapro.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#c7e2ff");
                    intoMainWindow(1, true);
                }
            }
            /* Launch TELE */
            DispatcherTimer teleTimer = new DispatcherTimer();
            teleTimer.Tick += new EventHandler(StartTele_Tick);
            teleTimer.Interval = new TimeSpan(0, 0, 10);
            teleTimer.IsEnabled = true;

            void StartTele_Tick(object s1, EventArgs e1)
            {
                teleTimer.Stop();
                p = Process.GetProcessesByName("tele2");
                if (p.Length == 0)
                {
                    StartProcess(2, true);
                }
                else
                {
                    proc[2] = p[0];
                    enable[2] = true;
                    m_tele.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#c7e2ff");
                    intoMainWindow(2, true);
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
                mc_encoder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#d10000");
                mc_encoder.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#000000");

                DispatcherTimer color_timer = new DispatcherTimer();
                color_timer.Tick += new EventHandler(Color_timer);
                color_timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                color_timer.IsEnabled = true;


                void Color_timer(object sender, EventArgs e)
                {
                    color_timer.Stop();
                    mc_encoder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFDDDDDD");
                }
            }
            else if (id == 1)
            {
                m_alphapro.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFDDDDDD");
                mc_alphapro.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#d10000");
                mc_alphapro.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#000000");

                DispatcherTimer color_timer = new DispatcherTimer();
                color_timer.Tick += new EventHandler(Color_timer);
                color_timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                color_timer.IsEnabled = true;


                void Color_timer(object sender, EventArgs e)
                {
                    color_timer.Stop();
                    mc_alphapro.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFDDDDDD");
                }
            }
            else
            {
                m_tele.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFDDDDDD");
                mc_tele.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#d10000");
                mc_tele.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#000000");

                DispatcherTimer color_timer = new DispatcherTimer();
                color_timer.Tick += new EventHandler(Color_timer);
                color_timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                color_timer.IsEnabled = true;


                void Color_timer(object sender, EventArgs e)
                {
                    color_timer.Stop();
                    mc_tele.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFDDDDDD");
                }
            }
            try
            {
                KillProcessAndChildrens(proc[id].Id);
            }
            catch (Exception)
            {

            }
            proc[id] = null;
            enable[id] = false;
        }

        private void StartProcess(int id, bool firstStart)
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
                mc_encoder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#ff0000");
                mc_encoder.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#000000");
                m_encoder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#c7e2ff");
                if(firstStart) m_encoder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#5eb5eb");
            }
            else if(id == 1)
            {
                startInfo.Arguments = "/C " + "\"C:\\alphapro64\\alphapro.exe\"";
                name = "alphapro";

                BrushConverter bc = new BrushConverter();
                mc_alphapro.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#ff0000");
                mc_alphapro.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#000000");
                m_alphapro.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#c7e2ff");
            }
            else
            {
                startInfo.Arguments = "/C " + "\"C:\\alphapro64\\Tele2.exe\"";
                name = "Tele2";

                BrushConverter bc = new BrushConverter();
                mc_tele.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#ff0000");
                mc_tele.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#000000");
                m_tele.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#c7e2ff");
            }

            proc[id].StartInfo = startInfo;
            proc[id].Start();
            enable[id] = true;
            
            int timer = 1000;
            if (id == 1)
            {
                timer = 2500;
                Process.GetProcessById(proc[id].Id).PriorityClass = ProcessPriorityClass.RealTime;
                priority.Header = Process.GetProcessById(proc[id].Id).PriorityClass;
                proc[id].Refresh();
            }
            else if (id == 2)
            {
                timer = 10000;
            }
            DispatcherTimer win_timer = new DispatcherTimer();
            win_timer.Tick += new EventHandler(StartTimer_Tick);
            win_timer.Interval = new TimeSpan(0, 0, 0, 0, timer);
            win_timer.IsEnabled = true;


            void StartTimer_Tick(object sender, EventArgs e)
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
                                intoMainWindow(id, firstStart);
                                win_timer.Stop();
                            }
                        }
                        catch (Exception) {
                        }
                    }

                }
            }
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
                int hwd = Convert.ToInt32(proc[0].MainWindowHandle);
                if (hwd != 0) SetForegroundWindow(hwd);
                intoMainWindow(0, false);
            }
            else
            {
                StartProcess(0, false);
            }

            BrushConverter bc = new BrushConverter();
            m_encoder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#5eb5eb");
            m_alphapro.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#c7e2ff");
            m_tele.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#c7e2ff");
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
                int hwd = Convert.ToInt32(proc[1].MainWindowHandle);
                if (hwd != 0) SetForegroundWindow(hwd);
                intoMainWindow(1, false);
            }
            else
            {
                StartProcess(1, false);
            }

            BrushConverter bc = new BrushConverter();
            m_encoder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#c7e2ff");
            m_alphapro.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#5eb5eb");
            m_tele.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#c7e2ff");
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
                string answer = "Вы уверены что хотите закрыть программу?";
                string exit = "Выйти?";
                if (Properties.Settings.Default.lang == "ukr")
                {
                    answer = "Ви впевнені що хочете закрити програму?";
                    exit = "Вийти?";
                }
                else if (Properties.Settings.Default.lang == "eng")
                {
                    answer = "Are you sure you want to close the program?";
                    exit = "Go out?";
                }

                MessageBoxResult msgResult = MessageBox.Show(answer, exit, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (msgResult == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    Application.Current.Shutdown();
                }

                KillProcessAndChildrens(proc[0].Id);
                KillProcessAndChildrens(proc[1].Id);
                KillProcessAndChildrens(proc[2].Id);
                Properties.Settings.Default.autostart = Convert.ToBoolean(autostart.IsChecked);
                Properties.Settings.Default.Save();
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
                int hwd = Convert.ToInt32(proc[2].MainWindowHandle);
                if(hwd != 0) SetForegroundWindow(hwd);
                intoMainWindow(2, false);
            }
            else
            {
                StartProcess(2, false);
            }

            BrushConverter bc = new BrushConverter();
            m_encoder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#c7e2ff");
            m_alphapro.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#c7e2ff");
            m_tele.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#5eb5eb");
        }

        private void autostart_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)autostart.IsChecked)
            {
                SetAutorunValue(true);
            }
            else
            {
                SetAutorunValue(false);
            }
        }
        public void SetAutorunValue(bool autorun)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (autorun)
            {
                registryKey.SetValue(name, System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            else
            {
                registryKey.DeleteValue(name);
            }
        }

        private void mc_encoder_Click(object sender, RoutedEventArgs e)
        {
            KillProcess(0);
        }

        private void mc_alphapro_Click(object sender, RoutedEventArgs e)
        {
            if(enable[2] == true)
            {
                MessageBox.Show(FindResource("mb_dis").ToString());
            }
            else
            {
                KillProcess(1);
            }
        }

        private void mc_tele_Click(object sender, RoutedEventArgs e)
        {
            KillProcess(2);
        }

        private void m_feedback_Click(object sender, RoutedEventArgs e)
        {
            Feedback feedback = new Feedback();
            feedback.ShowDialog();
        }

        private void m_about_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.Show();
        }
    }
}
