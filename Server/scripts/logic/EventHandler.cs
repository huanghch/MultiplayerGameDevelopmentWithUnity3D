using System;
using Proto;

public class EventHandler
{
    public static void OnDisconnect(ClientState c)
    {
        Console.WriteLine("OnDisConnect");
        //Player下线
        if (c.player != null)
        {
            //保存数据
            DbManager.UpdatePlayerData(c.player.id, c.player.data);
            //移除
            PlayerManager.RemovePlayer(c.player.id);
        }
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