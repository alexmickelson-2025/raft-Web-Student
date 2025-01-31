using System.Text.Json;
using library;
using NodeDataClass;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8080");
//In theory I would put getting all the node id, interval scalar, basically all
//environment variables here
var nodeId = Environment.GetEnvironmentVariable("NODE_ID") ?? throw new Exception("NODE_ID environment variable not set");
var otherNodesRaw = Environment.GetEnvironmentVariable("OTHER_NODES") ?? throw new Exception("OTHER_NODES environment variable not set");
var nodeIntervalScalarRaw = Environment.GetEnvironmentVariable("NODE_INTERVAL_SCALAR") ?? throw new Exception("NODE_INTERVAL_SCALAR environment variable not set");


var serviceName = "Node" + nodeId;
builder.Services.AddLogging();

var app = builder.Build();
var logger = app.Services.GetService<ILogger<Program>>();


IServer[] otherNodes = otherNodesRaw
  .Split(";")
  .Select(s => new HttpRpcToAnotherNode(int.Parse(s.Split(",")[0]), s.Split(",")[1]))
  .ToArray();

logger.LogInformation("other nodes {nodes}", JsonSerializer.Serialize(otherNodes));
 
 
var node = new Server(true, true) //Alex's code passed it in the other nodes in the parameter, so I need to do that below
{
  OtherServersList = otherNodes.ToList(),
  Id = int.Parse(nodeId)
  //,logger = app.Services.GetService<ILogger<RaftNode>>()
};
 
logger.LogInformation("Node ID {name}", nodeId);
logger.LogInformation("Other nodes environment config: {}", otherNodesRaw);

app.MapGet("/", () => "Hello World!");
app.MapGet("/health", () => {
    Console.WriteLine($"called health check on node {nodeId}");
    return "raft web app is healthy";
});

app.MapGet("/nodeData", () =>
{
  return new NodeData(){
    Id = node.Id,
    //Status: node.Status,
    ElectionTimeout = node.ElectionTimeout,
    Term = node.CurrentTerm,
    CurrentTermLeader = node.RecognizedLeader,
    CommittedEntryIndex = node.HighestCommittedIndex,
    Log =  node.LogBook,
    State = node.State,
    NodeIntervalScalar = 0
  };
});

app.Run();
