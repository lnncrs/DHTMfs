using DHTMfs.Data;
using DHTMfs.Models;
using DHTMfs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace DHTMfs.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NodesController : ControllerBase
    {
        private readonly ILogger<NodesController> _logger;
        private readonly NodeService _nodeService;

        public NodesController(ILogger<NodesController> logger, NodeService nodeService)
        {
            _logger = logger;
            _nodeService = nodeService;
        }

        [HttpGet("list", Name = "GetNodes")]
        public ActionResult<List<Node>> Get()
        {
            return _nodeService.GetNodes();
        }
    }
}