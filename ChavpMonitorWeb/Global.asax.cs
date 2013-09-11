using Chavp.Monitors;
using EasyNetQ;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ChavpMonitorWeb
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    // http://www.asp.net/signalr
    public class MvcApplication : System.Web.HttpApplication
    {
        string rabbitMQBrokerHost = "localhost";
        string virtualHost = "machines";
        string username = "guest";
        string password = "guest";

        IAdvancedBus _bus;
        private IHubContext _hubContext;

        protected void Application_Start()
        {
            RouteTable.Routes.MapHubs();
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            string connectionString = string.Format(
                "host={0};virtualHost={1};username={2};password={3}",
                rabbitMQBrokerHost, virtualHost, username, password);

            _bus = RabbitHutch.CreateBus(connectionString).Advanced;

            var exchange = _bus.ExchangeDeclare("machine", EasyNetQ.Topology.ExchangeType.Fanout);

            var queue = _bus.QueueDeclare(
                "machine.info",
                durable: false,
                exclusive: false,
                autoDelete: true);

            _bus.Bind(exchange, queue, "info");

            _hubContext = GlobalHost.ConnectionManager.GetHubContext<MachineHub>();

            _bus.Consume<MachineInfo>(queue, (body, info) => Task.Factory.StartNew(() =>
            {
                var machine = body.Body;

                _hubContext.Clients.All.update(machine);
            }));
        }

        protected void Application_End()
        {
            if (_bus != null)
            {
                _bus.Dispose();
            }
        }
    }
}