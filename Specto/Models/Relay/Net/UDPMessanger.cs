using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Specto.Relay
{
    public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);
    public class DataReceivedEventArgs
    {
        public string Data { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }

        public DataReceivedEventArgs(string data, IPEndPoint remoteEndPoint)
        {
            Data = data;
            RemoteEndPoint = remoteEndPoint;
        }
    }

    public class UDPMessenger : IDisposable
    {
        public event DataReceivedEventHandler OnDataReceived; 

        private UdpClient Client { get; set; }
        private IPEndPoint TargetEP;
        public int Port { get; private set; }
        private bool SpecificMode { get; set; } 
        public bool IsListening { get; private set; }  
        public float ListeningProgress { get; private set; }

        /// <summary>
        /// Creates messanger to communicate via broadcast ip.
        /// </summary> 
        public UDPMessenger(int port)
        { 
            Client = new UdpClient { EnableBroadcast = true };
            TargetEP = new IPEndPoint(IPAddress.Broadcast, port); 
            Port = port;
            SpecificMode = false;
            IsListening = false;
        }

        /// <summary>
        /// Creates messanger to communicate with specific IP.
        /// </summary> 
        public UDPMessenger(int port, IPAddress targetIP)
        {
            Client = new UdpClient { EnableBroadcast = true };
            TargetEP = new IPEndPoint(targetIP, port);

            Port = port;
            SpecificMode = true;
        }

        public async Task<DataReceivedEventArgs> DirectListenAsync(int ms)
        {
            if (IsListening)
                return null;

            IPEndPoint ep = (SpecificMode ? TargetEP : new IPEndPoint(IPAddress.Any, 0));
            byte[] received = null;

            IsListening = true;
            await Task.Delay(ms); 
            if (Client.Available > 0)
            {
                try { received = Client.Receive(ref ep); }
                catch { }
            }
            IsListening = false; 
            return new DataReceivedEventArgs(received == null ? "" : Encoding.ASCII.GetString(received), ep);
        }

        //public async Task<List<DataReceivedEventArgs>> DirectListenForManyAsync(int ms)
        //{
        //    if (IsListening)
        //        return null;

        //    IsListening = true;
        //    IPEndPoint ep = (SpecificMode ? TargetEP : new IPEndPoint(IPAddress.Any, 0));
        //    var datas = new List<DataReceivedEventArgs>();

        //    Stopwatch stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    while (stopwatch.ElapsedMilliseconds < ms)
        //    { 
        //        var data = await DirectListenAsync(ms / )
        //        if (data != null)
        //            datas.Add(data);
        //    } 

        //    IsListening = false;
        //    return datas;
        //}

        public async Task EventListenAsync(int ms)
        {
            IsListening = true;
            IPEndPoint ep = (SpecificMode ? TargetEP : new IPEndPoint(IPAddress.Any, 0)); 
            Client.BeginReceive(Received, null);
            await Task.Delay(ms);
            Client.EndReceive(null, ref ep);
            IsListening = false; 
        }

        void Received(IAsyncResult result)
        {  
            try
            { 
                IPEndPoint ep = (SpecificMode ? TargetEP : new IPEndPoint(IPAddress.Any, 0));
                var received = Client.EndReceive(result, ref ep);
                if(received != null)
                    OnDataReceived?.Invoke(this, new DataReceivedEventArgs(Encoding.ASCII.GetString(received), ep));

                if (IsListening)
                    Client.BeginReceive(Received, null); 
            }
            catch
            {
                return;
            }
        }

        public void SendCommand(Command message)
        {
            if (message != null)
            {
                var msg = message.ParseToJSON();
                var send_buffer = Encoding.ASCII.GetBytes(msg);

                try
                {
                    Client.Send(send_buffer, send_buffer.Length, TargetEP);
                }
                catch { }
            }
        }

        
        public void Dispose()
        {
            OnDataReceived = null;
            Client.Close();
            Client.Dispose(); 
        }
    }
}
