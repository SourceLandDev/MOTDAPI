using LLNET.Core;
using System.Net.Sockets;
using System.Text;

namespace MotdBEAPI {
    [PluginMain("MotdBEAPI")]
    public class Main : IPluginInitializer {
        public string Introduction => "MotdBEAPI";
        public Dictionary<string, string> MetaData => new();
        public Version Version => new(1, 0, 0);
        private static readonly string str = "01000000000000000000FFFF00FEFEFEFEFDFDFDFD123456780000000000000000";
        private static readonly byte[] data = new byte[str.Length / 2];
        public void OnInitialize() {
            for (int i = 0; i < str.Length; i += 2) {
                data[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
            }
            _ = LLNET.RemoteCall.DynamicRemoteCallAPI.ExportAs("MotdBE", "MotdBE", MotdBE);
        }
        public static string MotdBE(string ip, ushort port) {
            Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            byte[] back = new byte[255];
            socket.Connect(ip, port);
            _ = socket.Send(data);
            _ = socket.Receive(back);
            return Encoding.UTF8.GetString(back);
        }
    }
}
