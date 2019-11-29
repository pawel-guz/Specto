using System; 
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Specto.Relay
{
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
        public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);
        public event DataReceivedEventHandler OnDataReceived;
        private event EventHandler OnListenCompleted;

        private UdpClient Client { get; set; }
        private IPEndPoint TargetEP;
        public int Port { get; private set; }
        private bool SpecificMode { get; set; } 
        private bool IsListening { get; set; }  

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

        public DataReceivedEventArgs Listen(int ms)
        {
            IPEndPoint ep = (SpecificMode ? TargetEP : new IPEndPoint(IPAddress.Any, 0));
            IsListening = true;
            Thread.Sleep(ms);
            byte[] received = null;
            if (Client.Available > 0)
            {
                try
                {
                    received = Client.Receive(ref ep);
                    //DataReceived?.Invoke(this, new DataReceivedEventArgs(Encoding.ASCII.GetString(received), deviceEP));
                }
                catch
                {
                    Console.WriteLine("Nothing to receive.");
                }
            }
            IsListening = false;
            OnListenCompleted?.Invoke(this, EventArgs.Empty);

            return new DataReceivedEventArgs(received == null ? "" : Encoding.ASCII.GetString(received), ep);
        }

        public async Task ListenAsync(int ms)
        {
            IsListening = true;
            Client.BeginReceive(Received, null);
            await Task.Delay(ms);
            IsListening = false;
        }
        
        void Received(IAsyncResult result)
        {
            IPEndPoint ep = (SpecificMode ? TargetEP : new IPEndPoint(IPAddress.Any, 0));
            var received = Client.EndReceive(result, ref ep);
            OnDataReceived?.Invoke(this, new DataReceivedEventArgs(Encoding.ASCII.GetString(received), ep));

            if(IsListening)
                Client.BeginReceive(Received, null);
            else
                OnListenCompleted?.Invoke(this, EventArgs.Empty);
        }

        public void SendCommand(Command message)
        {
            if (message != null)
            {
                var msg = message.ParseToJSON();
                var send_buffer = Encoding.ASCII.GetBytes(msg);

                Client.Send(send_buffer, send_buffer.Length, TargetEP);
                Console.WriteLine($"Sending {msg} to {TargetEP.Address.ToString()}");
            }
        }

        
        public void Dispose()
        {
            OnListenCompleted += (s, e) =>
            {
                OnDataReceived = null;
                Client.Close();
                Client.Dispose();
            };

            if (!IsListening)
            {
                OnListenCompleted(this, EventArgs.Empty);
            }
        }
    }
}
