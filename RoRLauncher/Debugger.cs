using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using System.Management;

namespace RoRLauncher
{
    class Debugger
    {
        public DebugDumper dumper;

        // Pulls together pieces of information that can be useful in tracking down user issues that don't create an error popup
        private string DumpConfig(MainWindow mainWindow)
        {
            PresentationSource source = PresentationSource.FromVisual(mainWindow);
            System.Drawing.Point CurrentLocation = new System.Drawing.Point((int)mainWindow.Left, (int)mainWindow.Top);

            // Pull in VisualBasic stuff to make up for C# WPF not having many features accessible for this
            Microsoft.VisualBasic.Devices.ComputerInfo ComputerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
            Microsoft.VisualBasic.Devices.Computer Computer = new Microsoft.VisualBasic.Devices.Computer();
            Microsoft.VisualBasic.Devices.Network Network = new Microsoft.VisualBasic.Devices.Network();
            Microsoft.VisualBasic.Devices.ServerComputer ServerComputer = new Microsoft.VisualBasic.Devices.ServerComputer();
            Microsoft.VisualBasic.ApplicationServices.ApplicationBase ApplicationBase = new Microsoft.VisualBasic.ApplicationServices.ApplicationBase();

            StringBuilder Detail = new StringBuilder();
            string RegistryPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion";
            Detail.AppendLine(string.Format("OS: {0}", Microsoft.Win32.Registry.GetValue(RegistryPath, "ProductName", null)));
            Detail.AppendLine(string.Format("BLD: {0}", Microsoft.Win32.Registry.GetValue(RegistryPath, "CurrentBuild", null)));
            Detail.AppendLine(string.Format("LAB: {0}", Microsoft.Win32.Registry.GetValue(RegistryPath, "BuildLab", null)));
            // Check if the user is running this under Wine
            Detail.AppendLine(string.Format("WINE: {0}", (Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wine") != null ? true : false)));
            Detail.AppendLine(string.Format("ADM: {0}", new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator).ToString()));
            Detail.AppendLine(string.Format("DPI: X: {0}, Y: {1}", source.CompositionTarget.TransformToDevice.M11 * 96, source.CompositionTarget.TransformToDevice.M22 * 96));
            Detail.AppendLine(string.Format("PMN: {0}", Computer.Screen.Primary));
            Detail.AppendLine(string.Format("RES: X: {0}, Y: {1}", Computer.Screen.Bounds.Width, Computer.Screen.Bounds.Height));
            Detail.AppendLine(string.Format("SWA: X: {0}, Y: {1}, W: {2}, H: {3}", Computer.Screen.WorkingArea.X, Computer.Screen.WorkingArea.Y, Computer.Screen.WorkingArea.Width, Computer.Screen.WorkingArea.Height));
            Detail.AppendLine(string.Format("BPP: {0}", Computer.Screen.BitsPerPixel));
            Detail.AppendLine(string.Format("SDN: {0}", Computer.Screen.DeviceName));
            Detail.AppendLine(string.Format("NWK: {0}", System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()));
            Detail.AppendLine(string.Format("CCN: {0}", System.Globalization.CultureInfo.InstalledUICulture.EnglishName));
            Detail.AppendLine(string.Format("SCN: {0}", System.Globalization.CultureInfo.InstalledUICulture.Name));
            // Byte -> Megabyte = / 1048576
            // Byte -> Gigabyte = / 1073741824
            Detail.AppendLine(string.Format("APM: {0}/{1}", (ComputerInfo.AvailablePhysicalMemory / 1048576), (ComputerInfo.TotalPhysicalMemory / 1048576)));
            // Something is not right with this, the values are WAY too high
            Detail.AppendLine(string.Format("AVM: {0}/{1}", (ComputerInfo.AvailableVirtualMemory / 1048576), (ComputerInfo.TotalVirtualMemory / 1048576)));
            Detail.AppendLine(string.Format("VER: {0}", ApplicationBase.Info.Version.ToString()));
            Detail.AppendLine(string.Format("DIR: {0}", ApplicationBase.Info.DirectoryPath.ToString()));
            Detail.AppendLine(string.Format("APP: {0}", System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName)));
            Detail.AppendLine(string.Format("SZE: Width: {0}, Height: {1}", mainWindow.Width, mainWindow.Height));
            Detail.AppendLine(string.Format("CLT: Width: {0}, Height: {1}", mainWindow.RenderSize.Width, mainWindow.RenderSize.Height));

            // Wrap motherboard information in a try because something can always not work in this, better to skip it all than try for each
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");

                foreach (ManagementObject wmi in searcher.Get())
                {
                    Detail.AppendLine(string.Format("MBM: {0}", wmi.GetPropertyValue("Manufacturer").ToString()));
                    Detail.AppendLine(string.Format("MBID: {0}", wmi.GetPropertyValue("Product").ToString()));
                }
            }
            catch { }

            return Detail.ToString();
        }

