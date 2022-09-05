using System.Net.Sockets;

namespace Net
{
    public class ClientState
    {
        public Socket socket;
        public byte[] readBuff = new byte[1024];

        public int hp = -100;
        public float x = 0;
        public float y = 0;
        public float z = 0;
        public float eulY = 0;
    }
}