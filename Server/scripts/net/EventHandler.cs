using System;
using EchoServer;

public class EventHandler
{
    public static void OnDisconnect(ClientState c)
    {
        Console.WriteLine("OnDisConnect");
    }
}