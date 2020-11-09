using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
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

        private void StartProcess(string type)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe"
        };

            if (type == "encoder")
            {
                encoder = new Process();
                startInfo.Arguments = "/C " + "\"C:\\Program Files (x86)\\Easycaster\\Easycaster TV Encoder\\Easycaster TV Encoder.exe\"";
                encoder.StartInfo = startInfo;
                MessageBox.Show(startInfo.Arguments);
                encoder.Start();
            }
            else if(type == "alphaPro")
            {

            }
            else
            {

            }
        }

        private void intoMainWindow(Process p, String type)
        {
            int style = (int)GetWindowLong(p.MainWindowHandle, GWL_STYLE);
            SetWindowLong(p.MainWindowHandle, GWL_STYLE, (uint)(style & ~(WS_CAPTION | WS_SIZEBOX)));
            SetParent(p.MainWindowHandle, new WindowInteropHelper(Application.Current.MainWindow).Handle);
            MoveWindow(p.MainWindowHandle, 0, 0, 320, 180, true);
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

        private void m_l_eng_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.lang = "ENG";
            SetLanguageDictionary();
        }
    }
}
