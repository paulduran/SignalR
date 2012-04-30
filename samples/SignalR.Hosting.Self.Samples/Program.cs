﻿using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web.Http.SelfHost;
using SignalR.Hosting.WebApi;
using SignalR.Samples.Raw;

namespace SignalR.Hosting.Self.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Debug.Listeners.Add(new ConsoleTraceListener());
            Debug.AutoFlush = true;

            //DefaultSelfHost();

            WebApiSelfHost();

            Console.ReadKey();
        }

        private static void WebApiSelfHost()
        {
            var config = new HttpSelfHostConfiguration("http://localhost:8081");
            config.TransferMode = TransferMode.StreamedResponse;
            config.Routes.MapConnection<MyConnection>("Echo", "echo/{*operation}");
            config.Routes.MapConnection<Raw>("Raw", "raw/{*operation}");
            config.Routes.MapHubs();

            var dispatcher = new HubDispatcherMessageHandler(config)
            {
                InnerHandler = new PersistentConnectionDispatcher(config)
            };

            var server = new HttpSelfHostServer(config, dispatcher);
            server.OpenAsync().Wait();
        }

        private static void DefaultSelfHost()
        {
            string url = "http://*:8081/";
            var server = new Server(url);

            // Map connections
            server.MapConnection<MyConnection>("/echo")
                  .MapConnection<Raw>("/raw")
                  .MapHubs();

            server.Start();

            Console.WriteLine("Server running on {0}", url);
        }

        public class MyConnection : PersistentConnection
        {
            protected override Task OnConnectedAsync(IRequest request, string connectionId)
            {
                return Connection.Broadcast(String.Format("{0} connected from {1}", connectionId, request.Headers["User-Agent"]));
            }

            protected override Task OnReceivedAsync(string connectionId, string data)
            {
                return Connection.Broadcast(data);
            }
        }
    }
}
