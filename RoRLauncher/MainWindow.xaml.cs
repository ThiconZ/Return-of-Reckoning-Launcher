using RoRLauncher.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;

namespace RoRLauncher
{
    public partial class MainWindow : Window
    {
        public static MainWindow mainWindow;

        private bool savePassword;

        private XDocument doc;

        private int version = 2;

        private string Password = "";

        private int PasswordMode;

        public bool NoErrorMode = false;

        public bool CustomDependencyMode = false;

        public bool CheckDependencyHashes = false;

        public bool SkipSSLValidation = false;

        internal string Error
        {
            get
            {
                return this.ErrorText.Text.ToString();
            }
            set
            {
                base.Dispatcher.Invoke(new Action(delegate
                {
                    this.ErrorText.Text = value;
                    if (this.ErrorText.Text == "")
                    {
                        this.ErrorText.Visibility = Visibility.Hidden;
                        return;
                    }
                    this.ErrorText.Visibility = Visibility.Visible;
                }), new object[0]);
            }
        }

        private void worker_DownloadXML(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (SkipSSLValidation == true)
                {
                    // Ignore SSL Validation (typically is used when the SSL Certificate is expired and waiting to be updated)
                    ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback((Sender, Certificate, Chain, sslPolicyErrors) => true);
                }
                this.doc = XDocument.Load("http://launcher.returnofreckoning.com/launcher.xml");
            }
            catch
            {
                // Website/File could not be reached - either site is down or user has dns/internet issues
                base.Dispatcher.Invoke(new Action(delegate
                {
                    this.LS_Status.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Status_Offline.png") as ImageSource;
                    this.GS_Status.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Status_Offline.png") as ImageSource;
                }), new object[0]);
                Error = "Failed to connect to the server. Please check your Internet connection and relaunch the program. Visit the website for more help if the issue persists.";
            }
            
        }

        private void NewsVisibity(Visibility visibility)
        {
            this.PatchText.Visibility = visibility;
            this.PatchTitle.Visibility = visibility;
            this.PatchTitle2.Visibility = visibility;
            this.rectangle_info.Visibility = visibility;
            this.ScrollDown.Visibility = visibility;
            this.ScrollUp.Visibility = visibility;
        }

