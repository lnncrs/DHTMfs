using DHTMfs.Data;
using DHTMfs.Models;

namespace DHTMfs.Services
{
    public class NodeService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NodeService> _logger;

        private List<string> _urls;

        private string _host;

        private int _port;

        private string _nodeHash;

        public string NodeHash
        {
            get { return _nodeHash; }
        }

        public string Host
        {
            get { return _host; }
        }

        public int Port
        {
            get { return _port; }
        }

        public NodeService(AppDbContext context, IConfiguration configuration, ILogger<NodeService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;

            _urls = _configuration["Urls"].Split(';').ToList();
            _host = _urls[1].Split(':')[1].Trim('/');
            _port = int.Parse(_urls[1].Split(':')[2]);
            _nodeHash = ComputeHash.Sha256($"{_host}:{_port}");

            _logger.LogInformation($"NodeHash: {_nodeHash}");
            _logger.LogInformation($"NodeAddr: {_host}:{_port}");
        }

        public List<Node> GetNodes()
        {
            return _context.Nodes.ToList();
        }

        public Node GetNodeByHash(string hash)
        {
            return _context.Nodes.FirstOrDefault(n => n.NodeHash == hash);
        }

        public Node GetLocalNode()
        {
            return _context.Nodes.FirstOrDefault(n => n.IsLocal == true);
        }

        public void AddNode(Node node)
        {
            _context.Nodes.Add(node);
            _context.SaveChanges();
        }

        public void DeleteNode(Node node)
        {
            _context.Nodes.Remove(node);
            _context.SaveChanges();
        }

        public void UpdateNode(Node node)
        {
            _context.Nodes.Update(node);
            _context.SaveChanges();
        }

        public void UpdateIsLocal()
        {

            var nodes = _context.Nodes.ToList();

            nodes.Where(x => x.NodeHash != _nodeHash).ToList().ForEach(n => n.IsLocal = false);

            _context.Nodes.UpdateRange(nodes);
            _context.SaveChanges();
        }

        public bool AddNode(string host, int port)
        {
            var hash = ComputeHash.Sha256($"{host}:{port}");

            // new node
            var node = new Node
            {
                NodeHash = hash,
                Host = host,
                Port = port,
                LastCheck = DateTime.UtcNow,
                IsOnline = false,
                IsLocal = false
            };

            try
            {
                _context.Nodes.Add(node);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
                // throw;
            }
        }

        public bool RemoveNode(string hash)
        {
            var node = _context.Nodes.FirstOrDefault(n => n.NodeHash == hash);

            if(node == null)
            {
                return false;
            }

            try
            {
                _context.Nodes.Remove(node);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
                // throw;
            }
        }

        public void UpdateLocalNode()
        {
            // Current node
            var node = new Node
            {
                NodeHash = _nodeHash,
                Host = _host,
                Port = _port,
                LastCheck = DateTime.UtcNow,
                IsOnline = true,
                IsLocal = true
            };

            var existingNode = _context.Nodes.FirstOrDefault(n => n.NodeHash == _nodeHash);

            if (existingNode != null)
            {
                existingNode.LastCheck = DateTime.UtcNow;
                existingNode.IsOnline = true;
                existingNode.IsLocal = true;

                _context.Nodes.Update(existingNode);
            }
            else
            {
                _context.Nodes.Add(node);
            }

            _context.SaveChanges();
        }
    }
}
