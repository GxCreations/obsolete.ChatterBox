using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Windows.UI.Popups;
using Windows.UI.Core;
using ChatterBox.Client.Presentation.Shared.MVVM;
using Windows.Networking.Connectivity;
using Windows.Networking;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ChatterBox.Client.Presentation.Shared.Services
{
    public class NtpService : DispatcherBindableBase
    {

        public event Action<long> OnNTPTimeAvailable;

        public event Action OnNTPSyncFailed;

        public NtpService(CoreDispatcher uiDispatcher) : base(uiDispatcher)
        {
        }

        private Stopwatch ntpResponseMonitor = new Stopwatch();

        private Windows.UI.Xaml.DispatcherTimer ntpQueryTimer = null;

        /// <summary>
        /// Report whether succeeded in sync with the ntp server or not.
        /// </summary>
        private void NTPQueryTimeout(object sender, object e)
        {
            if (ntpResponseMonitor.IsRunning)
            {
                ntpResponseMonitor.Stop();
                ReportNtpSyncStatus(false);
            }
        }


        /// <summary>
        /// Report whether succeeded in sync with the ntp server or not.
        /// </summary>
        void ReportNtpSyncStatus(bool status, int rtt = 0)
        {
            MessageDialog dialog;
            if (status)
            {
                dialog = new MessageDialog(String.Format("Synced with ntp server. RTT time {0}ms", rtt));
            }
            else
            {
                OnNTPSyncFailed?.Invoke();
                dialog = new MessageDialog("Failed To sync with ntp server.");
            }

            RunOnUiThread(async () =>
            {
                ntpRTTIntervalTimer.Stop();
                await dialog.ShowAsync();
            });

        }

        private int averageNtpRTT = 0; //ms initialized to a invalid number
        private int minNtpRTT = -1;
        const int MaxNtpRTTProbeQuery = 100; // the attempt to get average RTT for NTP query/response
        private int currentNtpQueryCount = 0;
        private Windows.Networking.Sockets.DatagramSocket ntpSocket = null;
        private Windows.UI.Xaml.DispatcherTimer ntpRTTIntervalTimer = null;

        public void abortSync()
        {
            if (ntpRTTIntervalTimer != null)
            {
                ntpRTTIntervalTimer.Stop();
            }

            if (ntpQueryTimer != null)
            {
                ntpQueryTimer.Stop();
            }

            ntpResponseMonitor.Stop();
        }
        /// <summary>
        /// Retrieve the current network time from ntp server  "time.windows.com".
        /// </summary>
        public async void GetNetworkTime(string ntpServer)
        {

            averageNtpRTT = 0; //reset

            currentNtpQueryCount = 0; //reset
            minNtpRTT = -1; //reset

            //NTP uses UDP
            ntpSocket = new Windows.Networking.Sockets.DatagramSocket();
            ntpSocket.MessageReceived += OnNTPTimeReceived;


            if (ntpQueryTimer == null)
            {
                ntpQueryTimer = new Windows.UI.Xaml.DispatcherTimer();
                ntpQueryTimer.Tick += NTPQueryTimeout;
                ntpQueryTimer.Interval = new TimeSpan(0, 0, 5); //5 seconds
            }

            if (ntpRTTIntervalTimer == null)
            {
                ntpRTTIntervalTimer = new Windows.UI.Xaml.DispatcherTimer();
                ntpRTTIntervalTimer.Tick += SendNTPQuery;
                ntpRTTIntervalTimer.Interval = new TimeSpan(0, 0, 0, 0, 200); //200ms

            }

            ntpQueryTimer.Start();

            try
            {
                //The UDP port number assigned to NTP is 123
                await ntpSocket.ConnectAsync(new Windows.Networking.HostName(ntpServer), "123");
                ntpRTTIntervalTimer.Start();

            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception when connect socket: {e.Message}");
                ntpResponseMonitor.Stop();
                ReportNtpSyncStatus(false);
            }

        }

        private async void SendNTPQuery(object sender, object e)
        {
            currentNtpQueryCount++;
            // NTP message size - 16 bytes of the digest (RFC 2030)
            byte[] ntpData = new byte[48];

            //Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            ntpQueryTimer.Start();

            ntpResponseMonitor.Restart();
            await ntpSocket.OutputStream.WriteAsync(ntpData.AsBuffer());
        }

        /// <summary>
        /// Event hander when receiving response from the ntp server.
        /// </summary>
        /// <param name="socket">The udp socket object which triggered this event </param>
        /// <param name="eventArguments">event information</param>
        void OnNTPTimeReceived(Windows.Networking.Sockets.DatagramSocket socket, Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs eventArguments)
        {
            int currentRTT = (int)ntpResponseMonitor.ElapsedMilliseconds;

            Debug.WriteLine($"current RTT {currentRTT}");


            ntpResponseMonitor.Stop();

            if (currentNtpQueryCount < MaxNtpRTTProbeQuery)
            {
                //we only trace 'min' RTT within the RTT probe attempts
                if (minNtpRTT == -1 || minNtpRTT > currentRTT)
                {

                    minNtpRTT = currentRTT;

                    if (minNtpRTT == 0)
                        minNtpRTT = 1; //in case we got response so  fast, consider it to be 1ms.
                }


                averageNtpRTT = (averageNtpRTT * (currentNtpQueryCount - 1) + currentRTT) / currentNtpQueryCount;

                if (averageNtpRTT < 1)
                {
                    averageNtpRTT = 1;
                }

                RunOnUiThread(() =>
                {
                    ntpQueryTimer.Stop();
                    ntpRTTIntervalTimer.Start();
                });

                return;

            }

            //if currentRTT is good enough, e.g.: closer to minRTT, then, we don't have to continue to query.
            if (currentRTT > (averageNtpRTT + minNtpRTT) / 2)
            {
                RunOnUiThread(() =>
                {
                    ntpQueryTimer.Stop();
                    ntpRTTIntervalTimer.Start();
                });

                return;
            }


            byte[] ntpData = new byte[48];

            eventArguments.GetDataReader().ReadBytes(ntpData);

            //Offset to get to the "Transmit Timestamp" field (time at which the reply 
            //departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;

            //Get the seconds part
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

            //Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

            //Convert From big-endian to little-endian
            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            ulong milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            RunOnUiThread(() =>
            {
                OnNTPTimeAvailable?.Invoke((long)milliseconds + currentRTT / 2);
            });

            socket.Dispose();
            ReportNtpSyncStatus(true, currentRTT);
        }


        static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }

    }
}
