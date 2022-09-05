using System.Net.Sockets;

namespace Net
{
    public class ClientState
    {
        public Socket socket;
        public ByteArray readBuff = new ByteArray();
    }
}