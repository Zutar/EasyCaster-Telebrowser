using System;
using System.Diagnostics;
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

        Process encoder = new Process();
        Process alphaPro = new Process();
        Process tele = new Process();

        const int GWL_STYLE = (-16);
        const uint WS_SIZEBOX = 0x00040000;
        const int WS_CAPTION = 0x00C00000;

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

            /* Launch encoder */
            StartProcess("encoder");
            
            /* Launch alphaPro */
            DispatcherTimer alphaProTimer = new DispatcherTimer();
            alphaProTimer.Tick += new EventHandler(StartAlphaPro_Tick);
            alphaProTimer.Interval = new TimeSpan(0, 0, 5);
            alphaProTimer.IsEnabled = true;

            void StartAlphaPro_Tick(object s1, EventArgs e1)
            {
                alphaProTimer.Stop();
                intoMainWindow(encoder, "encoder");
            }
            /* Launch TELE */
            DispatcherTimer teleTimer = new DispatcherTimer();
            teleTimer.Tick += new EventHandler(StartTele_Tick);
            teleTimer.Interval = new TimeSpan(0, 0, 10);
            teleTimer.IsEnabled = true;

            void StartTele_Tick(object s1, EventArgs e1)
            {
                teleTimer.Stop();
            }
        }
        private void KillProcess(int id, String type)
        {
            BrushConverter bc = new BrushConverter();
            if (type == "encoder")
            {
                encoder = null;
                m_encoder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFDDDDDD");
            }
            try
            {
                KillProcessAndChildrens(id);
            }
            catch (Exception)
            {

            }
        }
        private void StartProcess(string type)
        {
            String name = "";
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process proc = null;

            if (type == "encoder")
            {
                encoder = new Process();
                startInfo.Arguments = "/C " + "\"C:\\Program Files (x86)\\Easycaster\\Easycaster TV Encoder\\Easycaster TV Encoder.exe\"";
                encoder.StartInfo = startInfo;
                encoder.Start();
                name = "Easycaster TV Encoder";
                proc = encoder;

                BrushConverter bc = new BrushConverter();
                m_encoder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#C2FF80");
            }
            else if(type == "alphaPro")
            {
                alphaPro = new Process();
                startInfo.Arguments = "/C " + "\"C:\\Program Files (x86)\\Easycaster\\Easycaster TV Encoder\\Easycaster TV Encoder.exe\"";
                alphaPro.StartInfo = startInfo;
                alphaPro.Start();
                name = "Alpha Pro";
                proc = alphaPro;

                BrushConverter bc = new BrushConverter();
                m_alphapro.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#C2FF80");
            }
            else
            {
                tele = new Process();
                startInfo.Arguments = "/C " + "\"C:\\Program Files (x86)\\Easycaster\\Easycaster TV Encoder\\Easycaster TV Encoder.exe\"";
                tele.StartInfo = startInfo;
                tele.Start();
                name = "TELE";
                proc = tele;

                BrushConverter bc = new BrushConverter();
                m_tele.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#C2FF80");
            }


            DispatcherTimer win_timer = new DispatcherTimer();
            win_timer.Tick += new EventHandler(StartTimer1_Tick);
            win_timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            win_timer.IsEnabled = true;


            void StartTimer1_Tick(object sender, EventArgs e)
            {
                ManagementObjectSearcher processSearcher = new ManagementObjectSearcher
                ("Select * From Win32_Process Where ParentProcessID=" + proc.Id);
                ManagementObjectCollection processCollection = processSearcher.Get();

                if (processCollection != null)
                {
                    Process p = new Process();
                    foreach (ManagementObject mo in processCollection)
                    {
                        try
                        {
                            p = Process.GetProcessById(Convert.ToInt32(mo["ProcessID"]));
                            intoMainWindow(p, type);
                            if (p.MainWindowHandle.ToString() != "0" && p.ProcessName == name)
                            {
                                encoder = p;

                                if (type == "encoder")
                                {
                                    encoder = p;
                                }
                                else if (type == "alphaPro")
                                {
                                    alphaPro = p;
                                }
                                else
                                {
                                    tele = p;
                                }

                                win_timer.Stop();
                            }
                        }
                        catch (Exception) { }
                    }

                }
            }
        }

        private void intoMainWindow(Process p, String type)
        {
            int x = 0;
            int height = 0;
            int width = 0;
            if (type == "encoder")
            {
                x = Convert.ToInt32(this.Width) / 2 - 375;
                height = Convert.ToInt32(this.Height);
            }
            else if(type == "alphaPro")
            {
                width = Convert.ToInt32(this.Width);
                height = Convert.ToInt32(this.Height);
            }
            else
            {
                width = Convert.ToInt32(this.Width);
                height = Convert.ToInt32(this.Height);
            }

            int style = (int)GetWindowLong(p.MainWindowHandle, GWL_STYLE);
            SetWindowLong(p.MainWindowHandle, GWL_STYLE, (uint)(style & ~(WS_CAPTION | WS_SIZEBOX)));
            SetParent(p.MainWindowHandle, new WindowInteropHelper(Application.Current.MainWindow).Handle);
            MoveWindow(p.MainWindowHandle, x, 25, width, height, true);
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
                MoveWindow(alphaPro.MainWindowHandle, -5000, 20, 320, 180, true);
            }
            catch (Exception) { }

            try
            {
                MoveWindow(tele.MainWindowHandle, -5000, 20, 320, 180, true);
            }
            catch (Exception) { }

            if (encoder != null)
            {
                intoMainWindow(encoder, "encoder");
            }
            else
            {
                KillProcess(encoder.Id, "encoder");
            }
        }

        private void m_alphapro_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MoveWindow(encoder.MainWindowHandle, -5000, 20, 320, 180, true);
            }
            catch (Exception) { }

            try
            {
                MoveWindow(tele.MainWindowHandle, -5000, 20, 320, 180, true);
            }
            catch (Exception) { }

            if (alphaPro != null)
            {
                intoMainWindow(alphaPro, "alphaPro");
            }
            else
            {
                KillProcess(alphaPro.Id, "alphaPro");
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
                KillProcessAndChildrens(encoder.Id);
                KillProcessAndChildrens(alphaPro.Id);
                KillProcessAndChildrens(tele.Id);
            }
            catch (Exception)
            {

            }
        }

        private void m_tele_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MoveWindow(encoder.MainWindowHandle, -5000, 20, 320, 180, true);
            }
            catch (Exception) { }

            try
            {
                MoveWindow(alphaPro.MainWindowHandle, -5000, 20, 320, 180, true);
            }
            catch (Exception) { }

            if (alphaPro != null)
            {
                intoMainWindow(tele, "tele");
            }
            else
            {
                KillProcess(tele.Id, "tele");
            }
        }
    }
}
