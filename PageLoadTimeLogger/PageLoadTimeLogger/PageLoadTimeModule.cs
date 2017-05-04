
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using System.Xml;

namespace PageLoadTimeLogger
{
    public class PageLoadTimeModule : IHttpModule

    {
        //log exception
        private bool logUnhandeldExceptions;

        public void Dispose()
        {

        }

        public void Init(HttpApplication context)
        {


            //bool success = bool.TryParse(ConfigurationManager.AppSettings["LogUnhandledExceptions"], out logUnhandeldExceptions);
            //if (!success) { logUnhandeldExceptions = true; }

            //context.Error += new EventHandler(OnError);

            context.Error += new System.EventHandler(OnError);

            context.BeginRequest += OnBeginRequest;
            context.EndRequest += OnEndRequest;



        }

        public void OnError(object obj, EventArgs args)
        {
            // At this point we have information about the error

            HttpContext ctx = HttpContext.Current;
            HttpResponse response = ctx.Response;
            HttpRequest request = ctx.Request;

            Exception exception = ctx.Server.GetLastError();

            response.Write("Your request could not processed. " +
                           "Please press the back button on" +
                           " your browser and try again.<br/>");
            response.Write("If the problem persists, please " +
                           "contact technical support<p/>");
            response.Write("Information below is for " +
                           "technical support:<p/>");

            string errorInfo = "<p/>URL: " + ctx.Request.Url.ToString();
            errorInfo += "<p/>Stacktrace:---<br/>" +
               exception.InnerException.StackTrace.ToString();
            errorInfo += "<p/>Error Message:<br/>" +
               exception.InnerException.Message;

            //Write out the query string 
            response.Write("Querystring:<p/>");

            for (int i = 0; i < request.QueryString.Count; i++)
            {
                response.Write("<br/>" +
                     request.QueryString.Keys[i].ToString() + " :--" +
                     request.QueryString[i].ToString() + "--<br/>");// + nvc.
            }

            //Write out the form collection
            response.Write("<p>---------------" +
                           "----------<p/>Form:<p/>");

            for (int i = 0; i < request.Form.Count; i++)
            {
                response.Write("<br/>" +
                         request.Form.Keys[i].ToString() +
                         " :--" + request.Form[i].ToString() +
                         "--<br/>");// + nvc.
            }

            response.Write("<p>-----------------" +
                           "--------<p/>ErrorInfo:<p/>");

            response.Write(errorInfo);

            // --------------------------------------------------
            // To let the page finish running we clear the error
            // --------------------------------------------------

            ctx.Server.ClearError();
        }

        //void OnError(object sender, EventArgs e)
        // {
        //     try
        //     {
        //         if (!logUnhandeldExceptions) { return; }

        //         string userIp;
        //         string url;
        //         string exception;

        //         HttpContext context = HttpContext.Current;

        //         if (context != null)
        //         {
        //             userIp = context.Request.UserHostAddress;
        //             url = context.Request.Url.ToString();

        //             // get last exception, but check if it exists
        //             Exception lastException = context.Server.GetLastError();

        //             if (lastException != null)
        //             {
        //                 exception = lastException.ToString();
        //             }
        //             else
        //             {
        //                 exception = "no error";
        //             }
        //         }
        //         else
        //         {
        //             userIp = "no httpcontext";
        //             url = "no httpcontext";
        //             exception = "no httpcontext";
        //         }
        //         StreamWriter sw = new StreamWriter("I:\\Test_File.txt");
        //         sw.Write("Unhandled exception occured. UserIp" + userIp + ", URL: " + url + ",Exception " + exception + ".");
        //         sw.Dispose();
        //         // HttpContext.Current.Response.Write("Unhandled exception occured. UserIp" + userIp+", URL: "+ url +",Exception "+ exception+".");
        //     }
        //     catch (Exception ex)
        //     {
        //         StreamWriter sw = new StreamWriter("I:\\Test_File.txt");
        //         sw.Write("Exception occured in OnError: " + ex.ToString());
        //         sw.Dispose();
        //         //HttpContext.Current.Response.Write("Exception occured in OnError: " +ex.ToString());
        //     }
        // }



        void OnBeginRequest(object sender, System.EventArgs e)
        {
            if (HttpContext.Current.Request.IsLocal
                && HttpContext.Current.IsDebuggingEnabled)
            {
                var stopwatch = new Stopwatch();
                HttpContext.Current.Items["Stopwatch"] = stopwatch;
                stopwatch.Start();
            }
        }
      

        void OnEndRequest(object sender, System.EventArgs e)
        {
            string myExternalIP;
            string strHostName = System.Net.Dns.GetHostName();
            string clientIPAddress = System.Net.Dns.GetHostAddresses(strHostName).GetValue(0).ToString();
            string clientip = clientIPAddress.ToString();
            System.Net.HttpWebRequest request =
        (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create("http://www.whatismyip.org");
            request.UserAgent = "User-Agent: Mozilla/4.0 (compatible; MSIE" +
                "6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
            System.Net.HttpWebResponse response =
            (System.Net.HttpWebResponse)request.GetResponse();
            using (System.IO.StreamReader reader = new
            StreamReader(response.GetResponseStream()))
            {
                myExternalIP = reader.ReadToEnd();
                reader.Close();
            }


            WebRequest rssReq = WebRequest.Create("http://freegeoip.appspot.com/xml/" + myExternalIP);
            WebProxy px = new WebProxy("http://freegeoip.appspot.com/xml/" + myExternalIP, true);
            rssReq.Proxy = px;
            rssReq.Timeout = 2000;
            WebResponse rep = rssReq.GetResponse();
            XmlTextReader xtr = new XmlTextReader(rep.GetResponseStream());
            // If we got an IPV6 address, then we need to ask the network for the IPV4 address 
            // This usually only happens when the browser is on the same machine as the server.
            //if (ipAddressString.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            //{
            //    ipAddressString = Dns.GetHostEntry(UserIP).AddressList
            //        .First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);





            //    url = ipAddressString.ToString();
            //}


           //HttpContext.Current.Session["UserCountryCode"] = dynObj.ToString();
            HttpContext.Current.Response.Write(xtr.ToString());
         HttpContext.Current.Response.Write(myExternalIP + "HERE IS IP ADDRESS");
            if (HttpContext.Current.Request.IsLocal
                && HttpContext.Current.IsDebuggingEnabled)
            {

                Stopwatch stopwatch =
                  (Stopwatch)HttpContext.Current.Items["Stopwatch"];
                stopwatch.Stop();

                TimeSpan ts = stopwatch.Elapsed;

                string elapsedTime = String.Format("{0}", ts.TotalMilliseconds);
                long ti = stopwatch.ElapsedMilliseconds;
                int tc = (int)ti;
                string requestInfo = HttpContext.Current.Request.Path.ToString();
                try
                {
                    // !!!!Set up your own database
                    SqlConnection conn = new SqlConnection("Data Source=.\\;Initial Catalog=;Integrated Security=True");
                    HttpContext.Current.Response.Write("<p>" + elapsedTime + "</p>");
                   // SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
                    conn.Open();
                    String strI = "insert into [dbo].[PLT2] (PageLoadTime,Path,Time) Values (@a1,@a2,@a3) ";
                    SqlCommand ip = new SqlCommand(strI, conn);
                    ip.Parameters.Add(new SqlParameter("@a1", tc));
                    ip.Parameters.Add(new SqlParameter("@a2", requestInfo));
                    ip.Parameters.Add(new SqlParameter("@a3", DateTime.Now));



                    ip.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception c)
                {
                    HttpContext.Current.Response.Write(c);
                }
            }
        }

    }
}