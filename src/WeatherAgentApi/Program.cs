using Microsoft.AspNetCore.Mvc;
using WeatherAgentApi;
using WeatherAgentApi.Services;

#pragma warning disable OPENAI001

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Registrar serviços
builder.Services.AddSingleton<AgentService>();

var app = builder.Build();

app.UseCors();
app.UseHttpsRedirection();

// Endpoints
app.MapPost("/api/agent/initialize", async (AgentService agentService) =>
{
    try
    {
        await agentService.InitializeAgent();
        return Results.Ok(new { message = "Agent initialized successfully" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("InitializeAgent");

app.MapPost("/api/conversations/create", async (AgentService agentService) =>
    {
        try
        {
            var conversationId = await agentService.CreateConversation();
            return Results.Ok(new { conversationId });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    })
    .WithName("CreateConversation");

app.MapPost("/api/conversations/{conversationId}/message", async (
        string conversationId,
        [FromBody] MessageRequest request,
        AgentService agentService) =>
    {
        try
        {
            var response = await agentService.SendMessage(conversationId, request.Message);
            return Results.Ok(new { response });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    })
    .WithName("SendMessage");

app.MapGet("/api/connections", async (AgentService agentService) =>
{
    try
    {
        var connections = await agentService.GetConnections();
        return Results.Ok(connections);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("GetConnections");

app.Run();