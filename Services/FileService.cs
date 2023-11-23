using DHTMfs.Data;
using DHTMfs.Models;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace DHTMfs.Services
{
    public class FileService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileService> _logger;
        private readonly NodeService _nodeService;
        private readonly HFile _HFile;

        private string _directoryPath;

        public FileService(AppDbContext context, IConfiguration configuration, ILogger<FileService> logger, NodeService nodeService, HFile hFile)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _nodeService = nodeService;
            _HFile = hFile;

            _directoryPath = Path.Combine(Path.GetTempPath(), _nodeService.NodeHash);

            if (!Directory.Exists(_directoryPath))
            {
                Directory.CreateDirectory(_directoryPath);
            }

            _logger.LogInformation($"CacheDirectory: {_directoryPath}");
        }

        public string GetDirectoryPath()
        {
            return _directoryPath;
        }

        public void AddOrUpdateFile(Models.File file)
        {
            if (_HFile.Files.ContainsKey(file.FileHash))
            {
                _HFile.Files[file.FileHash] = file;
            }
            else
            {
                _HFile.Files.TryAdd(file.FileHash, file);
            }
        }

        public void UpdateFileList()
        {
            var files = Directory.GetFiles(_directoryPath);

            foreach (var file in files)
            {
                var filePath = Path.Combine(_directoryPath, file);
                var fileHash = ComputeHash.Sha256File(filePath);

                var fileModel = new Models.File
                {
                    FileHash = fileHash,
                    OriginalName = Path.GetFileName(file),
                    Extension = Path.GetExtension(file),
                    Date = new FileInfo(file).LastWriteTimeUtc,
                    Size = new FileInfo(file).Length,
                    NodeHash = _nodeService.NodeHash
                };

                AddOrUpdateFile(fileModel);
            }
        }

        public Dictionary<string, Models.File> GetFiles()
        {
            var files = new Dictionary<string, Models.File>();

            foreach (var file in _HFile.Files)
            {
                files.Add(file.Key, file.Value);
            }

            return files;
        }

        public List<Models.File> SearchFilesByHash(string hash)
        {
            var files = new List<Models.File>();

            if (_HFile.Files.ContainsKey(hash))
            {
                files.Add(_HFile.Files[hash]);
            }

            return files;
        }

        public List<Models.File> SearchFilesByName(string name)
        {
            var files = new List<Models.File>();

            foreach (var file in _HFile.Files)
            {
                if (file.Value.OriginalName.Contains(name))
                {
                    files.Add(file.Value);
                }
            }

            return files;
        }

        public int GetFileCount()
        {
            return _HFile.Files.Count;
        }
    }
}
