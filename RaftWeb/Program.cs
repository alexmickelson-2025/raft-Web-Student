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
logger.LogInformation("Node ID {name}", nodeId);
logger.LogInformation("Other nodes environment config: {}", otherNodesRaw);

app.MapGet("/", () => "Hello World!");
app.MapGet("/health", () => "raft web app is healthy");

app.Run();
