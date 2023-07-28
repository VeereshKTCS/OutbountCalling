using Azure.Communication;
using Azure.Communication.CallAutomation;
using Azure.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OutboundCalling.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(new CallAutomationClient(builder.Configuration["ACS:ConnectionString"]));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/api/calls", async (CallRequest request, CallAutomationClient client) =>
{
    var applicationId = new PhoneNumberIdentifier(request.Source);

    var callThisPerson = new CallInvite(new PhoneNumberIdentifier(request.Destination), applicationId);


    var createCallOptions = new CreateCallOptions(callThisPerson,
        new Uri(builder.Configuration["VS_TUNNEL_URL"] + "api/callbacks"));

    await client.CreateCallAsync(createCallOptions);
});

app.MapPost("/api/callbacks", async (CloudEvent[] events, CallAutomationClient client, ILogger<Program> logger) =>
{

    foreach (var cloudEvent in events)
    {
        var @event = CallAutomationEventParser.Parse(cloudEvent);
        logger.LogInformation($"Received {@event.GetType()}");

        if (@event is CallConnected callConnected)
        {
            logger.LogInformation($"Call connected: {callConnected.CallConnectionId} | {callConnected.CorrelationId}");
            logger.LogInformation($"Participant added");
            string audioFile = "https://audiofilesane.blob.core.windows.net/audiofiles/VMTestAudio.wav";
            FileSource playSource = new FileSource(new Uri(uriString: audioFile));
            CallAutomationClient callAutomationClient = new CallAutomationClient(builder.Configuration["ACS:ConnectionString"]);
            Thread.Sleep(25000);
            var callMedia = callAutomationClient.GetCallConnection(callConnected.CallConnectionId).GetCallMedia();
            var playResponse = await callMedia.PlayToAllAsync(playSource);
        }


        if (@event is PlayCompleted playCompleted)
        {
            logger.LogInformation($"Play completed! The OperationContext is {playCompleted.OperationContext}.");
            CallAutomationClient callAutomationClient = new CallAutomationClient(builder.Configuration["ACS:ConnectionString"]);
            await callAutomationClient.GetCallConnection(playCompleted.CallConnectionId).HangUpAsync(true);
        }

        if (@event is PlayFailed playFailed)
        {
            logger.LogError($"Play failed: {playFailed.ResultInformation.Message}");
        }
    }
});


app.MapPost("/api/talktoagent", async (CallRequest request, CallAutomationClient client) =>
{
    var applicationId = new PhoneNumberIdentifier(request.Source);
    var callThisPerson = new CallInvite(new PhoneNumberIdentifier(request.Destination), applicationId);
    var createCallOptions = new CreateCallOptions(callThisPerson,
        new Uri(builder.Configuration["VS_TUNNEL_URL"] + "api/talktoagentcallback"));
    await client.CreateCallAsync(createCallOptions);
});

app.MapPost("/api/talktoagentcallback", async (CloudEvent[] events, CallAutomationClient client, ILogger<Program> logger) =>
{

    foreach (var cloudEvent in events)
    {
        var @event = CallAutomationEventParser.Parse(cloudEvent);
        logger.LogInformation($"Received {@event.GetType()}");
        if (@event is CallConnected callConnected)
        {
            logger.LogInformation($"Call connected: {callConnected.CallConnectionId} | {callConnected.CorrelationId}");
            logger.LogInformation($"Participant added");
            string audioFile = "https://audiofilesane.blob.core.windows.net/audiofiles/talktoagent.wav";
            FileSource playSource = new FileSource(new Uri(uriString: audioFile));
            CallAutomationClient callAutomationClient = new CallAutomationClient(builder.Configuration["ACS:ConnectionString"]);
            Thread.Sleep(25000);
            var callMedia = callAutomationClient.GetCallConnection(callConnected.CallConnectionId).GetCallMedia();
            var playResponse = await callMedia.PlayToAllAsync(playSource);
        }


        if (@event is PlayCompleted playCompleted)
        {
            logger.LogInformation($"Play completed! The OperationContext is {playCompleted.OperationContext}.");
            CallAutomationClient callAutomationClient = new CallAutomationClient(builder.Configuration["ACS:ConnectionString"]);
            await callAutomationClient.GetCallConnection(playCompleted.CallConnectionId).HangUpAsync(true);
        }
        if (@event is PlayFailed playFailed)
        {
            logger.LogError($"Play failed: {playFailed.ResultInformation.Message}");
        }
    }
});

app.MapPost("/api/emptycall", async (CallRequest request, CallAutomationClient client) =>
{
    var applicationId = new PhoneNumberIdentifier(request.Source);
    var callThisPerson = new CallInvite(new PhoneNumberIdentifier(request.Destination), applicationId);
    var createCallOptions = new CreateCallOptions(callThisPerson,
        new Uri(builder.Configuration["VS_TUNNEL_URL"] + "api/emptycallback"));
    await client.CreateCallAsync(createCallOptions);
});

app.MapPost("/api/emptycallback", async (CloudEvent[] events, CallAutomationClient client, ILogger<Program> logger) =>
{

    foreach (var cloudEvent in events)
    {
        var @event = CallAutomationEventParser.Parse(cloudEvent);
        logger.LogInformation($"Received {@event.GetType()}");
        if (@event is CallConnected callConnected)
        {
            logger.LogInformation($"Call connected: {callConnected.CallConnectionId} | {callConnected.CorrelationId}");
            logger.LogInformation($"Participant added");
            CallAutomationClient callAutomationClient = new CallAutomationClient(builder.Configuration["ACS:ConnectionString"]);
            Thread.Sleep(25000);
        }
        if (@event is PlayCompleted playCompleted)
        {
            CallAutomationClient callAutomationClient = new CallAutomationClient(builder.Configuration["ACS:ConnectionString"]);
            await callAutomationClient.GetCallConnection(playCompleted.CallConnectionId).HangUpAsync(true);
        }
        if (@event is PlayFailed playFailed)
        {
            logger.LogError($"Play failed: {playFailed.ResultInformation.Message}");
        }
    }
});
app.Run();