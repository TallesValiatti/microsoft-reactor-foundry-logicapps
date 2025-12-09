using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Azure.Identity;
using OpenAI.Responses;

namespace WeatherAgentApi.Services;

[Experimental("OPENAI001")]
public class AgentService
{
    private readonly string _url = "https://msft-foundry-reactor.services.ai.azure.com/api/projects/proj-default";
    private readonly string _agentName = "weather-agent-reactor";
    private readonly string _model = "gpt-5-mini";
    private readonly string _connectionName = "LogicApps_Tool_Connection_logicappweather_9961";
    
    private AIProjectClient? _projectClient;
    private OpenAIResponseClient? _responseClient;
    private readonly ConcurrentDictionary<string, string> _conversations = new();

    public async Task InitializeAgent()
    {
        _projectClient = new AIProjectClient(new Uri(_url), new AzureCliCredential());

        var connectionResult = _projectClient.Connections.GetConnection(
            _connectionName,
            includeCredentials: false
        );

        AIProjectConnection connection = connectionResult;
        var projectSecurityScheme = new OpenAPIProjectConnectionSecurityScheme(connection.Id);
        OpenAPIAuthenticationDetails auth = new OpenAPIProjectConnectionAuthenticationDetails(projectSecurityScheme);

        var openApiFunction = ProjectsOpenAIModelFactory.OpenAPIFunctionDefinition(
            Constants.OpenApiToolName,
            Constants.OpenApiToolDescription,
            BinaryData.FromString(Constants.OpenApiToolSpec),
            auth: auth,
            defaultParams: ["api-version"]
        );

        AgentTool openApiAgentTool = AgentTool.CreateOpenApiTool(openApiFunction);

        AgentDefinition agentDefinition = new PromptAgentDefinition(_model)
        {
            Instructions = "You are a helpful weather assistant that answers questions about weather in different locations",
            Tools = { openApiAgentTool }
        };

        _projectClient.Agents.CreateAgentVersion(
            _agentName,
            options: new AgentVersionCreationOptions(agentDefinition)
        );

        var agent = await _projectClient.Agents.GetAgentAsync(_agentName);
        _responseClient = _projectClient.OpenAI.GetProjectResponsesClientForAgent(agent.Value);
    }

    public async Task<string> CreateConversation()
    {
        if (_projectClient == null)
            throw new InvalidOperationException("Agent not initialized");

        var openAiClient = _projectClient.GetProjectOpenAIClient();
        var conversation = await openAiClient.Conversations.CreateProjectConversationAsync();
        var conversationId = conversation.Value.Id;
        
        _conversations.TryAdd(conversationId, conversationId);
        
        return conversationId;
    }

    public async Task<string> SendMessage(string conversationId, string message)
    {
        if (_projectClient == null || _responseClient == null)
            throw new InvalidOperationException("Agent not initialized");

        if (!_conversations.ContainsKey(conversationId))
            throw new InvalidOperationException("Conversation not found");

        var agent = await _projectClient.Agents.GetAgentAsync(_agentName);

        var responseClient = _projectClient.OpenAI.GetProjectResponsesClientForAgent(
            agent.Value, 
            defaultConversationId: conversationId
        );

        OpenAIResponse response = await responseClient.CreateResponseAsync(message);
        return response.GetOutputText();
    }

    public async Task<List<ConversationItem>> GetConversationHistory(string conversationId)
    {
        if (_projectClient == null)
            throw new InvalidOperationException("Agent not initialized");

        var openAiClient = _projectClient.GetProjectOpenAIClient();
        var items = openAiClient.Conversations.GetProjectConversationItemsAsync(conversationId);
        
        var history = new List<ConversationItem>();
        
        await foreach (var item in items)
        {
            history.Add(new ConversationItem(
                item.Id,
                item.ToString() ?? "",
                "unknown",
                DateTime.UtcNow
            ));
        }

        return history;
    }

    public async Task<List<string>> GetConnections()
    {
        if (_projectClient == null)
            throw new InvalidOperationException("Agent not initialized");

        var connections = new List<string>();
        var connectionResults = _projectClient.Connections.GetConnectionsAsync();

        await foreach (var item in connectionResults)
        {
            connections.Add($"{item.Name} (ID: {item.Id})");
        }

        return connections;
    }
}