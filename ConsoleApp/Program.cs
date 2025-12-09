using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Azure.Identity;
using ConsoleApp;
using OpenAI.Responses;
#pragma warning disable OPENAI001

var url = "https://msft-foundry-reactor.services.ai.azure.com/api/projects/proj-default";
var newAgent = "weather-agent-reactor";
var model = "gpt-5-mini";
var connectionName = "LogicApps_Tool_Connection_logicappweather_9961";

AIProjectClient projectClient = new(new Uri(url), new AzureCliCredential()); 

var connectionResults = projectClient.Connections.GetConnectionsAsync();

await foreach (var item in connectionResults)
{
    Console.WriteLine($"Connection: {item.Id}, Name: {item.Name}");
}

var connectionResult = projectClient.Connections.GetConnection(
    connectionName,
    includeCredentials: false // or true if you ever need them
);

AIProjectConnection connection = connectionResult;

// connection.Id is your Project Connection ID
var projectSecurityScheme = new OpenAPIProjectConnectionSecurityScheme(connection.Id);

OpenAPIAuthenticationDetails auth =
    new OpenAPIProjectConnectionAuthenticationDetails(projectSecurityScheme);

// Describe the OpenAPI tool to the Agent Service
var openApiFunction = ProjectsOpenAIModelFactory.OpenAPIFunctionDefinition(
    Constants.OpenApiToolName,
    Constants.OpenApiToolDescription,    // used by the model to choose the tool
    BinaryData.FromString(Constants.OpenApiToolSpec),
    auth: auth,
    defaultParams: new[] { "api-version" }            // optional: parameters that will be filled by defaults
);

// Turn that function into an AgentTool
AgentTool openApiAgentTool = AgentTool.CreateOpenApiTool(openApiFunction);

// 4. Build the agent definition and add the tool
AgentDefinition agentDefinition = new PromptAgentDefinition(model)
{
    Instructions = "You are a helpful assistant that answers general questions",
    Tools =
    {
        openApiAgentTool
    }
};

projectClient.Agents.CreateAgentVersion(
    newAgent, 
    options: new AgentVersionCreationOptions(agentDefinition));

var agentVersions = projectClient.Agents.GetAgentVersions(newAgent);
foreach (AgentVersion oneAgentVersion in agentVersions)
{
    Console.WriteLine($"Agent: {oneAgentVersion.Id}, Name: {oneAgentVersion.Name}, Version: {oneAgentVersion.Version}");
}

var openAiClient = projectClient.GetProjectOpenAIClient();
    
var agent = await projectClient.Agents.GetAgentAsync(newAgent);

var conversation = await openAiClient.Conversations.CreateProjectConversationAsync();

OpenAIResponseClient responseClient = projectClient.OpenAI.GetProjectResponsesClientForAgent(agent.Value, defaultConversationId: conversation.Value.Id);

OpenAIResponse response = responseClient.CreateResponse("Qual o tempo em buenos aires?");

var items = openAiClient.Conversations.GetProjectConversationItemsAsync(conversation.Value.Id);

Console.WriteLine(response.GetOutputText());

// enumerate here
await foreach (var item in items)
{
    Console.WriteLine($"Item ID: {item.Id}");
}

OpenAIResponse response2 = responseClient.CreateResponse("E de vitorias ES?");

var items2 = openAiClient.Conversations.GetProjectConversationItemsAsync(conversation.Value.Id);

Console.WriteLine(response2.GetOutputText());

// enumerate here
await foreach (var item in items2)
{
    var itemResponse = item.AsOpenAIResponseItem();
    Console.WriteLine($"Item ID: {item.Id}");
}

var a = 1;