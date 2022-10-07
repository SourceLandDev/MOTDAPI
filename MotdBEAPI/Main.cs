using LLNET.Core;
using System.Net.Sockets;
using System.Text;

namespace MOTDBEAPI {
    [PluginMain("MOTDBEAPI")]
    public class Main : IPluginInitializer {
        public string Introduction => "MOTDBEAPI";
        public Dictionary<string, string> MetaData => new();
        public Version Version => new(1, 0, 0);
        private static readonly byte[] data = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 0, 254, 254, 254, 254, 253, 253, 253, 253, 18, 52, 86, 120, 0, 0, 0, 0, 0, 0, 0, 0 };
        public void OnInitialize() {
            LLNET.RemoteCall.RemoteCallAPI.ExportAs("MOTD", "GetBEMOTD", (string ip, ushort port) => {
                Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                byte[] back = new byte[256];
                try {
                    socket.Connect(ip, port);
                    socket.Send(data);
                    socket.Receive(back);
                } catch { }
                return Encoding.UTF8.GetString(back);
            });
        }
    }
}
