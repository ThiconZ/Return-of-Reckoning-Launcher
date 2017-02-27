using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RoRLauncher
{
    public partial class DebugDumper : Window
    {
        // Use PInvoke to remove Minimize/Restore/Close system buttons but keep Windows Style set
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public bool PreventEarlyExit = false;

        public DebugDumper()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            PreventEarlyExit = true;
            // Reset progress bar to default value
            DumpProgressBar.Value = 0;
            // Force check checkbox values and progress - most likely will not all be set to false due to operations occuring before the form can be fully displayed
            WriteStatusCheckboxs_Checked(null, null);
        }

        private void WriteStatusCheckboxs_Checked(object sender, RoutedEventArgs e)
        {
            int Percentage = 0;

            if (ConfigCheckbox.IsChecked == null)
                Percentage += 10;
            else if (ConfigCheckbox.IsChecked == true)
                Percentage += 33;
            else
                Percentage += 0;

            if (DXDiagCheckbox.IsChecked == null)
                Percentage += 10;
            else if (DXDiagCheckbox.IsChecked == true)
                Percentage += 33;
            else
                Percentage += 0;

            if (MSInfoCheckBox.IsChecked == null)
                Percentage += 10;
            else if (MSInfoCheckBox.IsChecked == true)
                Percentage += 34;
            else
                Percentage += 0;

            DumpProgressBar.Value = Percentage;

            if (ConfigCheckbox.IsChecked == true && DXDiagCheckbox.IsChecked == true && MSInfoCheckBox.IsChecked == true)
            {
                // Create a timer and use it to delay the form close to help make sure the user knows the dumping fully completed
                System.Windows.Threading.DispatcherTimer ExitDelay = new System.Windows.Threading.DispatcherTimer();
                ExitDelay.Interval = new TimeSpan(0,0,1);
                ExitDelay.Tick += new EventHandler(delegate(object s, EventArgs ev) 
                {
                    PreventEarlyExit = false;
                    base.Close();
                    ExitDelay.IsEnabled = false;
                    ExitDelay.Stop();
                });
                ExitDelay.IsEnabled = true;
                ExitDelay.Start();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (PreventEarlyExit == true)
            {
                e.Cancel = true;
            }
        }
    }
}
