using System;
using System.Net;
using SkyWalking.Context;
using SkyWalking.Context.Tag;
using SkyWalking.Context.Trace;
using SkyWalking.NetworkProtocol.Trace;

namespace ServiceStack.SkyWalking
{
    internal class ServiceStackClientDiagnosticProcessor
    {
        public UrlTraceFilter Filter { get; set; }

        public void OnBegRequest(HttpWebRequest request)
        {

            var trace = Filter;
            if (trace != null)
            {
                if (!trace.IsTrace(request.RequestUri.ToString()))
                    return;

            }
           var contextCarrier = new ContextCarrier();
                      var peer = $"{request.RequestUri.Host}:{request.RequestUri.Port}";
            try
            {
                var span = ContextManager.CreateExitSpan(request.RequestUri.ToString(), contextCarrier, peer);
                Tags.Url.Set(span, request.RequestUri.ToString());
                span.AsHttp();
                span.SetComponent(ComponentsDefine.HttpClient);
                Tags.HTTP.Method.Set(span, request.Method.ToString());
                foreach (var item in contextCarrier.Items)
                    request.Headers.Add(item.HeadKey, item.HeadValue);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void OnEndRequest(HttpWebResponse response)
        {
            var span = ContextManager.ActiveSpan;
            if (span != null && response != null)
            {
                Tags.StatusCode.Set(span, response.StatusCode.ToString());
            }

            ContextManager.StopSpan(span);
        }
    }
}