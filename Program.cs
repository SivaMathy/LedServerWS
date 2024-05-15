using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;
using ConsoleApp1.Model;
using Newtonsoft.Json;
using ConsoleApp1.Controller;

namespace ConsoleApp1

{
    internal class Program
    {
        static void Main(string[] args)
        {
            WebSocketServer webSocketServer = new WebSocketServer("ws://localhost:7890");

            webSocketServer.AddWebSocketService<LedControl>("/ledcontrol");
            webSocketServer.AddWebSocketService<LoginController>("/login");
            webSocketServer.Start();
            Console.WriteLine("Server started on 7890");
            Console.ReadKey();
            webSocketServer.Stop();
        }
    }
}
