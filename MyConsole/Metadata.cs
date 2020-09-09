using System;
using System.Net.Sockets;
using System.Threading;

namespace YCIOT.ModbusPoll.AnSen
{
    public class Metadata : IDisposable
    {
        public TcpClient TcpClient { get; set; }
        public NetworkStream NetworkStream { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
        public CancellationToken Token { get; set; }
        public object SendLock { get; set; }

        public Metadata(TcpClient client)
        {
            TcpClient = client;
            NetworkStream = TcpClient.GetStream();
            TokenSource = new CancellationTokenSource();
            Token = TokenSource.Token;
            SendLock = new object();
        }

        public void Dispose()
        {
            if (NetworkStream != null)
            {
                NetworkStream.Close();
                NetworkStream.Dispose();
            }

            TokenSource.Cancel();

            if (TcpClient == null) return;

            TcpClient.Close();
            TcpClient.Dispose();
        }
    }
}