using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCommon
{
    /// <summary>
    /// 消息发送器
    /// </summary>
    public class Sender
    {
        TcpClient client;
        Stream streamToTran;

        private bool bIsConnect = false;

        public bool Connect(string ipAddress, int ipPort, out string strException)
        {
            strException = string.Empty;
            try
            {
                client = new TcpClient();
                client.Connect(ipAddress, ipPort);
                bIsConnect = true;
                streamToTran = client.GetStream();
                return true;
            }
            catch (System.Exception ex)
            {
                strException = ex.Message;
                Console.WriteLine(strException);
                return false;
            }
        }

        public bool SendMessage(byte[] btAryBuffer)
        {
            try
            {
                lock (streamToTran)
                {
                    streamToTran.Write(btAryBuffer, 0, btAryBuffer.Length);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<byte[]> ReceiveMessage()
        {
            try
            {
                if (!streamToTran.CanRead) return null;

                var buffer = new byte[1024];

                await using var ms = new MemoryStream();
                while (true)
                {
                    streamToTran = client.GetStream();

                    var read = await streamToTran.ReadAsync(buffer, 0, buffer.Length);

                    if (read > 0)
                    {
                        ms.Write(buffer, 0, read);

                        return ms.ToArray();
                    }
                    else
                    {
                        throw new SocketException();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("接收数据异常：" + ex.Message);
                return null;
            }
        }

        public void SignOut()
        {
            if (streamToTran != null)
                streamToTran.Dispose();
            if (client != null)
                client.Close();

            bIsConnect = false;
        }

        public bool IsConnect()
        {
            return bIsConnect;
        }
    }
}
