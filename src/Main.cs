using System.Net;
using System.Net.Sockets;
using System.Text;
using LiteLoader.NET;
using LiteLoader.RemoteCall;
using LiteLoader.Schedule;

namespace MOTDAPI;
[PluginMain("MOTDAPI")]
public class Main : IPluginInitializer
{
    public string Introduction => "MOTDAPI";
    public Dictionary<string, string> MetaData => new();
    public Version Version => new();
    private static readonly byte[] s_bedrockEditionUnconnectedPong = { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 0, 254, 254, 254, 254, 253, 253, 253, 253, 18, 52, 86, 120, 0, 0, 0, 0, 0, 0, 0, 0 };
    private static readonly byte[] s_javaEditionStatusRequest = { 6, 0, 0, 0, 0x63, 0xdd, 1, 1, 0 };
    public void OnInitialize()
    {
        RemoteCallAPI.ExportFunc("MOTDAPI", "GetFromBE", (List<Valuetype> args) => GetFromBedrockEdition(args[0], args[1], args[2]));
        RemoteCallAPI.ExportFunc("MOTDAPI", "GetFromBEAsync", (List<Valuetype> args) =>
        {
            Task<string> task = Task.Run(() => GetFromBedrockEdition(args[0], args[1], args[4]));
            task.ContinueWith((task) =>
            {
                if (!RemoteCallAPI.HasFunc(args[2], args[3]))
                {
                    return;
                }
                ScheduleAPI.NextTick(() =>
                    RemoteCallAPI.ImportFunc(args[2], args[3])(new() { task.Id, task.Result }));
            });
            return task.Id;
        });
        RemoteCallAPI.ExportFunc("MOTDAPI", "GetFromJE", (List<Valuetype> args) => GetFromJavaEdition(args[0], args[1], args[2]));
        RemoteCallAPI.ExportFunc("MOTDAPI", "GetFromJE", (List<Valuetype> args) =>
        {
            Task<string> task = Task.Run(() => GetFromJavaEdition(args[0], args[1], args[4]));
            task.ContinueWith((task) =>
            {
                if (!RemoteCallAPI.HasFunc(args[2], args[3]))
                {
                    return;
                }
                ScheduleAPI.NextTick(() =>
                    RemoteCallAPI.ImportFunc(args[2], args[3])(new() { task.Id, task.Result }));
            });
            return task.Id;
        });
    }

    private static IPAddress GetIP(string address)
    {
        if (!IPAddress.TryParse(address, out IPAddress? ip))
        {
            ip = Dns.GetHostAddresses(address)[0];
        }
        return ip;
    }

    private static string GetFromBedrockEdition(string address, ushort port, int timeout = 0)
    {
        byte[] back = new byte[1024];
        int length = 0;
        try
        {
            IPAddress ip = GetIP(address);
            Socket socket = new(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(ip, port);
            socket.Send(s_bedrockEditionUnconnectedPong);
            Task<int> task = socket.ReceiveAsync(back);
            if (timeout > 0)
            {
                if (!task.Wait(timeout))
                {
                    return string.Empty;
                }
            }
            else
            {
                task.Wait();
            }
            length = task.Result;
        }
        catch (SocketException)
        {
        }
        return length > 35 ?
               Encoding.UTF8.GetString(back, 35, length - 35) :
               Encoding.UTF8.GetString(back);
    }

    private static string GetFromJavaEdition(string address, ushort port, int timeout = 0)
    {
        byte[] back = new byte[1024 * 8];
        int length = 0;
        try
        {
            IPAddress ip = GetIP(address);
            Socket socket = new(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ip, port);
            socket.Send(s_javaEditionStatusRequest);
            Task<int> task = socket.ReceiveAsync(back);
            if (timeout > 0)
            {
                if (!task.Wait(timeout))
                {
                    return string.Empty;
                }
            }
            else
            {
                task.Wait();
            }
            length = task.Result;
            socket.Close();
        }
        catch (SocketException)
        {
        }
        return length > 5 ?
            Encoding.UTF8.GetString(back, 5, length - 5) :
            Encoding.UTF8.GetString(back);
    }
}
