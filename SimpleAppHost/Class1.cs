using System;
using System.Collections.Generic;
using System.Text;

using System.Web;
using System.Web.Hosting;
using System.Collections.Specialized;

using System.Net;

namespace SimpleAppHost
{
    public class SimpleHost : MarshalByRefObject
    {
        //public string ProcessRequest(string page, NameValueCollection queryString )
        public string ProcessRequest(string page, string query)
        {
            // Process the request and send back the string output
            using (System.IO.StringWriter sw = new System.IO.StringWriter())
            {
                HttpRuntime.ProcessRequest(new SimpleWorkerRequest(page, query, sw));
                return sw.ToString();
            }
        }


        public static SimpleHost InitialiseHost()
        {
            SimpleHost theHost = (SimpleHost)ApplicationHost.CreateApplicationHost(typeof(SimpleHost), "/", @"c:\HttpListenerWebHost");
            return theHost;
        }
    }
}
