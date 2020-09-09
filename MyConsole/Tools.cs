using Acme.Common.Utils;
using AMWD.Modbus.Common.Util;
using ServiceStack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YCIOT.ModbusPoll.AnSen.Common
{
    public class Tools
    {
        public static string BytesToHexString(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.AppendFormat("{0:X2} ", b);
            }
            return sb.ToString();
        }

        public static bool IsClientConnected(TcpClient client)
        {
            if (client.Connected)
            {
                if (client.Client.Poll(0, SelectMode.SelectWrite) && (!client.Client.Poll(0, SelectMode.SelectError)))
                {
                    var buffer = new byte[1];
                    return client.Client.Receive(buffer, SocketFlags.Peek) != 0;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static async Task<byte[]> DataReadAsync(TcpClient client, CancellationToken token)
        {
            if (token.IsCancellationRequested) throw new OperationCanceledException();

            var stream = client.GetStream();
            if (!stream.CanRead) return null;
            if (!stream.DataAvailable) return null;

            var buffer = new byte[1024];

            await using var ms = new MemoryStream();
            while (true)
            {
                var read = await stream.ReadAsync(buffer, 0, buffer.Length, token);

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

        public static async Task<byte[]> DataReadAsync(TcpClient client, CancellationToken token, int timeout = 3000, int dataLength = 25)
        {
            if (token.IsCancellationRequested) throw new OperationCanceledException();

            var stream = client.GetStream();
            if (!stream.CanRead) return null;
            if (!stream.DataAvailable) return null;

            var buffer = new byte[1024];

            await using var ms = new MemoryStream();

            DateTime start = DateTime.Now;

            while (true)
            {
                var read = await stream.ReadAsync(buffer, 0, buffer.Length, token);

                if (read > 0)
                {
                    if (read != dataLength)
                    {
                        DateTime end = DateTime.Now;
                        if (end.Subtract(start).TotalMilliseconds > timeout)
                        {
                            Console.WriteLine("获取数据超时");
                            return null;
                        }
                        continue;
                    }

                    ms.Write(buffer, 0, read);

                    return ms.ToArray();
                }
                else
                {
                    throw new SocketException();
                }
            }
        }

        public static void Cacular(byte[] data)
        {
            //DtuId：212
            //WellId：3
            //配注仪状态: 0
            Console.WriteLine($"配注仪状态 : {data[4]}");
            //设定流量回读: 0.17
            var t = BCDUtils.BCDToUshort((ushort)(data[5] << 8 | data[6]));
            Console.WriteLine($"设定流量回读 : {t / 100.0}");

            //瞬时流量: 0.15
            t = BCDUtils.BCDToUshort((ushort)(data[7] << 8 | data[8]));
            Console.WriteLine($"瞬时流量 : { t / 100.0}");

            //累计流量: 607.26
            var t1 = BCDUtils.BCDToUshort((ushort)(data[9] << 8 | data[10]));
            var t2 = BCDUtils.BCDToUshort((ushort)(data[11] << 8 | data[12]));
            Console.WriteLine($"累计流量 : {(t1 * 10000 + t2) / 100.0}");
            //扩展: 0.0
            //水井压力: 7.35
            t = BCDUtils.BCDToUshort((ushort)(data[17] << 8 | data[18]));
            Console.WriteLine($"水井压力 : {t / 100.0}");
        }

        public static byte[] GetRequestData(uint slotId = 1)
        {
            var data = new byte[] { 0x01, 0x04, 0x00, 0x01, 0x00, 0x0A, 0x94, 0x08 };
            uint address = 14 + (slotId - 1) * 10;

            //槽位随机测试
            var slot = new Random().Next(1, 2);
            address = 14 + ((uint)slot - 1) * 10;
            Console.WriteLine($"当前槽位为：{slot}");

            data[2] = (byte)(address >> 8);
            data[3] = (byte)(address & 0xFF);
            var length = data.Length;
            var crc32 = data.Take(length - 2).ToArray().CRC16();

            data[length - 2] = crc32[0];

            data[length - 1] = crc32[1];
            return data;
        }
    }
}
