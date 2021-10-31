using ApiModels.Requests;
using MeowthCoreServer.Models;
using MeowthCoreServer.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeowthCoreServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ListenersController : ControllerBase
    {
        private readonly IListenerService _listeners;
        private readonly IAgentService _agentService;
        public ListenersController(IListenerService listeners,IAgentService agentService)
        {
            _listeners = listeners;
            _agentService = agentService;
        }
        [HttpGet]
        public IActionResult GetListeners()
        {
            var listeners = _listeners.GetListeners();
            return Ok(listeners);
        }

        [HttpGet("{name}")]
        public IActionResult GetListener(string name)
        {
            var listener = _listeners.GetListener(name);
            if (listener is null) return NotFound();
            return Ok(listener);
        }
        [HttpPost]
        public IActionResult StartListener([FromBody] StartHttpListenerRequest request)
        {
            var listener = new HttpListener(request.Name, request.BindPort);
            listener.Init(_agentService);
            listener.Start();
            _listeners.AddListener(listener);

            var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
            var path = $"{root}/{listener.Name}";
            return Created(path,listener);
        }
        [HttpDelete("{name}")] 
        public IActionResult StopListener(string name)
        {
            var listener = _listeners.GetListener(name);
            if (listener is null) return NotFound();
            listener.Stop();

            _listeners.RemoveListener(listener);
            return NoContent();
        }
    }
}
