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
    private static readonly byte[] data = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 0, 254, 254, 254, 254, 253, 253, 253, 253, 18, 52, 86, 120, 0, 0, 0, 0, 0, 0, 0, 0 };
    public void OnInitialize()
    {
        Logger logger = new("MOTDAPI");
        _ = RemoteCallAPI.ExportAs("MOTDAPI", "GetFromBE", (string addr, ushort port) =>
        {
            byte[] back = new byte[256];
            try
            {
                if (!(IPAddress.TryParse(addr, out IPAddress ip) && ip != null)) // 无法直接转换的ip尝试使用dns解析
                {
                    ip = Dns.GetHostAddresses(addr)[0];
                }
                Socket socket = new(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                socket.Connect(ip, port);
                _ = socket.Send(data);
                _ = socket.Receive(back);
            }
            catch (SocketException ex)
            {
                logger.Debug.WriteLine(ex);
            }
            return Encoding.UTF8.GetString(back);
        });
    }
}
