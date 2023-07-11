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
            FileSource playSource = new FileSource(new Uri(uriString: builder.Configuration["ACS:AudioFile"]));

            CallAutomationClient callAutomationClient = new CallAutomationClient(builder.Configuration["ACS:ConnectionString"]);
            var callMedia = callAutomationClient.GetCallConnection(callConnected.CallConnectionId).GetCallMedia();

            
            var playResponse = await callMedia.PlayToAllAsync(playSource);

            //await client.GetCallConnection(callConnected.CallConnectionId).GetCallMedia().PlayToAllAsync(playSource, playOptions);
        }

        if (@event is PlayCompleted playCompleted)
        {
            logger.LogInformation($"Play completed! The OperationContext is {playCompleted.OperationContext}.");
        }

        if (@event is PlayFailed playFailed)
        {
            logger.LogError($"Play failed: {playFailed.ResultInformation.Message}");
        }
    }
});


app.Run();