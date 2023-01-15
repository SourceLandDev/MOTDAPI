using LiteLoader.Logger;
using LiteLoader.NET;
using LiteLoader.RemoteCall;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MOTDAPI;
[PluginMain("MOTDAPI")]
public class Main : IPluginInitializer
{
    public string Introduction => "MOTDAPI";
    public Dictionary<string, string> MetaData => new();
    public Version Version => new(1, 0, 0);
    private static readonly byte[] BEData = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 0, 254, 254, 254, 254, 253, 253, 253, 253, 18, 52, 86, 120, 0, 0, 0, 0, 0, 0, 0, 0 };
    private static readonly byte[] JEData = new byte[] { 6, 0, 0, 0, 0x63, 0xdd, 1, 1, 0 };
    public void OnInitialize()
    {
        Logger logger = new("MOTDAPI");
        _ = RemoteCallAPI.ExportAs("MOTDAPI", "GetFromBE", (string addr, ushort port) =>
        {
            byte[] back = new byte[1024 * 8]; // 防止返回内容过多导致撑爆字节数组
            int length = 0;
            try
            {
                IPAddress ip = GetIP(addr);
                Socket socket = new(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                socket.Connect(ip, port);
                _ = socket.Send(BEData);
                length = socket.Receive(back);
            }
            catch (SocketException ex)
            {
                logger.Debug.WriteLine(ex);
            }
            return length > 35 ?
                   Encoding.UTF8.GetString(back, 35, length - 35) : // 基岩版服务器返回数据包头部有35个byte无效
                   Encoding.UTF8.GetString(back);
        });
        _ = RemoteCallAPI.ExportAs("MOTDJEAPI", "GetFromJE", (string addr, ushort port) =>
        {
            byte[] back = new byte[1024 * 16]; // 防止返回内容过多导致撑爆字节数组
            int length = 0;
            try
            {
                IPAddress ip = GetIP(addr);
                Socket socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ip, port);
                _ = socket.Send(JEData);
                length = socket.Receive(back);
                socket.Close();
            }
            catch (SocketException ex)
            {
                logger.Debug.WriteLine(ex);
            }
            return length > 5 ?
                Encoding.UTF8.GetString(back, 5, length - 5) : // Java服务器返回数据包头部有5个byte无效
                Encoding.UTF8.GetString(back);
        });
    }

    /// <summary>
    /// 获取IP地址
    /// </summary>
    /// <param name="addr">域名或IP字符串</param>
    /// <returns>IP地址</returns>
    private IPAddress GetIP(string addr)
    {
        if (!(IPAddress.TryParse(addr, out IPAddress ip) && ip != null)) // 无法直接转换的ip尝试使用dns解析
        {
            ip = Dns.GetHostAddresses(addr)[0];
        }
        return ip;
    }
}
