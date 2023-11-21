using DHTMfs.Data;
using DHTMfs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;
using DHTMfs.Services;

namespace DHTMfs.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly ILogger<NodesController> _logger;
        private readonly FileService _fileService;

        public FilesController(ILogger<NodesController> logger, FileService fileService)
        {
            _logger = logger;
            _fileService = fileService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var files = _fileService.GetFiles();
            return Ok(files);
        }
    }
}