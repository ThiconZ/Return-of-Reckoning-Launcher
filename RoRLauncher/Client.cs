using MYPHandler;
using nsHashDictionary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace RoRLauncher
{
    public static class Client
    {
        public static int Version = 1;

        public static string IP = "127.0.0.1";

        public static int Port = 8000;

        public static bool Started = false;

        public static string User;

        public static string Auth;

        public static string Language = "";

        public static Socket _Socket;

        private static byte[] m_tcpSendBuffer = new byte[65000];

        private static readonly Queue<byte[]> m_tcpQueue = new Queue<byte[]>(256);

        private static bool m_sendingTcp = false;

        private static readonly System.AsyncCallback m_asyncTcpCallback = new System.AsyncCallback(Client.AsyncTcpSendCallback);

        private static readonly System.AsyncCallback ReceiveCallback = new System.AsyncCallback(Client.OnReceiveHandler);

        private static byte[] _pBuf = new byte[2048];

        private static int ReconnectAttempts = 0;

        public static void Print(string Message)
        {
            MainWindow.mainWindow.Error = Message;
        }

        public static void Popup(string Message)
        {
            MainWindow.mainWindow.Popup(Message);
        }

        public static bool Connect()
        {
            try
            {
                if (Client._Socket != null)
                {
                    Client._Socket.Close();
                }
                Client._Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Client._Socket.Connect(Client.IP, Client.Port);
                Client.BeginReceive();
                Client.SendCheck();
            }
            catch (System.Exception arg)
            {
                if (MainWindow.mainWindow.NoErrorMode == false)
                    Client.Popup("Could not connect to Login server\n" + arg);
                else
                    Client.Print("Could not connect to Login server");
                return false;
            }
            return true;
        }

        public static void Close()
        {
            try
            {
                if (Client._Socket != null)
                {
                    Client._Socket.Close();
                }
            }
            catch (System.Exception)
            {
            }
        }

        public static void UpdateLanguage()
        {
            if (Client.Language.Length <= 0)
            {
                return;
            }
            int num = 1;
            string language;
            switch (language = Client.Language)
            {
                case "English":
                    num = 1;
                    break;
                case "French":
                    num = 2;
                    break;
                case "Deutch":
                    num = 3;
                    break;
                case "Italian":
                    num = 4;
                    break;
                case "Spanish":
                    num = 5;
                    break;
                case "Korean":
                    num = 6;
                    break;
                case "Chinese":
                    num = 7;
                    break;
                case "Japanese":
                    num = 9;
                    break;
                case "Russian":
                    num = 10;
                    break;
            }
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            try
            {
                System.IO.Directory.SetCurrentDirectory(currentDirectory + "\\..\\user\\");
                System.IO.StreamReader streamReader = new System.IO.StreamReader("\\user\\UserSettings.xml");
                string text = "";
                string text2;
                while ((text2 = streamReader.ReadLine()) != null)
                {
                    Client.Print(text2);
                    int num3 = text2.IndexOf("Language id=");
                    if (num3 > 0)
                    {
                        num3 = text2.IndexOf("\"") + 1;
                        int num4 = text2.LastIndexOf("\"");
                        text2 = text2.Remove(num3, num4 - num3);
                        text2 = text2.Insert(num3, string.Concat(num));
                    }
                    text = text + text2 + Environment.NewLine;
                }
                streamReader.Close();
                System.IO.StreamWriter streamWriter = new System.IO.StreamWriter("UserSettings.xml", false);
                streamWriter.Write(text);
                streamWriter.Flush();
                streamWriter.Close();
            }
            catch (System.Exception ex)
            {
                Client.Popup("Language:" + Environment.NewLine + ex.ToString());
            }
            System.IO.Directory.SetCurrentDirectory(currentDirectory);
        }

        public static void UpdateRealms()
        {
            PacketOut packet = new PacketOut(7);
            Client.SendTCP(packet);
        }

        public static void SendTCP(PacketOut packet)
        {
            packet.WritePacketLength();
            byte[] buffer = packet.GetBuffer();
            Client.SendTCP(buffer);
        }

        public static void SendTCP(byte[] buf)
        {
            if (Client.m_tcpSendBuffer == null)
            {
                return;
            }
            if (Client._Socket.Connected)
            {
                try
                {
                    lock (Client.m_tcpQueue)
                    {
                        if (Client.m_sendingTcp)
                        {
                            Client.m_tcpQueue.Enqueue(buf);
                            return;
                        }
                        Client.m_sendingTcp = true;
                    }
                    System.Buffer.BlockCopy(buf, 0, Client.m_tcpSendBuffer, 0, buf.Length);
                    Client._Socket.BeginSend(Client.m_tcpSendBuffer, 0, buf.Length, SocketFlags.None, Client.m_asyncTcpCallback, null);
                }
                catch
                {
                    Client.Close();
                }
            }
        }

        private static void AsyncTcpSendCallback(System.IAsyncResult ar)
        {
            try
            {
                Queue<byte[]> tcpQueue = Client.m_tcpQueue;
                Client._Socket.EndSend(ar);
                int num = 0;
                byte[] tcpSendBuffer = Client.m_tcpSendBuffer;
                if (tcpSendBuffer != null)
                {
                    lock (tcpQueue)
                    {
                        if (tcpQueue.Count > 0)
                        {
                            num = Client.CombinePackets(tcpSendBuffer, tcpQueue, tcpSendBuffer.Length);
                        }
                        if (num <= 0)
                        {
                            Client.m_sendingTcp = false;
                            return;
                        }
                    }
                    Client._Socket.BeginSend(tcpSendBuffer, 0, num, SocketFlags.None, Client.m_asyncTcpCallback, null);
                }
            }
            catch (System.Exception)
            {
                Client.Close();
            }
        }

        private static int CombinePackets(byte[] buf, Queue<byte[]> q, int length)
        {
            int num = 0;
            do
            {
                byte[] array = q.Peek();
                if (num + array.Length > buf.Length)
                {
                    if (num != 0)
                    {
                        break;
                    }
                    q.Dequeue();
                }
                else
                {
                    System.Buffer.BlockCopy(array, 0, buf, num, array.Length);
                    num += array.Length;
                    q.Dequeue();
                }
            }
            while (q.Count > 0);
            return num;
        }

        public static void SendTCPRaw(PacketOut packet)
        {
            Client.SendTCP((byte[])packet.GetBuffer().Clone());
        }

        private static void OnReceiveHandler(System.IAsyncResult ar)
        {
            try
            {
                int num = Client._Socket.EndReceive(ar);
                if (num > 0)
                {
                    byte[] pBuf = Client._pBuf;
                    int size = num;
                    PacketIn packet = new PacketIn(pBuf, 0, size);
                    Client.OnReceive(packet);
                    Client.BeginReceive();
                }
                else
                {
                    Client.Close();
                }

                //throw new SocketException(0x274d); // Force a host refused connection error for testing

                // Reset the reconnect attempts back to 0 since it seems we had a full connection success finally
                Client.ReconnectAttempts = 0;
            }
            catch (SocketException ex)
            {
                if (Client.ReconnectAttempts < 9) // Only allow 10 reconnect attempts in a row, don't want to lock up the application or flood the server
                {
                    // This error will happen often after the first time is occurs and will continue to happen for a period of time after.
                    // It typically is an issue of active host refusing connection then terminating all connections.
                    // For now though forcing another connection attempt appears to 'fix' the issue and allow launching the game when Connect is clicked
                    if (MainWindow.mainWindow.NoErrorMode == false)
                    {
                        Client.Print("Socket Receive Handler Encountered An Error. See the Error popup for more information.");
                        string ErrorHelpBuilder = "";
                        ErrorHelpBuilder += "The launcher has attempted to fix the Socket Receive Handler error automatically.";
                        ErrorHelpBuilder += " If clicking Connect again does not work please try relaunching the program.";
                        ErrorHelpBuilder += " Visit the website for help if you continue to see this error.";
                        ErrorHelpBuilder += Environment.NewLine + Environment.NewLine + "Debug Error (Include this when posting for help online):" + Environment.NewLine + Environment.NewLine;
                        Client.Popup(ErrorHelpBuilder + ex);
                    }
                    else
                    {
                        Client.Print("Socket Receive Handler Encountered An Error.");
                    }

                    Client.ReconnectAttempts += 1;

                    // Use a timer to spread out reconnection attempts to try and help prevent connection collisions
                    MainWindow.mainWindow.Dispatcher.Invoke(new Action(delegate
                    {
                        System.Windows.Threading.DispatcherTimer ReconnectDelay = new System.Windows.Threading.DispatcherTimer();
                        ReconnectDelay.Interval = new TimeSpan(0, 0, 0, 0, new Random().Next(5, 500)); // Give a decent amount of variation but don't make it take too long, no one likes slow stuff
                        ReconnectDelay.Tick += new EventHandler(delegate (object s, EventArgs ev)
                        {
                            Client.Connect();
                            ReconnectDelay.IsEnabled = false;
                            ReconnectDelay.Stop();
                        });
                        ReconnectDelay.IsEnabled = true;
                        ReconnectDelay.Start();
                    }), new object[0]);
                }
                else
                {
                    if (MainWindow.mainWindow.NoErrorMode == false)
                    {
                        Client.Print("Socket Receive Handler Encountered An Error. Self-repair attempt limit reached. Please restart the launcher to continue. See the Error popup for more information.");
                        string ErrorHelpBuilder = "";
                        ErrorHelpBuilder += "The launcher has reach the maximum self-repair attempts and none were successful.";
                        ErrorHelpBuilder += " Please try relaunching the program to continue.";
                        ErrorHelpBuilder += " Visit the website for help if you continue to see this error.";
                        ErrorHelpBuilder += Environment.NewLine + Environment.NewLine + "Debug Error (Include this when posting for help online):" + Environment.NewLine + Environment.NewLine;
                        Client.Popup(ErrorHelpBuilder + ex);
                    }
                    else
                    {
                        Client.Print("Socket Receive Handler Encountered An Error. Self-repair attempt limit reached. Please restart the launcher to continue.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Hopefully no unknown errors occur, but in the event one does happen it's best to have a clean handler for it
                if (MainWindow.mainWindow.NoErrorMode == false)
                {
                    Client.Print("An unknown error was encountered. See the Error popup for more information.");
                    string ErrorHelpBuilder = "";
                    ErrorHelpBuilder += "The launcher has encounted an error in the Socket Receive Handler that it could not self-repair.";
                    ErrorHelpBuilder += " If clicking Connect again does not work please try relaunching the program.";
                    ErrorHelpBuilder += " Visit the website for help if you continue to see this error.";
                    ErrorHelpBuilder += Environment.NewLine + Environment.NewLine + "Debug Error (Include this when posting for help online):" + Environment.NewLine + Environment.NewLine;
                    Client.Popup(ErrorHelpBuilder + ex);
                }
                else
                {
                    Client.Print("An unknown error was encountered. Please disable --NoErrors command-line option and run the launcher again for the full error message.");
                }
            }
        }

        public static void BeginReceive()
        {
            if (Client._Socket != null && Client._Socket.Connected)
            {
                int num = Client._pBuf.Length;
                if (0 >= num)
                {
                    Client.Close();
                    return;
                }
                Client._Socket.BeginReceive(Client._pBuf, 0, num, SocketFlags.None, Client.ReceiveCallback, null);
            }
        }

        public static void OnReceive(PacketIn packet)
        {
            lock (packet)
            {
                packet.Size = (ulong)packet.GetUint32();
                packet.Opcode = (ulong)packet.GetUint8();
                Client.Handle(packet);
            }
        }

        public static void Handle(PacketIn packet)
        {
            if (!System.Enum.IsDefined(typeof(Opcodes), (byte)packet.Opcode))
            {
                Client.Popup("Invalid opcode: " + packet.Opcode.ToString("X02"));
                return;
            }
            Opcodes opcodes = (Opcodes)packet.Opcode;
            switch (opcodes)
            {
                case Opcodes.LCR_CHECK:
                    switch (packet.GetUint8())
                    {
                        case 0:
                            Client.Start();
                            return;
                        case 1:
                            {
                                string @string = packet.GetString();
                                Client.Print(@string);
                                Client.Close();
                                return;
                            }
                        case 2:
                            {
                                string string2 = packet.GetString();
                                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(string2);
                                System.IO.FileInfo fileInfo = new System.IO.FileInfo("mythloginserviceconfig.xml");
                                System.IO.FileStream fileStream = fileInfo.Create();
                                fileStream.Write(bytes, 0, bytes.Length);
                                fileStream.Close();
                                return;
                            }
                        default:
                            return;
                    }
                    break;
                case Opcodes.CL_START:
                    break;
                case Opcodes.LCR_START:
                    {
                        byte @uint = packet.GetUint8();
                        if (@uint == 1)
                        {
                            Client.Print("Invalid Username/Password");
                            MainWindow.mainWindow.EnableConnect(true);
                            return;
                        }
                        if (@uint == 2)
                        {
                            Client.Print("Your account has been suspended.");
                            MainWindow.mainWindow.EnableConnect(true);
                            return;
                        }
                        if (@uint == 3)
                        {
                            Client.Print("Your account is not active.");
                            MainWindow.mainWindow.EnableConnect(true);
                            return;
                        }
                        if (@uint > 3)
                        {
                            Client.Print("Invalid Response: " + @uint);
                            MainWindow.mainWindow.EnableConnect(true);
                            return;
                        }
                        Client.Auth = packet.GetString();
                        Client.Print("");
                        try
                        {
                            MainWindow.KillProcessByName("WAR", false);
                            MainWindow.mainWindow.EnableConnect(false);
                            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
                            Client.patchExe();
                            Client.UpdateWarData();
                            Process process = new Process();
                            process.StartInfo.FileName = "WAR.exe";
                            process.StartInfo.Arguments = " --acctname=" + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(Client.User)) + " --sesstoken=" + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(Client.Auth));
                            process.EnableRaisingEvents = true;
                            process.Exited += new System.EventHandler(Client.war_Exited);
                            process.Start();
                            
                            System.IO.Directory.SetCurrentDirectory(currentDirectory);
                            
                            // Minimize the launcher once the game process is started
                            MainWindow.mainWindow.ModifyWindowState(System.Windows.WindowState.Minimized);
                        }
                        catch (System.UnauthorizedAccessException ex)
                        {
                            if (MainWindow.mainWindow.NoErrorMode == false)
                                Client.Popup("Access to WAR.exe is denied. Please obtain access permissions to the Warhammer directory and files and try again: " + Environment.NewLine + ex.ToString());
                            else
                                Client.Print("Error starting game. User lacks file access permissions");

                            // Prompt the user to restart the launcher as admin - this normally fixes this error
                            if (MainWindow.mainWindow.IsAdministrator() == false)
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
                                    MainWindow.mainWindow.RemoveReadOnly(new DirectoryInfo(System.IO.Directory.GetCurrentDirectory()));
                                    MainWindow.mainWindow.RestartAsAdmin();
                                }                                
                            }
                        }
                        catch (System.Exception ex)
                        {
                            if (MainWindow.mainWindow.NoErrorMode == false)
                                Client.Popup("Error starting: " + ex.ToString());
                            else
                                Client.Print("Error starting game");
                        }
                        break;
                    }
                default:
                    if (opcodes != Opcodes.LCR_INFO)
                    {
                        return;
                    }
                    break;
            }
        }

        public static void Start()
        {
            if (Client.Started)
            {
                return;
            }
            Client.Started = true;
        }

        public static void SendCheck()
        {
            PacketOut packetOut = new PacketOut(1);
            packetOut.WriteUInt32((uint)Client.Version);
            System.IO.FileInfo fileInfo = new System.IO.FileInfo("mythloginserviceconfig.xml");
            if (fileInfo.Exists)
            {
                packetOut.WriteByte(1);
                packetOut.WriteUInt64((ulong)fileInfo.Length);
            }
            else
            {
                packetOut.WriteByte(0);
            }
            Client.SendTCP(packetOut);
        }

        public static void patchExe()
        {
            if (Hash.GetMd5HashFromFile(System.IO.Directory.GetCurrentDirectory() + "\\WAR.exe") == "8fc62753982d50cf6a6b73025adf98fb")
            {
                return;
            }
            using (System.IO.Stream stream = new System.IO.FileStream(System.IO.Directory.GetCurrentDirectory() + "\\WAR.exe", System.IO.FileMode.OpenOrCreate))
            {
                int num = 5603265;
                stream.Seek((long)num, System.IO.SeekOrigin.Begin);
                stream.WriteByte(1);
                byte[] buffer = new byte[]
                {
                    144,
                    144,
                    144,
                    144,
                    87,
                    139,
                    248,
                    235,
                    50
                };
                int num2 = 5603531;
                stream.Seek((long)num2, System.IO.SeekOrigin.Begin);
                stream.Write(buffer, 0, 9);
                byte[] buffer2 = new byte[]
                {
                    144,
                    144,
                    144,
                    144,
                    235,
                    8
                };
                int num3 = 5603659;
                stream.Seek((long)num3, System.IO.SeekOrigin.Begin);
                stream.Write(buffer2, 0, 6);
            }
        }

        public static void UpdateWarData()
        {
            try
            {
                System.IO.FileStream fileStream = new System.IO.FileStream(System.IO.Directory.GetCurrentDirectory() + "\\mythloginserviceconfig.xml", System.IO.FileMode.Open, System.IO.FileAccess.Read);
                HashDictionary hashDictionary = new HashDictionary();
                hashDictionary.AddHash(1071658597u, 882780812u, "mythloginserviceconfig.xml", 0);
                MYPHandler.MYPHandler mYPHandler = new MYPHandler.MYPHandler("data.myp", null, null, hashDictionary);
                mYPHandler.GetFileTable();
                FileInArchive fileInArchive = mYPHandler.SearchForFile("mythloginserviceconfig.xml");
                if (fileInArchive != null)
                {
                    if (System.IO.File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\mythloginserviceconfig.xml"))
                    {
                        mYPHandler.ReplaceFile(fileInArchive, fileStream);
                        fileStream.Close();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Client.Popup("Data.myp:" + Environment.NewLine + ex.ToString());
            }
        }

        private static void war_Exited(object sender, System.EventArgs e)
        {
            // Call up the connection systems again now that the game has closed
            // This will allow the user to click Connect again to start the game another time all during the same session
            MainWindow.mainWindow.Dispatcher.Invoke(new Action(delegate
            {
                MainWindow.mainWindow.ConnectToServers();
            }), new object[0]);

            // Restore the launcher to the screen
            MainWindow.mainWindow.ModifyWindowState(System.Windows.WindowState.Normal);
        }
    }
}
