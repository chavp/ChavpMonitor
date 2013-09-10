using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace ChavpMonitorWeb
{
    public class MachineHub : Hub
    {
        public void Update()
        {
            Clients.All.update("Hello");
        }
    }
}