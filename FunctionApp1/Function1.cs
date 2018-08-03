using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Amqp;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // Log what is actually loaded
            AppDomain.CurrentDomain.AssemblyLoad += (object sender, AssemblyLoadEventArgs args) =>
            {
                log.Warning(args.LoadedAssembly.FullName);
            };

            // Assembly.LoadFile(@"D:\home\site\wwwroot\bin\Amqp.Net.dll");

            return await ForceLoadingAmqpLibrary(req, log);
        }

        private static async Task<HttpResponseMessage> ForceLoadingAmqpLibrary(HttpRequestMessage req, TraceWriter log)
        {
            // Force loading of Amqp assembly.
            var x = new ClassLibrary1.ComponentUsingAmqp();
            x.message = new Message();
            x.message.ToString();

            // From default template ------------------------------------------------
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string name = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
                .Value;

            if (name == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                name = data?.name;
            }

            return name == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
        }
    }
}
