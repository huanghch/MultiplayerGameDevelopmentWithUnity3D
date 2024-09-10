using System;
using Proto;

public class EventHandler
{
    public static void OnDisconnect(ClientState c)
    {
        Console.WriteLine("OnDisConnect");
    }

    public static void OnTimer()
    {
        CheckPing();
    }

    public static void CheckPing()
    {
        // 现在的时间戳
        long currentTime = NetManager.GetTimeStamp();
        // 遍历，删除
        foreach(ClientState s in NetManager.clients.Values)
        {
            if(currentTime - s.lastPingTime > NetManager.pingInterval * 4)
            {
                Console.WriteLine("Ping Close " + s.socket.RemoteEndPoint.ToString());
                NetManager.Close(s);
            }
        }
        return;
    }
}