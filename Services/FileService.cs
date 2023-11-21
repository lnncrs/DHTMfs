using DHTMfs.Data;

namespace DHTMfs.Services
{
    public class FileService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileService> _logger;

        private string _directoryPath;

        public FileService(AppDbContext context, IConfiguration configuration, ILogger<FileService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;

            _directoryPath = Path.GetTempPath();
        }

        public List<string> GetFiles()
        {
            var files = Directory.GetFiles(_directoryPath);
            return files.ToList();
        }
    }
}
