using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MeowthCoreServer.Services;
using ApiModels.Requests;
using MeowthCoreServer.Models;

namespace MeowthCoreServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AgentsController : ControllerBase
    {
        private readonly IAgentService _agents;
        public AgentsController(IAgentService agents)
        {
            _agents = agents;
        }
        [HttpGet]
        public IActionResult GetAgents()
        {
            var agents = _agents.GetAgents();
            return Ok(agents);
        }

        [HttpGet("{agentId}")]
        public IActionResult GetAgent(string agentId)
        {
            var agent = _agents.GetAgent(agentId);
            if (agent is null) return NotFound();
            return Ok(agent);
        }

        [HttpGet("{agentId}/tasks/{taskId}")]
        public IActionResult GetTaskResult(string agentId,string taskId)
        {
            var agent = _agents.GetAgent(agentId);
            if (agent is null) return NotFound("Agent not found");

            var result = agent.GetTaskResult(taskId);
            if (result is null) return NotFound("Task not found");
            return Ok(result);
        }



        [HttpGet("{agentId}/tasks")]
        public IActionResult GetTaskResults(string agentId)
        {
            var agent = _agents.GetAgent(agentId);
            if (agent is null) return NotFound("Agent not found");

            var results = agent.GetTaskResults();
            return Ok(results);
        }



        [HttpPost("{agentId}")]
        public IActionResult TaskAgent(string agentId,[FromBody]TaskAgentRequest request)
        {
            var agent = _agents.GetAgent(agentId);
            if (agent is null) return NotFound();

            var task = new AgentTask()
            {
                Id = Guid.NewGuid().ToString(),
                Command = request.Command,
                Args = request.Args,
                File = request.File
            };
            agent.QueueTask(task);
            var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
            var path = $"{root}/tasks/{task.Id}";
            return Created(path, task);
        }
    }
}
