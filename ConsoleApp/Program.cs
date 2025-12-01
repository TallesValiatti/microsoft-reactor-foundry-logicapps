using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Azure.Identity;
using OpenAI.Responses;
#pragma warning disable OPENAI001

var url = "https://foundry-power-gpt-dev-westus.services.ai.azure.com/api/projects/firstProject";
var newAgent = "MyFirstAgent";
var model = "gpt-4.1-mini";

AIProjectClient projectClient = new(new Uri(url), new AzureCliCredential());

AgentDefinition agentDefinition = new PromptAgentDefinition(model)
{
    Instructions = "You are a helpful assistant that answers general questions",
};

AgentVersion newAgentVersion = projectClient.Agents.CreateAgentVersion(newAgent, options: new AgentVersionCreationOptions(agentDefinition));

var agentVersions = projectClient.Agents.GetAgentVersions(newAgent);
foreach (AgentVersion oneAgentVersion in agentVersions)
{
    Console.WriteLine($"Agent: {oneAgentVersion.Id}, Name: {oneAgentVersion.Name}, Version: {oneAgentVersion.Version}");
}

var openAIClient = projectClient.GetProjectOpenAIClient();

var agent = await projectClient.Agents.GetAgentAsync(newAgent);

var conversation = await openAIClient.Conversations.CreateProjectConversationAsync();

OpenAIResponseClient responseClient = projectClient.OpenAI.GetProjectResponsesClientForAgent(agent.Value, defaultConversationId: conversation.Value.Id);

OpenAIResponse response = responseClient.CreateResponse("Qual a capital da argentina?");

var items = openAIClient.Conversations.GetProjectConversationItemsAsync(conversation.Value.Id);

Console.WriteLine(response.GetOutputText());

// enumerate here
await foreach (var item in items)
{
    Console.WriteLine($"Item ID: {item.Id}");
}

OpenAIResponse response2 = responseClient.CreateResponse("E do brasil?");

var items2 = openAIClient.Conversations.GetProjectConversationItemsAsync(conversation.Value.Id);

Console.WriteLine(response2.GetOutputText());

// enumerate here
await foreach (var item in items2)
{
    var itemResponse = item.AsOpenAIResponseItem();
    Console.WriteLine($"Item ID: {item.Id}");
}


