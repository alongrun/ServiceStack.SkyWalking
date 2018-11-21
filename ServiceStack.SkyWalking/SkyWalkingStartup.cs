using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Configuration;
using SkyWalking.Boot;
using SkyWalking.Config;
using SkyWalking.Diagnostics;
using SkyWalking.Logging;
using SkyWalking.Remote;

namespace ServiceStack.SkyWalking
{
    public class SkyWalkingStartup : IDisposable
    {

        private TracingDiagnosticProcessorObserver _observer;
        public void Start()
        {
            LoadConfigurationSetting();
            LogManager.SetLoggerFactory(new DebugLoggerFactoryAdapter());
           // List<ITracingDiagnosticProcessor> ls=new List<ITracingDiagnosticProcessor>();
           // ls.Add(new HttpClientDiagnosticProcessor());
            
            
         //   _observer=new TracingDiagnosticProcessorObserver(ls);
            AsyncContext.Run(async () => await StartAsync());
        }

        private async Task StartAsync()
        {
            await GrpcConnectionManager.Instance.ConnectAsync(TimeSpan.FromSeconds(3));
            await ServiceManager.Instance.Initialize();
        }

        private void LoadConfigurationSetting()
        {
           // CollectorConfig.DirectServers = GetAppSetting(nameof(CollectorConfig.DirectServers), true);

           // AgentConfig.ApplicationCode = GetAppSetting(nameof(AgentConfig.ApplicationCode), true);
            AgentConfig.Namespace = GetAppSetting(nameof(AgentConfig.Namespace), false);
            CollectorConfig.CertificatePath = GetAppSetting(nameof(CollectorConfig.CertificatePath), false);
            CollectorConfig.Authentication = GetAppSetting(nameof(CollectorConfig.Authentication), false);
            var samplePer3Secs = GetAppSetting(nameof(AgentConfig.SamplePer3Secs), false);
            if (int.TryParse(samplePer3Secs, out var v))
            {
                AgentConfig.SamplePer3Secs = v;
            }
            var pendingSegmentsLimit = GetAppSetting(nameof(AgentConfig.PendingSegmentsLimit), false);
            if (int.TryParse(pendingSegmentsLimit, out v))
            {
                AgentConfig.PendingSegmentsLimit = v;
            }
        }

        private string GetAppSetting(string key, bool @throw)
        {
            var value = WebConfigurationManager.AppSettings[key];
            if (@throw && string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Cannot read valid '{key}' in AppSettings.");
            }

            return value;
        }

        public void Dispose()
        {
            AsyncContext.Run(async () => await GrpcConnectionManager.Instance.ShutdownAsync());
            ServiceManager.Instance.Dispose();
        }
    }
}