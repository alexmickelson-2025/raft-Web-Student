﻿//using BlazorServerWebSimulation;
//using static System.Collections.Specialized.BitVector32;
//using System.Data;
//using System.Reflection.Emit;
//using System.Threading;
//using System.Xml.Linq;
//using System;

//namespace BlazorServerWebSimulation;

//public class ForReferenceOnlyAlexCode
//{
//}

// ////****NOTE: I just copy/pasted the code in here so I could read it/sort through it better ******//

//public class RaftSimulationNode : INode
//{
//    public string Message { get; private set; } = "";
//    public static int NetworkRequestDelay { get; set; } = 1000;
//    public static int NetworkResponseDelay { get; set; } = 0;
//    public bool SimulationRunning { get; private set; } = false;
//    public NodeStatus Status => InnerNode.Status;
//    public int CurrentTerm => InnerNode.CurrentTerm;

//    public int Id => InnerNode.Id;

//    public RaftNode InnerNode { get; }

//    public int CurrentTermLeader => InnerNode.CurrentTermLeader;

//    public Task RequestAppendEntries(AppendEntriesData request)
//    {
//        if (!SimulationRunning)
//            return Task.CompletedTask;
//        Task.Delay(NetworkRequestDelay).ContinueWith(async (_previousTask) =>
//        {
//            Message = $"Received Append Entries from {request.LeaderId}";
//            await InnerNode.RequestAppendEntries(request);
//        });
//        return Task.CompletedTask;
//    }

//    public Task RequestVote(VoteRequestData request)
//    {
//        if (!SimulationRunning)
//            return Task.CompletedTask;
//        Task.Delay(NetworkRequestDelay).ContinueWith(async (_previousTask) =>
//        {
//            Message = $"Received request to vote for {request.CandidateId} for term {request.Term}";
//            await InnerNode.RequestVote(request);
//        });
//        return Task.CompletedTask;
//    }

//    public Task ResponseVote(VoteResponseData response)
//    {
//        if (!SimulationRunning)
//            return Task.CompletedTask;
//        Task.Delay(NetworkResponseDelay).ContinueWith(async (_previousTask) =>
//        {
//            Message = $"Sending vote for term {response.TermId}";
//            await InnerNode.ResponseVote(response);
//        });
//        return Task.CompletedTask;
//    }

//    public Task RespondAppendEntries(RespondEntriesData response)
//    {
//        if (!SimulationRunning)
//            return Task.CompletedTask;
//        Task.Delay(NetworkResponseDelay).ContinueWith(async (_previousTask) =>
//        {
//            Message = $"";
//            await InnerNode.RespondAppendEntries(response);
//        });
//        return Task.CompletedTask;
//    }

//    public RaftSimulationNode(RaftNode node)
//    {
//        InnerNode = node;
//    }

//    public void StopSimulationLoop()
//    {
//        Console.WriteLine("canceling election");
//        InnerNode.CancellationTokenSource.Cancel();
//        SimulationRunning = false;
//    }
//    public void StartSimulationLoop()
//    {
//        InnerNode.CancellationTokenSource = new CancellationTokenSource();
//        InnerNode.RunElectionLoop();
//        SimulationRunning = true;
//    }

//    public async Task SendCommand(ClientCommandData data)
//    {
//        if (!SimulationRunning)
//            return;
//        await InnerNode.SendCommand(data);
//    }
//}
//@page "/"
//@rendermode InteractiveServer
//@implements IDisposable

//<PageTitle>Home</PageTitle>

//<h1>Raft Simulation</h1>

//@if (!isRunning)
//{
//  < button class="btn btn-primary" @onclick=StartSimulation>Start Simulation</button>
//}
//else
//{
//  < button class= "btn btn-danger" @onclick = "StopNodes" > Stop Simulation </ button >
//}

//< div class= "row" >
//  < div class= "col" >
//    < div >

//      < label for= "speed" >
//        Election timeout between @(FormatMilliSeconds(150 * RaftNode.NodeIntervalScalar)) and @(FormatMilliSeconds(300 *
//        RaftNode.NodeIntervalScalar)) seconds


//        < input type = "range" id = "speed" name = "speed" min = "1" max = "150" @bind:event= "oninput"
//          @bind = RaftNode.NodeIntervalScalar @onchange = UpdateTimer />
//      </ label >
//    </ div >
//    < div >

//      < label for= "NetworkRequestDelay" >
//        Network Delay @FormatMilliSeconds(RaftSimulationNode.NetworkRequestDelay) seconds


//        < input type = "range" id = "NetworkRequestDelay" name = "NetworkRequestDelay" min = "10" max = "10000"
//          @bind:event= "oninput" @bind = RaftSimulationNode.NetworkRequestDelay />
//      </ label >
//    </ div >
//  </ div >
//  < div class= "col" >
//    < div class= "border p-3 rounded-3" >
//      < label >
//        Key < input class= "form-control" @bind = userInputKey />
//      </ label >
//      < label >
//        Value < input class= "form-control" @bind = userInputValue />
//      </ label >
//      < hr >
//      @foreach(var node in nodes)
//      {
//        < button class= "btn btn-outline-primary mx-1" @onclick = "() => SendCommand(node.Id)" > Send to Node @node.Id</button>
//      }



//      < div >
//        @commandStatus
//      </ div >
//    </ div >
//  </ div >
//</ div >


//< div class= "row" >
//  @foreach(var node in nodes)
//  {
//    var timeoutRemaining = DateTime.Now - node.InnerNode.ElectionTimeout;
//    var maxIntervalMilliseconds = 300 * RaftNode.NodeIntervalScalar;
//    var percentageRemaining = (int)(100 * (Math.Abs(timeoutRemaining.TotalMilliseconds) / maxIntervalMilliseconds));

