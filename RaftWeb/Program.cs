using System.Text.Json;
using library;

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

//IServer[] otherNodes = [];
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

//Appending Entries
app.MapPost("/request/appendEntries", async (RaftLogEntry request) =>
{
  logger.LogInformation("received append entries request {request}", request);
  IServer? serverRequesting = otherNodes.Where(n => n.Id == request.FromServerId).FirstOrDefault();
  node.ReceiveAppendEntriesLogFrom(request.fromServer, [request]);
  await Task.CompletedTask;
});
 
app.MapPost("/response/appendEntries", (AppendEntryResponse response, int respondingServerId) =>
{
  logger.LogInformation("received append entries response {response}", response);
  IServer? serverResponding = otherNodes.First(n => n.Id == respondingServerId);
  node.ReceiveAppendEntriesLogResponseFrom(serverResponding, response);
});

//Voting
app.MapPost("/request/vote", async (RaftLogEntry discard) =>
{
  logger.LogInformation("received vote request {request}", discard);
  //await node.RequestVote(request);
  foreach (var server in otherNodes) {
    node.SendRequestForVoteRPCTo(server);
  }
});
 
 
app.MapPost("/response/vote", async (AppendEntryResponse response, int respondingServerId) =>
{
  logger.LogInformation("received vote response {response}", response);
  //await node.ResponseVote(response);
  IServer? serverResponding = otherNodes.Where(n => n.Id == respondingServerId).FirstOrDefault();
  node.ReceiveVoteResponseFrom(serverResponding, response.TermNumber, response.Accepted);
});
 
//Client Command
app.MapPost("/request/command", async ((string, string) data) =>
{
  //await node.SendCommand(data);
  node.ReceiveClientCommand(data);
});

app.Run();