using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Funq;
using ServiceStack.Configuration;

using ServiceStack.Web;
using SkyWalking.Config;
using SkyWalking.Context;
using SkyWalking.Context.Tag;
using SkyWalking.Context.Trace;
using SkyWalking.Logging;
using SkyWalking.NetworkProtocol.Trace;

namespace ServiceStack.SkyWalking
{
    public  static class HttpContextDiagnosticStrings
    {
        public const string SpanKey = "sw3-http";
    }
    public class SkyWalkingFeature : IPlugin
    {
        private SkyWalkingStartup _skyWalkingStartup;

        private ServiceStackClientDiagnosticProcessor _processor;

        public SkyWalkingFeature(string appCode,string server)

        {
            CollectorConfig.DirectServers = server;
            AgentConfig.ApplicationCode = appCode;
        }

        public void Register(IAppHost appHost)
        {

            var traceFilter = new UrlTraceFilter();

            appHost.GlobalRequestFilters.Add(RequestFilters);
            appHost.OnEndRequestCallbacks.Add(EndRequest);
            _processor   =new ServiceStackClientDiagnosticProcessor(){Filter = traceFilter} ;
            JsonServiceClient.GlobalRequestFilter += _processor.OnBegRequest;
            JsonServiceClient.GlobalResponseFilter += _processor.OnEndRequest;
            
            appHost.Register(traceFilter);
            _skyWalkingStartup = new SkyWalkingStartup();
            _skyWalkingStartup.Start();
        }

        public void Dispose()
        {
            _skyWalkingStartup.Dispose();
        }

        private void EndRequest(IRequest request)
        {

          

            var response = request.Response; 
                var httpRequestSpan = ContextManager.ActiveSpan;
                if (httpRequestSpan == null)
                {
                    return;
                }

           

                var statusCode = response.StatusCode;
                if (statusCode >= 400)
                {
                    httpRequestSpan.ErrorOccurred();
                }

                Tags.StatusCode.Set(httpRequestSpan, statusCode.ToString());
             //   var resp = (HttpListenerResponse) response.OriginalResponse;
                if (response.IsErrorResponse())
                {

                    Exception exception =response as Exception;
                    if (exception != null)
                    {
                        httpRequestSpan.ErrorOccurred().Log(exception);
                    }
                }

                httpRequestSpan.Log(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    new Dictionary<string, object>
                    {
                        {"event", "ServiceStack Hosting EndRequest"},
                        {
                            "message",
                            $"Request finished {response.StatusCode} {response.ContentType}"
                        }
                    });
                ContextManager.StopSpan(httpRequestSpan);
            }

            private  void RequestFilters(IRequest request, IResponse response, object dto)
        {
            HttpListenerRequest req = (HttpListenerRequest) request.OriginalRequest;
            var trace = request.TryResolve<UrlTraceFilter>();
            if (trace != null)
            {
                if (!trace.IsTrace(request.AbsoluteUri))
                    return;

            }

            //var req = request;
         /*   if (dto == null)
            {
                return;
            }*/
            var carrier = new ContextCarrier();
            //ContextManager.Extract(carrier);
            foreach (var item in carrier.Items)
                    item.HeadValue = request.Headers[item.HeadKey];
            
           
            
           

            var httpRequestSpan =
                ContextManager.CreateEntrySpan($"{AgentConfig.ApplicationCode} {request.AbsoluteUri}",
                    carrier);
            httpRequestSpan.AsHttp();
            
          
            httpRequestSpan.SetComponent(ComponentsDefine.AspNetCore);
            Tags.Url.Set(httpRequestSpan, request.RawUrl);
            Tags.HTTP.Method.Set(httpRequestSpan, request.GetHttpMethodOverride());
           httpRequestSpan.Log(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                new Dictionary<string, object>
                {
                    {"event", "ServiceStack BeginRequest"},
                    {
                        "message",
                        $"Request starting {req.Url.Scheme} {req.HttpMethod} {req.Url.OriginalString}"
                    }
                });


           // carrier = new ContextCarrier();
           /* if (carrier.IsValid)
            {
                var exitSpan =
                    ContextManager.CreateExitSpan($"{AgentConfig.ApplicationCode} {request.AbsoluteUri}",
                        carrier,carrier.PeerHost);

                //  ContextManager.Inject(carrier);

                foreach (var item in carrier.Items)
                    request.Headers[item.HeadKey] = item.HeadKey;
            }*/


        }
    }
}