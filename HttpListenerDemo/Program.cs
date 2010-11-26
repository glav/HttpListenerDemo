using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Threading;
using System.Diagnostics;

using System.Web;
using System.Web.Hosting;

namespace HttpListenerDemo
{
    class Program
    {
        #region Private fields

        const int PORT_NUMBER = 9999;

        private static bool _keepAppRunning = true;
        private static HttpListener _listener;
        private static Thread _listeningThread = null;

        private static SimpleAppHost.SimpleHost _host = null;

        const string WEBPATH = @"C:\Documents and Settings\Administrator\My Documents\Visual Studio 2005\Projects\TechEdDemos\HttpListenerDemo\HttpListenerDemo\SomeStaticFiles";

        #endregion

        static void Main(string[] args)
        {
            // Create our new appDomain host
            Console.WriteLine("Starting the new AppDomain to host our ASP.NET runtime system...");
            _host = SimpleAppHost.SimpleHost.InitialiseHost();    
            
            Console.WriteLine("Starting the HttpListener on port:{0}", PORT_NUMBER);

            InitialiseListener();

            _listeningThread = new Thread(ListeningThread);
            _listeningThread.Start();

            Console.WriteLine("Listener started on port: {0}, waiting for requests, press ANY KEY to quit.", PORT_NUMBER);

            #region Keep the console open until a key is pressed or the browser user asks for it
            // Quit listening if the user presses a key from the console window
            // or if the _keepAppRunning flag is false (which is set by a
            //  URL argument)
            while (_keepAppRunning)
            {
                if (Console.KeyAvailable)
                    _keepAppRunning = false;
            }

            // Close down our thread and listener if required.
            if (_listeningThread.ThreadState == System.Threading.ThreadState.Running)
                _listeningThread.Abort();
            
            #endregion

            if (_listener.IsListening)
                _listener.Stop();
        }

        #region InitialiseListener

        /// <summary>
        /// Initialise our http listener 
        /// </summary>
        private static void InitialiseListener()
        {
            _listener = new HttpListener();

            _listener.AuthenticationSchemes = AuthenticationSchemes.Basic;
            
            string prefix = string.Format("http://+:{0}/", PORT_NUMBER);
            _listener.Prefixes.Add(prefix);
            
            _listener.Start();
        }

        #endregion

        #region Main ListeningThread method

        private static void ListeningThread()
        {
            while (_keepAppRunning)
            {
                HttpListenerContext context = _listener.GetContext();

                #region Stopwatch start & console output

                Console.WriteLine("---Request Recieved----");

                // Yet another new V2 feature
                Stopwatch sw = new Stopwatch();
                sw.Reset();
                sw.Start();

                #endregion

                try
                {
                    WriteRequestHeaderInformation(context);

                    CreateResponseDocument(context);
                }
                catch (Exception ex)
                {
                    #region Display any exception details
                    ConsoleColor oldCol = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("\nThere was an error processing this request. See below for detals :-");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("\n");
                    Console.ForegroundColor = oldCol;
                    #endregion
                }

                Console.WriteLine("----Request processed in {0} milliseconds ----",sw.ElapsedMilliseconds);
            }
        }

        #endregion

        #region Create a simple response document to send to the browser

        private static void CreateResponseDocument(HttpListenerContext ctxt)
        {
            #region Check if its a file request and setup variables/flags

            bool isFileToProcess = true;
            string htmlOutput = null;

            // Check if its a .HTML or .HTM file
            if (ctxt.Request.Url.PathAndQuery.ToUpper().IndexOf(".DUMMY") >= 0)
            {
                isFileToProcess = false;
            }

            #endregion

            // Check if the user has request a html file, if so, serve it up,
            // otherwise, give them our simple response.
            if ( isFileToProcess)
            {
                // Get our host to service the file request - going cross domain here
                htmlOutput = _host.ProcessRequest(ctxt.Request.Url.LocalPath.Replace("/",""),ctxt.Request.Url.Query.Replace("?",""));
            }
            else
            {
                // There was no file request, so we just return a simple
                // response document.
                htmlOutput = string.Format("<h1>Glavs SuperDuper Web Server - Baldy Edition</h1><p>You have hit the HttpListener demo at {0}</p>", DateTime.Now.ToString("hh:mm:ss"));
            }

            // Send the output, if any, to the response context for the listener
            if (htmlOutput != null && ctxt.Response.OutputStream.CanWrite)
            {
                byte[] htmlData = System.Text.UTF8Encoding.UTF8.GetBytes(htmlOutput);
                ctxt.Response.OutputStream.Write(htmlData, 0, htmlData.Length);
            }

            // Have we been asked to shutdown the server?
            if (ctxt.Request.QueryString.Count > 0 && ctxt.Request.QueryString["quit"] != null && ctxt.Request.QueryString["quit"].ToUpper() == "TRUE")
            {
                _keepAppRunning = false;
                byte[] finalOutput = Encoding.UTF8.GetBytes("<h2>The Server has been shutdown!</h2>");
                ctxt.Response.OutputStream.Write(finalOutput, 0, finalOutput.Length);
            }

            ctxt.Response.Close();
        }

        #endregion

        #region Write Out some header information

        private static void WriteRequestHeaderInformation(HttpListenerContext ctxt)
        {
            Console.WriteLine("Header Information:");
            ConsoleColor oldColour = Console.ForegroundColor;

            // Show all the headers in our request context
            foreach (string key in ctxt.Request.Headers.AllKeys)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                
                Console.Write("[{0}]", key);
                
                Console.CursorLeft = 25;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" : ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                
                Console.WriteLine("[{0}]", ctxt.Request.Headers[key]);
            }
            Console.WriteLine("--");

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("User Information");
            Console.CursorLeft = 25;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(" : ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.CursorLeft = 28;
            
            // Show the user information
            if (ctxt.User != null)
            {
                Console.WriteLine("Type = [{0}]", ctxt.User);
                Console.CursorLeft = 28;
                Console.WriteLine("Name = [{0}]", ctxt.User.Identity.Name);
            }
            else
            {
                Console.WriteLine("No User defined");
            }

            Console.ForegroundColor = oldColour;
        }

        #endregion
    }
}
