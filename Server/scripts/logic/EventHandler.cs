using System;
using Server;
using Net;

public class EventHandler
{
    public static void OnDisconnect(ClientState c)
    {
        Console.WriteLine("OnDisConnect");
    }
}