//    < div class= "p-3 col-4" >
//      < div class= "border p-3 rounded-3" >
//        < div class= "d-flex justify-content-between" >
//          < h3 >
//            Node @node.Id
//          </ h3 >
//          @if(node.SimulationRunning)
//          {
//            < button class= "btn btn-outline-danger" @onclick = "node.StopSimulationLoop" > Stop Node </ button >
//          }
//          else
//{
//            < button class= "btn btn-outline-primary" @onclick = "node.StartSimulationLoop" > Start Node </ button >
//          }
//        </ div >
//        < div class= "@StateClass(node)" >
//          @node.Status
//</ div >
//        < div class= "@TermClass(node)" >
//Term @node.CurrentTerm
//</ div >
//        < div >
//Leader is @node.CurrentTermLeader
//        </ div >
//        < div >
//          < div class= "progress" role = "progressbar" aria - label = "Basic example"
//            aria - valuenow =@(Math.Abs(timeoutRemaining.TotalMilliseconds)) aria - valuemin = "0"
//            aria - valuemax = "@(maxIntervalMilliseconds)" >
//            < div class= "progress-bar bg-dark-subtle" style = "width: @percentageRemaining%;" ></ div >
//          </ div >

//        </ div >
//        < div class= "text-body-secondary text-break" style = "height: 3em;" >
//@node.Message
//        </ div >

//        < div class= "bg-body-secondary my-3 p-1 rounded-3" >
//          < strong > LOG </ strong >
//          @foreach(var(log, index) in node.InnerNode.Log.Select((l, i) => (l, i)))
//          {
//            < div class=@(index <= node.InnerNode.CommittedEntryIndex ? "text-success" : "") >
//              @log.TermId - @log.Key: @log.Value
//            </ div >
//          }
//        </ div >

//        < div class= "bg-body-secondary my-3 p-1 rounded-3" >
//          < strong > STATE </ strong >
//          @foreach(var entry in node.InnerNode.State)
//          {
//            < div > @entry.Key: @entry.Value </ div >
//          }
//        </ div >
//      </ div >
//    </ div >
//  }
//</ div >


//@code {
//    List<RaftSimulationNode> nodes { get; set; } = [];
//    string userInputKey = "";
//    string userInputValue = "";
//    string commandStatus = "";

//    async Task SendCommand(int destinationId)
//    {
//        commandStatus = "sending";
//        var dest = nodes.FirstOrDefault(n => n.Id == destinationId);

//        var command = new ClientCommandData(
//        Type: ClientCommandType.Set,
//        Key: userInputKey,
//        Value: userInputValue,
//        RespondToClient: async (success, leaderId) =>
//        {
//            commandStatus = $"response was {success}, leader is {leaderId}";
//        });
//        await dest.SendCommand(command);
//    }

//    string TermClass(RaftSimulationNode node)
//    {
//        var maxTerm = nodes.Select(n => n.CurrentTerm).Max();
//        if (maxTerm == node.CurrentTerm)
//            return "text-primary";
//        return "";
//    }
//    string StateClass(RaftSimulationNode node)
//    {
//        if (node.Status == NodeStatus.Leader)
//            return "text-primary";
//        if (node.Status == NodeStatus.Candidate)
//            return "text-warning";
//        if (node.Status == NodeStatus.Follower)
//            return "text-body-secondary";
//        return "";
//    }
//    bool isRunning = false;

//  private Timer? timer;

//protected override void OnInitialized()
//{
//    RaftNode.NodeIntervalScalar = 12;
//}

//void StartSimulation()
//{
//    var node1 = new RaftNode([]) { Id = 1 };
//    var node2 = new RaftNode([]) { Id = 2 };
//    var node3 = new RaftNode([]) { Id = 3 };

//    var simulationWrapper1 = new RaftSimulationNode(node1);
//    var simulationWrapper2 = new RaftSimulationNode(node2);
//    var simulationWrapper3 = new RaftSimulationNode(node3);

//    node1.ClusterNodes = [simulationWrapper2, simulationWrapper3];
//    node2.ClusterNodes = [simulationWrapper1, simulationWrapper3];
//    node3.ClusterNodes = [simulationWrapper1, simulationWrapper2];

//    nodes = [simulationWrapper1, simulationWrapper2, simulationWrapper3];

//    simulationWrapper1.StartSimulationLoop();
//    simulationWrapper2.StartSimulationLoop();
//    simulationWrapper3.StartSimulationLoop();

//    isRunning = true;
//    timer = new Timer(_ =>
//    {
//        InvokeAsync(StateHasChanged);
//    }, null, 0, 200);
//}

//public void UpdateTimer()
//{
//    @* timer?.Dispose();
//    timer = new Timer(_ =>
//    {
//      InvokeAsync(StateHasChanged);
//    }, null, 0, 100 * (int)RaftNode.NodeIntervalScalar); *@
//}
//public void StopNodes()
//{
//    foreach (var node in nodes)
//    {
//        node.StopSimulationLoop();
//    }
//    isRunning = false;
//    StateHasChanged();
//    timer?.Dispose();
//    timer = null;
//}

//public void Dispose()
//{
//    StopNodes();
//}

//public static string FormatTimeSpan(TimeSpan timeSpan)
//{
//    double totalSeconds = timeSpan.TotalSeconds;
//    return $"{totalSeconds:F1}";
//}

//public static string FormatMilliSeconds(double milliSeconds)
//{
//    return $"{milliSeconds / 1000.0:F1}";
//}
//}