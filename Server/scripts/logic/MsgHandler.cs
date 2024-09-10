using System;
using Proto;

public partial class MsgHandler
{
    public static void MsgMove(ClientState c, MsgBase msgBase)
    {
        MsgMove msgMove = (MsgMove) msgBase;
        Console.WriteLine(msgMove.x);
        Console.WriteLine(msgMove.y);
        Console.WriteLine(msgMove.z);
        msgMove.x++;
        msgMove.y++;
        msgMove.z++;
        NetManager.Send(c, msgMove);
    }
}