        private void worker_DownloadXMLCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (this.doc == null)
            {
                this.NewsVisibity(Visibility.Hidden);
                this.LS_Status.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Status_Offline.png") as ImageSource;
                return;
            }
            XElement xElement = (this.doc.Descendants("Urgent").Count<XElement>() > 0) ? this.doc.Descendants("Urgent").First<XElement>() : null;
            if (xElement != null)
            {
                this.PatchTitle.Text = (string)xElement.Element("Date");
                this.PatchTitle2.Text = (string)xElement.Element("Title");
                this.PatchText.Text = (string)xElement.Element("Text");
                this.PatchTitle.Foreground = new SolidColorBrush(Colors.Red);
                this.PatchTitle2.Foreground = new SolidColorBrush(Colors.Red);
                this.PatchText.Foreground = new SolidColorBrush(Colors.Red);
                this.NewsVisibity(Visibility.Visible);
            }
            else
            {
                XElement xElement2 = this.doc.Descendants((XName)"News").Count<XElement>() > 0 ? this.doc.Descendants((XName)"News").OrderByDescending<XElement, uint>((Func<XElement, uint>)(n => (uint)n.Element((XName)"Id"))).First<XElement>() : (XElement)null;
                if (xElement2 != null)
                {
                    this.PatchTitle.Text = (string)xElement2.Element("Date");
                    this.PatchTitle2.Text = (string)xElement2.Element("Title");
                    this.PatchText.Text = (string)xElement2.Element("Text");
                    this.NewsVisibity(Visibility.Visible);
                }
                else
                {
                    this.NewsVisibity(Visibility.Hidden);
                }
            }
            XElement xElement3 = (this.doc.Descendants("Version").Count<XElement>() > 0) ? this.doc.Descendants("Version").First<XElement>() : null;
            if (xElement3 != null && this.version < (int)xElement3)
            {
                this.Patch_button.Visibility = Visibility.Visible;
                this.Connect_button.Visibility = Visibility.Hidden;
                this.Connect_button_grey.Visibility = Visibility.Hidden;
                this.PATCH.Text = "UPDATE";
            }
            else
            {
                XElement xElement4 = (this.doc.Descendants("PasswordMode").Count<XElement>() > 0) ? this.doc.Descendants("PasswordMode").First<XElement>() : null;
                if (xElement4 != null)
                {
                    this.PasswordMode = (int)xElement4;
                }
                XElement xElement5 = (this.doc.Descendants("LauncherServer").Count<XElement>() > 0) ? this.doc.Descendants("LauncherServer").First<XElement>() : null;
                if (xElement5 != null)
                {
                    Client.IP = (string)xElement5.Element("Ip");
                    Client.Port = (int)xElement5.Element("Port");
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += new DoWorkEventHandler(this.worker_Connect);
                    backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.worker_ConnectCompleted);
                    backgroundWorker.RunWorkerAsync();
                }
                else
                {
                    MessageBox.Show("ERROR IPPORT");
                }
            }
            this.LS_Status.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Status_Online.png") as ImageSource;
        }

        private void worker_Connect(object sender, DoWorkEventArgs e)
        {
            e.Result = Client.Connect();
        }

        private void worker_ConnectCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Extra loading reset, could be removed for memory saving
            this.GS_Status.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Status_Loading.png") as ImageSource;

            if ((bool)e.Result)
            {
                this.GS_Status.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Status_Online.png") as ImageSource;
            }
            else
            {
                this.GS_Status.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Status_Offline.png") as ImageSource;
            }

            // I dislike this second 'button' for 'disabling' the connect button
            // Should replace it with a single one that changes loaded resources
            // all these image 'buttons' should be turned into Buttons at some point for better usability too

            // Why is connect being 'enabled' regardless of the Game Server status result?
            // If Client is giving us a True result that doesn't actually mean there is a completed connection.
            // This leads to the Connect button not working when clicked. - FIX THIS: High priority!
            this.Connect_button.Visibility = Visibility.Visible;
            this.Connect_button_grey.Visibility = Visibility.Hidden;
        }

        byte[] ComputeFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(File.ReadAllBytes(filePath));
            }
        }

        byte[] ComputeDependencyHash(string fileName)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("RoRLauncher.deps." + fileName))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }

        /// <summary>
        /// Compares the Internal and External dependecy file versions.
        /// </summary>
        /// <param name="fileName">Complete file name and extension of the dependency. Must be the same internally and externally.</param>
        /// <returns>Returns False if Internal version is grater than or equal to External version.</returns>
        private bool CompareDependencyFileVersion(string fileName)
        {
            // Load the dependency files in a way that won't lock them from future use

            FileVersionInfo externalFile = FileVersionInfo.GetVersionInfo(Directory.GetCurrentDirectory() + "/" + fileName);
            Version externalVersion = new Version(externalFile.FileVersion);

            Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("RoRLauncher.deps." + fileName);
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Close();
            stream.Dispose();
            Version internalVersion = new Version(Assembly.ReflectionOnlyLoad(bytes).GetName().Version.ToString());

            // Returns false if internal file version is greater than or equal to the external file
            if (internalVersion >= externalVersion)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if dependency file already exists. If it does, determine if it should be replaced by the internal dependency file.
        /// </summary>
        /// <param name="fileName">Complete file name and extension of the dependency. Must be the same internally and externally.</param>
        /// <param name="hashCheck">The check be done based on file hashes and not file versions.</param>
        /// <returns>Returns False if the file should be replaced.</returns>
        private bool CheckForDependency(string fileName, bool hashCheck = false)
        {
            // Check if the files we're going to unpack already exist in the current location
            if (File.Exists(Directory.GetCurrentDirectory() + "/" + fileName) == true)
            {
                if (hashCheck == false)
                {
                    // If CompareDependencyFileVersion is false then we should replace the external file with the internal one
                    return CompareDependencyFileVersion(fileName);
                }
                else
                {
                    // If the files are here, do an md5 check to see if they are the right versions before unpacking new ones
                    byte[] externalHash;
                    byte[] internalHash;
                    externalHash = ComputeFileHash(Directory.GetCurrentDirectory() + "/" + fileName);
                    internalHash = ComputeDependencyHash(fileName);

                    return externalHash.SequenceEqual(internalHash);
                }
            }
            else
            {
                return false;
            }
        }

        private void worker_Unpack(object sender, DoWorkEventArgs e)
        {
            // Do safe unpacking of dependencies
            // If one happens to be locked open in another program and is the right version it will still work this way, previously it would throw an error
            if (CheckForDependency("HashDictionary.dll", CheckDependencyHashes) == false)
            {
                this.Unpack("HashDictionary.dll");
            }
            if (CheckForDependency("ICSharpCode.SharpZipLib.dll", CheckDependencyHashes) == false)
            {
                this.Unpack("ICSharpCode.SharpZipLib.dll");
            }
        }

        public MainWindow()
        {
            this.InitializeComponent();
            MainWindow.mainWindow = this;
            MainWindow.KillProcessByName("WAR", false);
            MainWindow.KillProcessByName("RoRLauncher", true);

            // Default images are already set so these can all be removed to increase startup speed and save memory

            // Set server status default images
            this.LS_Status.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Status_Loading.png") as ImageSource;
            this.GS_Status.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Status_Loading.png") as ImageSource;

            // Set connect button default image
            this.connect_frame.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Frame_Button_Yellow.png") as ImageSource;
            this.connect_frame.Opacity = .3;

            // Set forum button default image
            this.forum_frame.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Frame_Button_Yellow.png") as ImageSource;
            this.forum_frame.Opacity = .3;

            // Set patch button default image
            this.patch_frame.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Frame_Button_Red.png") as ImageSource;
            this.patch_frame.Opacity = .3;

            // Set connect grey button default image
            this.connect_grey_frame.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Frame_Button_Grey.png") as ImageSource;

            this.ErrorText.Visibility = Visibility.Hidden;
            this.Patch_button.Visibility = Visibility.Hidden;
            this.Connect_button.Visibility = Visibility.Hidden;
            this.Updating_Area.Visibility = Visibility.Hidden;
            this.NewsVisibity(Visibility.Hidden);

            // Load saved user information and set remembering checkbox default image
            if (Settings.Default.Username != "")
            {
                this.savePassword = true;
                this.UsernameTextBox.Text = Settings.Default.Username;
                this.PasswordTextBox.Password = "password";
                this.Password = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(Settings.Default.Password));
                this.RememberMeBox.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Checked_Box_Yellow.png") as ImageSource;
            }
            else
            {
                this.savePassword = false;
                this.RememberMeBox.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Frame_Box_Yellow.png") as ImageSource;
            }

            // Make sure the mainWindow is created and visible before moving onto command line options
            // Without this errors will occur from the Debug mode option
            mainWindow.Show();
            // Get list of passed command line arguments
            string[] arguments = Environment.GetCommandLineArgs();
            foreach (string option in arguments)
            {
                switch (option.ToLower())
                {
                    // User wants to run in debug mode to create debug dumps to report an issue
                    case "--debug":
                        CreateFullDebugDump();
                        break;
                    // The user has opted to not have error popups and minimal error status messages when possible
                    // They don't want to help the launcher and report issues ;-;
                    // Some errors will still be shown in their normal states (minus the popup) because there are important errors sometimes!
                    case "--noerrors":
                        NoErrorMode = true;
                        break;
                    // The user is claiming to be running custom dependency libs so we should not check if internal ones should be unpacked
                    case "--customdeps":
                        CustomDependencyMode = true;
                        break;
                    // User wants to have the dependency files checked by hashes and not versions
                    case "--customdephash":
                        CheckDependencyHashes = true;
                        break;
                    // User does not want SSL Validation to occur, SSL for server must be out of date
                    case "--skipsslvalidation":
                        SkipSSLValidation = true;
                        break;
                    default:
                        break;
                }
            }

            // Pull status/connection information from the server
            this.ConnectToServers();

            // Unpack required library dependencies to the current directory
            // NOTE: these are never cleaned up by the program - maybe it should try to delete them when closed?
            if (CustomDependencyMode == false)
            {
                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += new DoWorkEventHandler(this.worker_Unpack);
                backgroundWorker.RunWorkerAsync();
            }
        }

        public void ConnectToServers()
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(this.worker_DownloadXML);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.worker_DownloadXMLCompleted);
            backgroundWorker.RunWorkerAsync();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                base.DragMove();
            }
        }

        private void CLOSE_BUTTON_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Environment.Exit(0);
        }

        public static string ConvertSHA256(string value)
        {
            System.Security.Cryptography.SHA256 sHA = System.Security.Cryptography.SHA256.Create();
            byte[] array = sHA.ComputeHash(System.Text.Encoding.Default.GetBytes(value));
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                stringBuilder.Append(array[i].ToString("x2"));
            }
            return stringBuilder.ToString();
        }

        private void FORUM_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://returnofreckoning.com");
        }

        private void ScrollUp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.myScroll.LineUp();
            this.myScroll.LineUp();
            this.CheckScroll();
        }

        private void ScrollDown_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.myScroll.LineDown();
            this.myScroll.LineDown();
            this.CheckScroll();
        }

        private void CheckScroll()
        {
            bool flag = this.myScroll.ScrollableHeight > 0.0 && this.myScroll.VerticalOffset > 0.0;
            bool flag2 = this.myScroll.ScrollableHeight > 0.0 && this.myScroll.VerticalOffset + this.myScroll.ViewportHeight < this.myScroll.ExtentHeight;
            if (flag)
            {
                this.ScrollUp.Opacity = 1.0;
            }
            else
            {
                this.ScrollUp.Opacity = 0.302;
            }
            if (flag2)
            {
                this.ScrollDown.Opacity = 1.0;
                return;
            }
            this.ScrollDown.Opacity = 0.302;
        }

        private void CONNECT_GREY_MouseEnter(object sender, MouseEventArgs e)
        {
            this.connect_grey_frame.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/MouseOver_Button_Grey.png") as ImageSource;
        }

        private void CONNECT_GREY_MouseLeave(object sender, MouseEventArgs e)
        {
            this.connect_grey_frame.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Frame_Button_Grey.png") as ImageSource;
        }

        private void CONNECT_MouseEnter(object sender, MouseEventArgs e)
        {
            this.connect_frame.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/MouseOver_Button_Yellow.png") as ImageSource;
            this.connect_frame.Opacity = 1;
        }

        private void CONNECT_MouseLeave(object sender, MouseEventArgs e)
        {
            this.connect_frame.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Frame_Button_Yellow.png") as ImageSource;
            this.connect_frame.Opacity = .3;
        }

        private void CONNECT_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // If the user doesn't have WAR in the directory don't even let them send their info since it won't matter
            if (File.Exists(Directory.GetCurrentDirectory() + "/WAR.exe") == false)
            {
                Error = "WAR.exe was not found in the same directory as the launcher.";
                return;
            }
            string text = this.UsernameTextBox.Text.ToLower();
            string str = (this.PasswordMode == 0) ? this.PasswordTextBox.Password.ToLower() : this.PasswordTextBox.Password;
            if (this.Password == "")
            {
                this.Password = MainWindow.ConvertSHA256(text + ":" + str);
            }
            if (this.savePassword)
            {
                Settings.Default.Username = this.UsernameTextBox.Text;
                Settings.Default.Password = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(this.Password));
                Settings.Default.Save();
            }
            if (Client._Socket == null || !Client._Socket.Connected)
            {
                // This will be shown even in NoErrorMode due to being a critical usability issue
                Error = "Error encountered with connection. Socket: " + ((Client._Socket == null)?"Null":"Exists") + ", Connected: " + Client._Socket.Connected.ToString();
                return;
            }
            Client.User = text;
            PacketOut packetOut = new PacketOut(3);
            packetOut.WriteString(text);
            packetOut.WriteString(this.Password);
            Client.SendTCP(packetOut);
            this.Connect_button.Visibility = Visibility.Hidden;
            this.Connect_button_grey.Visibility = Visibility.Visible;
        }

        private void PATCH_MouseEnter(object sender, MouseEventArgs e)
        {
            this.patch_frame.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/MouseOver_Button_Red.png") as ImageSource;
            this.patch_frame.Opacity = 1;
        }

        private void PATCH_MouseLeave(object sender, MouseEventArgs e)
        {
            this.patch_frame.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Frame_Button_Red.png") as ImageSource;
            this.patch_frame.Opacity = .3;
        }

        private void PATCH_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.client_DownloadProgressChanged);
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.client_DownloadFileCompleted);
            webClient.DownloadFileAsync(new Uri("http://launcher.returnofreckoning.com/RoRUpdater.exe"), "RoRUpdater.exe");
            this.Updating_Area.Visibility = Visibility.Visible;
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double progress = (double.Parse(e.BytesReceived.ToString()) / double.Parse(e.TotalBytesToReceive.ToString())) * 100.0;
            this.ProgressBarFiller.Width = (double)((int)System.Math.Floor(progress * 469.0 / 100.0));
            this.ProgressText.Text = System.Math.Truncate(progress).ToString() + "%";
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                double num = 100.0;
                this.ProgressBarFiller.Width = (double)((int)System.Math.Floor(num * 469.0 / 100.0));
                this.ProgressText.Text = System.Math.Truncate(num).ToString() + "%";
                new Process
                {
                    StartInfo =
                    {
                        FileName = "RoRUpdater.exe"
                    }
                }.Start();
                System.Environment.Exit(0);
                return;
            }
            this.Popup("Error Downloading updater:" + Environment.NewLine + e.Error);
        }

        private void FORUM_MouseEnter(object sender, MouseEventArgs e)
        {
            this.forum_frame.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/MouseOver_Button_Yellow.png") as ImageSource;
            this.forum_frame.Opacity = 1;
        }

        private void FORUM_MouseLeave(object sender, MouseEventArgs e)
        {
            this.forum_frame.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Frame_Button_Yellow.png") as ImageSource;
            this.forum_frame.Opacity = .3;
        }

        private void myScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.CheckScroll();
        }

        private void RememberMeBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.savePassword == true)
            {
                this.RememberMeBox.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Frame_Box_Yellow.png") as ImageSource;
                savePassword = false;
                Settings.Default.Username = "";
                Settings.Default.Password = "";
                Settings.Default.Save();
            }
            else
            {
                this.RememberMeBox.Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/RoRLauncher;component/Images/Checked_Box_Yellow.png") as ImageSource;
                this.savePassword = true;
                string text = this.UsernameTextBox.Text.ToLower();
                string str = (this.PasswordMode == 0) ? this.PasswordTextBox.Password.ToLower() : this.PasswordTextBox.Password;
                Settings.Default.Username = text;
                if (this.Password == "")
                {
                    this.Password = MainWindow.ConvertSHA256(text + ":" + str);
                }
                Settings.Default.Password = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(this.Password));
                Settings.Default.Save();
            }
        }

        public void Popup(string message)
        {
            ModifyWindowState(WindowState.Normal);
            base.Dispatcher.Invoke(new Action(delegate
            {
                PopupWindow.GetPopup().Show();
                PopupWindow.GetPopup().SetError(message);
                
            }), new object[0]);
        }

        public void EnableConnect(bool enable)
        {
            base.Dispatcher.Invoke(new Action(delegate
            {
                if (enable)
                {
                    this.Connect_button.Visibility = Visibility.Visible;
                    this.Connect_button_grey.Visibility = Visibility.Hidden;
                    return;
                }
                this.Connect_button.Visibility = Visibility.Hidden;
                this.Connect_button_grey.Visibility = Visibility.Visible;
            }), new object[0]);
        }

        private bool Unpack(string fileName)
        {
            System.IO.FileStream fileStream;
            System.IO.Stream manifestResourceStream;
            try
            {
                fileStream = System.IO.File.Open(fileName, System.IO.FileMode.Create);
                manifestResourceStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("RoRLauncher.deps." + fileName);
            }
            catch (System.Exception arg)
            {
                this.Popup("Error unpacking:" + Environment.NewLine + arg);

                if (arg is UnauthorizedAccessException)
                {
                    // Prompt the user to restart the launcher as admin - this normally fixes this error
                    if (IsAdministrator() == false)
                    {
                        if (System.Windows.MessageBox.Show("Running the launcher as Administrator may resolve this error. Would you like to restart it as Administrator now?", "Restart as Administrator", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
                        {
                            MainWindow.mainWindow.RestartAsAdmin();
                        }
                    }
                    else
                    {
                        if (System.Windows.MessageBox.Show("Removing the Read-Only attribute from files in the game directory may resolve this error. Would you like to remove this attribute and restart the application now? Note this will also restart the application as Administrator.", "Remove Read-Only Attribute", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
                        {
                            // User must have read/write access to the directory for this to be allowed
                            RemoveReadOnly(new DirectoryInfo(System.IO.Directory.GetCurrentDirectory()));
                            MainWindow.mainWindow.RestartAsAdmin();
                        }
                    }
                }

                return false;
            }
            try
            {
                manifestResourceStream.CopyTo(fileStream);
                fileStream.Dispose();
                manifestResourceStream.Dispose();
            }
            catch (System.Exception arg2)
            {
                this.Popup("Error unpacking 2:" + Environment.NewLine + arg2);
                return false;
            }
            return true;
        }

        public static void KillProcessByName(string program, bool two = false)
        {
            byte b = 0;
            try
            {
                Process[] processesByName = Process.GetProcessesByName(program);
                Process[] array = processesByName;
                for (int i = 0; i < array.Length; i++)
                {
                    Process process = array[i];
                    b += 1;
                    if (!two || b > 1)
                    {
                        process.Kill();
                        process.WaitForExit();
                        process.Dispose();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MainWindow.mainWindow.Popup(string.Concat(new object[]
                {
                    "Please close ",
                    program,
                    ".exe" + Environment.NewLine,
                    ex
                }));
            }
        }

        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            this.Password = "";
        }

        public void ModifyWindowState(WindowState State)
        {
            base.Dispatcher.Invoke(new Action(delegate
            {
                this.WindowState = State;
            }), new object[0]);
        }

        private void MINIMIZE_BUTTON_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Note that since this form has no Windows Style there will be no animations for minimize/restore
            ModifyWindowState(WindowState.Minimized);
        }

        private void LoginTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // If the Enter key was pressed in either Username or Password box and Connection is allowed
            // Perform the Connect Clicking action
            if (e.Key == Key.Enter)
            {
                if (this.Connect_button.Visibility == Visibility.Visible)
                {
                    CONNECT_MouseLeftButtonDown(null, null);
                }
            }
        }

        private void CreateFullDebugDump()
        {
            DebugDumper dumper = new DebugDumper();
            dumper.Owner = this;
            try
            {
                Debugger debugger = new Debugger();
                debugger.dumper = dumper;
                debugger.DumpConfigs(this);
                debugger.DumpDxDiag();
                debugger.DumpMSInfo32();
                dumper.ShowDialog();
            }
            catch(Exception ex)
            {
                // Don't know why the user would be running both command line options in this case, but they are still getting the 'minimal' version for this
                if (NoErrorMode == false)
                    this.Popup("Error creating dump files:" + Environment.NewLine + ex);
                else
                    Error = "Error creating dump files. --NoErrors was set on the command-line, remove it and run the launcher again for the full error message.";
            }
        }

        public void RestartAsAdmin()
        {
            // This is required to prevent a critical error that prevents the restart from finishing
            try
            {
                Client._Socket.Shutdown(System.Net.Sockets.SocketShutdown.Send);
                Client._Socket.Close();
                Client._Socket.Dispose();
                Client.Close();
            }
            catch{}
            var exeName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
            startInfo.Verb = "runas";
            startInfo.Arguments = Environment.CommandLine;
            // Close a popup window if there is one, just to be safe
            try
            {
                PopupWindow.GetPopup().Close();
            }
            catch{}
            System.Diagnostics.Process.Start(startInfo);
            // If something stopped Shutdown, force an unclean exit because we don't care anymore
            try
            {
                Application.Current.Shutdown();
            }
            catch
            {
                Process.GetCurrentProcess().Kill();
            }
            return;
        }

        public bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public void RemoveReadOnly(DirectoryInfo directory)
        {
            if (directory != null)
            {
                directory.Attributes = FileAttributes.Normal;
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Attributes = FileAttributes.Normal;
                }
                foreach (DirectoryInfo dir in directory.GetDirectories())
                {
                    RemoveReadOnly(dir);
                }
            }
        }
    }
}
