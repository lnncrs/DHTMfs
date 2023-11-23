using DHTMfs.Data;
using DHTMfs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;
using DHTMfs.Services;
using RestSharp;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DHTMfs.Controllers
{
    [ApiController]
    [Route("file")]
    public class FileController(ILogger<NodeController> logger, FileService fileService, NodeService nodeService) : ControllerBase
    {
        private readonly ILogger<NodeController> _logger = logger;
        private readonly FileService _fileService = fileService;
        private readonly NodeService _nodeService = nodeService;

        [HttpGet("list", Name = "GetFiles")]
        public ActionResult<List<Models.File>> Get()
        {
            _logger.LogInformation($"file:list");

            var files = _fileService.GetFiles();
            return Ok(files);
        }

        [HttpGet("synclocal", Name = "SyncLocalFiles")]
        public ActionResult SyncLocalFiles()
        {
            _logger.LogInformation($"file:synclocal");

            _fileService.UpdateFileList();
            return Ok();
        }

        [HttpGet("sync", Name = "SyncRemoteFiles")]
        public ActionResult SyncRemoteFiles()
        {
            _logger.LogInformation($"file:sync");

            var nodes = _nodeService.GetNodes().Where(n => n.IsOnline == true).ToList();

            // call rest api on each node
            foreach (var node in nodes)
            {
                // skip local node
                if (node.IsLocal)
                {
                    if (node.LastFileSync != null && node.LastFileSync > DateTime.UtcNow.AddMinutes(-3))
                    {
                        _logger.LogInformation($"file:sync skipping {node.Host}:{node.Port}, last sync was less than 3 minutes ago");
                        return Ok();
                    }else{
                        node.LastFileSync = DateTime.UtcNow;
                        _nodeService.UpdateNode(node);
                    }
                    continue;
                }

                _logger.LogInformation($"file:sync {node.Host}:{node.Port}");

                var client = new RestClient($"https://{node.Host}:{node.Port}/file/list");
                var request = new RestRequest();
                var response = client.Execute(request);

                _logger.LogInformation($"file:sync {node.Host}:{node.Port}, status: {response.StatusCode}");

                if(response.StatusCode == System.Net.HttpStatusCode.OK){

                    _logger.LogInformation($"file:sync {node.Host}:{node.Port}, ONLINE");

                    var content = response.Content;
                    Dictionary<string, Models.File> filesFromResponse = JsonConvert.DeserializeObject<Dictionary<string, Models.File>>(content);

                    foreach(var fileFromResponse in filesFromResponse.Values){

                        _logger.LogInformation($"file:sync updating file {fileFromResponse.OriginalName}");

                        _fileService.AddOrUpdateFile(fileFromResponse);
                    }
                }
            }

            var files = _fileService.GetFiles();
            _logger.LogInformation($"file:sync synced {files.Count} nodes");
            return Ok(files);
        }

        [HttpGet("search", Name = "SearchFileRemote")]
        public ActionResult SearchFileRemote(string hash, bool useHashAsName = false)
        {
            _logger.LogInformation($"file:search");

            var files = new List<Models.File>();

            // search local first
            if (!useHashAsName)
            {
                files = _fileService.SearchFilesByHash(hash);
            }
            else
            {
                files = _fileService.SearchFilesByName(hash);
            }

            if(files.Count > 0)
            {
                return Ok(files);
            }

            // search remote
            var nodes = _nodeService.GetNodes().Where(n => n.IsOnline == true && n.IsLocal == false).ToList();

            // call rest api on each node
            foreach (var node in nodes)
            {
                _logger.LogInformation($"file:search {node.Host}:{node.Port}");

                var client = new RestClient($"https://{node.Host}:{node.Port}/file/searchlocal?hash={hash}&useHashAsName={useHashAsName}");
                var request = new RestRequest();
                var response = client.Execute(request);

                _logger.LogInformation($"file:search {node.Host}:{node.Port}, status: {response.StatusCode}");

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation($"file:search {node.Host}:{node.Port}, ONLINE");

                    var content = response.Content;
                    List<Models.File> filesFromResponse = JsonConvert.DeserializeObject<List<Models.File>>(content);

                    foreach (var fileFromResponse in filesFromResponse)
                    {
                        _logger.LogInformation($"file:search adding file {fileFromResponse.OriginalName}");

                        files.Add(fileFromResponse);
                    }

                    //if found, exit foreach
                    if(files.Count > 0){
                        break;
                    }
                }
            }

            return Ok(files);
        }

        [HttpGet("searchlocal", Name = "SearchFileLocal")]
        public ActionResult SearchFileLocal(string hash, bool useHashAsName = false)
        {
            _logger.LogInformation($"file:searchlocal");

            if (!useHashAsName)
            {
                var files = _fileService.SearchFilesByHash(hash);
                return Ok(files);
            }
            else
            {
                var files = _fileService.SearchFilesByName(hash);
                return Ok(files);
            }
        }
    }
}