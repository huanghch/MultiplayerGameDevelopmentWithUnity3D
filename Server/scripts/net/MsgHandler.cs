using System;
using System.Text;
using EchoServer;

public class MsgHandler
{
    public static void MsgEnter(ClientState c, string msgArgs)
    {
        Console.WriteLine("MsgEnter: "+msgArgs);
        
        // 解析参数
        string[] split = msgArgs.Split(',');
        string desc = split[0];
        float x = float.Parse(split[1]);
        float y = float.Parse(split[2]);
        float z = float.Parse(split[3]);
        float eulY = float.Parse(split[4]);
        
        // 赋值
        c.hp = 100;
        c.x = x;
        c.y = y;
        c.z = z;
        c.eulY = eulY;
        
        // 广播
        string sendStr = "Enter|" + msgArgs;
        foreach (ClientState cs in MainClass.clients.Values)
        {
            //MainClass.Send(cs, sendStr);
        }
    }

    public static void MsgList(ClientState c, string msgArgs)
    {
        Console.WriteLine("MsgList: "+msgArgs);

        StringBuilder sendStr = new StringBuilder("List|");
        foreach (ClientState cs in MainClass.clients.Values)
        {
            
            sendStr.Append(cs.socket.RemoteEndPoint);
            sendStr.Append(',');
            sendStr.Append(cs.x);
            sendStr.Append(',');
            sendStr.Append(cs.y);
            sendStr.Append(',');
            sendStr.Append(cs.z);
            sendStr.Append(',');
            sendStr.Append(cs.eulY);
            sendStr.Append(',');
            sendStr.Append(cs.hp);
            sendStr.Append(',');
        }
        //MainClass.Send(c, sendStr.ToString());
    }
}