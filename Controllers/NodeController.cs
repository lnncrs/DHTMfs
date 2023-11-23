using DHTMfs.Data;
using DHTMfs.Models;
using DHTMfs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DHTMfs.Controllers
{
    [ApiController]
    [Route("node")]
    public class NodeController(ILogger<NodeController> logger, NodeService nodeService, FileService fileService) : ControllerBase
    {
        private readonly ILogger<NodeController> _logger = logger;
        private readonly NodeService _nodeService = nodeService;
        private readonly FileService _fileService = fileService;

        [HttpGet("ping", Name = "GetPing")]
        public IActionResult GetPing()
        {
            _logger.LogInformation($"node:ping");

            return Ok(DateTime.UtcNow);
        }

        [HttpGet("status", Name = "GetStatus")]
        public IActionResult GetStatus()
        {
            _logger.LogInformation($"node:status");

            _nodeService.UpdateLocalNode();

            var status = new
            {
                TimeStamp = DateTime.UtcNow,
                Node = _nodeService.GetLocalNode(),
                NodeCount = _nodeService.GetNodes().Count(),
                FileCount = _fileService.GetFileCount(),
                CacheDirectory = _fileService.GetDirectoryPath()
            };

            return Ok(status);
        }

        [HttpGet("list", Name = "GetNodes")]
        public ActionResult<List<Node>> Get()
        {
            _logger.LogInformation($"node:list");

            var nodes = _nodeService.GetNodes();
            return Ok(nodes);
        }

        [HttpGet("add", Name = "AddNode")]
        public IActionResult AddNode(string host, int port)
        {
            _logger.LogInformation($"node:add");

            return Ok(_nodeService.AddNode(host, port));
        }

        [HttpGet("remove", Name = "RemoveNode")]
        public IActionResult RemoveNode(string hash)
        {
            _logger.LogInformation($"node:remove");

            return Ok(_nodeService.RemoveNode(hash));
        }

        [HttpGet("sync", Name = "SyncNodes")]
        public IActionResult SyncNodes()
        {
            _logger.LogInformation($"node:sync");

            var nodes = _nodeService.GetNodes();

            // call rest api on each node
            foreach (var node in nodes)
            {
                // skip and update local node
                if (node.IsLocal)
                {
                    if (node.LastNodeSync != null && node.LastNodeSync > DateTime.UtcNow.AddMinutes(-3))
                    {
                        _logger.LogInformation($"node:sync skipping {node.Host}:{node.Port}, last sync was less than 3 minutes ago");
                        return Ok();
                    }else{
                        node.LastNodeSync = DateTime.UtcNow;
                        _nodeService.UpdateNode(node);
                    }
                    continue;
                }

                _logger.LogInformation($"node:sync {node.Host}:{node.Port}");

                var client = new RestClient($"https://{node.Host}:{node.Port}/node/list");
                var request = new RestRequest();
                var response = client.Execute(request);

                _logger.LogInformation($"node:sync {node.Host}:{node.Port}, status: {response.StatusCode}");

                node.IsOnline = response.StatusCode == System.Net.HttpStatusCode.OK;
                node.LastCheck = DateTime.UtcNow;

                if (node.IsOnline)
                {
                    _logger.LogInformation($"node:sync {node.Host}:{node.Port}, ONLINE");

                    var content = response.Content;
                    List<Node> nodesFromResponse = JsonConvert.DeserializeObject<List<Node>>(content);

                    foreach (var nodeFromResponse in nodesFromResponse)
                    {
                        if (nodeFromResponse.NodeHash == _nodeService.NodeHash)
                        {
                            continue;
                        }

                        if (_nodeService.GetNodes().Any(n => n.NodeHash == nodeFromResponse.NodeHash))
                        {
                            _logger.LogInformation($"node:sync updating node {nodeFromResponse.Host}:{nodeFromResponse.Port}");

                            var nodeToUpdate = _nodeService.GetNodeByHash(nodeFromResponse.NodeHash);
                            nodeToUpdate.IsOnline = false;
                            nodeToUpdate.LastCheck = DateTime.UtcNow;

                            _nodeService.UpdateNode(nodeToUpdate);
                        }
                        else
                        {
                            _logger.LogInformation($"node:sync adding node {nodeFromResponse.Host}:{nodeFromResponse.Port}");
                            _nodeService.AddNode(nodeFromResponse);
                        }

                    }
                }
            }

            _logger.LogInformation($"node:sync synced {nodes.Count} nodes");

            nodes = _nodeService.GetNodes();
            return Ok(nodes);
        }

        [HttpGet("check", Name = "CheckNodes")]
        public IActionResult CheckNodes()
        {
            _logger.LogInformation($"node:check");

            var nodes = _nodeService.GetNodes();

            // call rest api on each node
            foreach (var node in nodes)
            {
                // skip local node
                if (node.IsLocal)
                {
                    continue;
                }

                var client = new RestClient($"https://{node.Host}:{node.Port}/node/ping");
                var request = new RestRequest();
                var response = client.Execute(request);

                _logger.LogInformation($"node:check node {node.Host}:{node.Port} responded with {response.StatusCode}");

                node.IsOnline = response.StatusCode == System.Net.HttpStatusCode.OK;
                node.LastCheck = DateTime.UtcNow;

                _nodeService.UpdateNode(node);
            }

            _logger.LogInformation($"node:check checked {nodes.Count} nodes");

            nodes = _nodeService.GetNodes();
            return Ok(nodes);
        }
    }
}