        /// <summary>
        /// Returns a string that contains the contents of the computer and application configurations.
        /// No file is created when using this option.
        /// </summary>
        public string DumpConfigs(MainWindow mainWindow, bool returnAsString)
        {
            return DumpConfig(mainWindow);
        }

        /// <summary>
        /// Writes out a text file named RoR_Configs.txt to the current directory and contains the contents of the computer and application configurations.
        /// </summary>
        public void DumpConfigs(MainWindow mainWindow)
        {
            if (dumper != null)
            {
                dumper.ConfigCheckbox.IsChecked = null;
                // Enable mousing over the description text of the new window to display the file output paths
                dumper.Description.ToolTip = Directory.GetCurrentDirectory() + "\\RoR_Configs.txt";
            }
            StreamWriter file = new StreamWriter(Directory.GetCurrentDirectory() + "\\RoR_Configs.txt");
            file.WriteLine(DumpConfig(mainWindow));
            file.Close();
            if (dumper != null)
            {
                dumper.ConfigCheckbox.IsChecked = true;
            }
        }

        /// <summary>
        /// Writes out a text file named RoR_DxDiag.txt to the current directory and contains the contents of an DxDiag dump.
        /// </summary>
        public void DumpDxDiag()
        {
            if (dumper != null)
            {
                dumper.DXDiagCheckbox.IsChecked = null;
                dumper.Description.ToolTip += Environment.NewLine + Directory.GetCurrentDirectory() + "\\RoR_DxDiag.txt";
            }
            Process process = new Process();
            process.StartInfo.FileName = "DxDiag.exe";
            process.StartInfo.Arguments = " /t " + Directory.GetCurrentDirectory() + "\\RoR_DxDiag.txt";
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => DxDiag_Exited(this, args);
            process.Start();
        }

        private void DxDiag_Exited(object sender, EventArgs e)
        {
            Debugger debugger = (Debugger)sender;
            if (debugger.dumper != null)
            {
                debugger.dumper.Dispatcher.Invoke(new Action(delegate
                {
                    debugger.dumper.DXDiagCheckbox.IsChecked = true;
                }), new object[0]);
            }
        }

        /// <summary>
        /// Writes out a text file named RoR_MSInfo.txt to the current directory and contains the contents of an MSInfo32 dump.
        /// </summary>
        public void DumpMSInfo32()
        {
            if (dumper != null)
            {
                dumper.MSInfoCheckBox.IsChecked = null;
                dumper.Description.ToolTip += Environment.NewLine + Directory.GetCurrentDirectory() + "\\RoR_MSInfo.txt";
            }
            Process process = new Process();
            process.StartInfo.FileName = "MSInfo32.exe";
            process.StartInfo.Arguments = " /report \"" + Directory.GetCurrentDirectory() + "\\RoR_MSInfo.txt\"";
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => MSInfo_Exited(this, args);
            process.Start();
        }

        private static void MSInfo_Exited(object sender, System.EventArgs e)
        {
            Debugger debugger = (Debugger)sender;
            if (debugger.dumper != null)
            {
                debugger.dumper.Dispatcher.Invoke(new Action(delegate
                {
                    debugger.dumper.MSInfoCheckBox.IsChecked = true;
                }), new object[0]);
            }
        }
    }
}
