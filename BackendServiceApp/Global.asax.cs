using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace BackendServiceApp
{   
        public class Global : HttpApplication
        {
            void Application_Start(object sender, EventArgs e)
            {
                RegisterRoutes();
            }

            private void RegisterRoutes()
            {
                // Edit the base address of Service1 by replacing the "Service1" string below
                RouteTable.Routes.Add(new ServiceRoute("BackendService", new WebServiceHostFactory(), typeof(BackendService)));
            }

            protected void Application_BeginRequest(object sender, EventArgs e)

{

          HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");

          if (HttpContext.Current.Request.HttpMethod == "OPTIONS")

{

               HttpContext.Current.Response.AddHeader("Cache-Control", "no-cache");

               HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST");

               HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");

               HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");

               HttpContext.Current.Response.End();

              }

}

        }
}


