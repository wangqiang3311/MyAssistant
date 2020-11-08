using System;
using System.Net.Sockets;
using System.Threading;

namespace TCPServerTestTool
{
    public class ManyMetadata : IDisposable
    {
        public TcpClient TcpClient { get; set; }
        public int? LinkId { get; set; }
        public NetworkStream NetworkStream { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
        public CancellationToken Token { get; set; }
        public object SendLock { get; set; }

        public ManyMetadata(TcpClient client)
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