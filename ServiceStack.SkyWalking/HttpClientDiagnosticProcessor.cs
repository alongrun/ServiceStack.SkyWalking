using System;
using System.Net.Http;
using SkyWalking.Context;
using SkyWalking.Context.Tag;
using SkyWalking.Context.Trace;
using SkyWalking.Diagnostics;
using SkyWalking.NetworkProtocol.Trace;

namespace ServiceStack.SkyWalking
{
    public class HttpClientDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = "HttpHandlerDiagnosticListener";

        [DiagnosticName("System.Net.Http.Request")]
        public void HttpRequest([Property(Name = "Request")] HttpRequestMessage request)
        {
            var contextCarrier = new ContextCarrier();
            var peer = $"{request.RequestUri.Host}:{request.RequestUri.Port}";
            var span = ContextManager.CreateExitSpan(request.RequestUri.ToString(), contextCarrier, peer);
            Tags.Url.Set(span, request.RequestUri.ToString());
            span.AsHttp();
            span.SetComponent(ComponentsDefine.HttpClient);
            Tags.HTTP.Method.Set(span, request.Method.ToString());
            foreach (var item in contextCarrier.Items)
                request.Headers.Add(item.HeadKey, item.HeadValue);
           
        }

        [DiagnosticName("System.Net.Http.Response")]
        public void HttpResponse([Property(Name = "Response")] HttpResponseMessage response)
        {
            var span = ContextManager.ActiveSpan;
            if (span != null && response != null)
            {
                Tags.StatusCode.Set(span, response.StatusCode.ToString());
            }

            ContextManager.StopSpan(span);
        }

        [DiagnosticName("System.Net.Http.Exception")]
        public void HttpException([Property(Name = "Request")] HttpRequestMessage request,
            [Property(Name = "Exception")] Exception ex)
        {
            var span = ContextManager.ActiveSpan;
            if (span != null && span.IsExit)
            {
                span.ErrorOccurred();
            }
        }
    }
}