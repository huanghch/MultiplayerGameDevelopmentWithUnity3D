namespace EchoServer;

public class MsgHandler
{
    public static void MsgEnter(ClientState c, string msgArgs)
    {
        Console.WriteLine("MsgEnter: "+msgArgs);
    }

    public static void MsgList(ClientState c, string msgArgs)
    {
        Console.WriteLine("MsgList: "+msgArgs);
    }
}