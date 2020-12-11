using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using CloudNative.CloudEvents;
using System.Threading.Tasks;

namespace TestHttpListener
{
   class Program
   {
      private static HttpListener _listener;
      static void Main(string[] args) {
         const string listenerAddress = "http://localhost:52672/";
         _listener = new HttpListener() {
            AuthenticationSchemes = AuthenticationSchemes.Anonymous,
            Prefixes = { listenerAddress }
         };
         _listener.Start();
         _listener.GetContextAsync().ContinueWith(async t =>
         {
            if (t.IsCompleted) {
               await HandleContext(t.Result);
            }
         }).Wait();

         _listener.Stop();
         Console.ReadLine();
      }

      static async Task HandleContext(HttpListenerContext requestContext) {

         if (requestContext.Request.IsCloudEvent()) {
            var receivedCloudEvent = requestContext.Request.ToCloudEvent(new JsonEventFormatter());
            Console.WriteLine("Cloud Event");
            Console.WriteLine($"  Source: {receivedCloudEvent.Source}");
            Console.WriteLine($"  Subject: {receivedCloudEvent.Subject}");
            Console.WriteLine($"  Id: {receivedCloudEvent.Id}");
            Console.WriteLine($"  Data: {receivedCloudEvent.Data}");

            //HttpListenerResponse response = requestContext.Response;
            // Construct a response.
            await requestContext.Response.CopyFromAsync(receivedCloudEvent, ContentMode.Binary, new JsonEventFormatter());
            requestContext.Response.StatusCode = (int)HttpStatusCode.OK;
            requestContext.Response.Close();
         }

      }
   }
}

