using System.ClientModel;

using Azure;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Workshop.ConsoleApp.Plugins.RestaurantAgent;

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

using OpenAI;

var config = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                 .AddUserSecrets<Program>()
                 .Build();

var builder = Kernel.CreateBuilder();
if (string.IsNullOrWhiteSpace(config["Azure:OpenAI:Endpoint"]!) == false)
{
    var client = new AzureOpenAIClient(
        new Uri(config["Azure:OpenAI:Endpoint"]!),
        new AzureKeyCredential(config["Azure:OpenAI:ApiKey"]!));

    builder.AddAzureOpenAIChatCompletion(
                deploymentName: config["Azure:OpenAI:DeploymentName"]!,
                azureOpenAIClient: client);
}
else
{
    var client = new OpenAIClient(
        credential: new ApiKeyCredential(config["GitHub:Models:AccessToken"]!),
        options: new OpenAIClientOptions { Endpoint = new Uri(config["GitHub:Models:Endpoint"]!) });

    builder.AddOpenAIChatCompletion(
                modelId: config["GitHub:Models:ModelId"]!,
                openAIClient: client);
}
var kernel = builder.Build();

kernel.Plugins.AddFromType<MenuPlugin>();
var settings = new PromptExecutionSettings()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};
var agent = new ChatCompletionAgent()
{
    Kernel = kernel,
    Arguments = new KernelArguments(settings),
    Instructions = "Answer questions about the menu.",
    Name = "Host",
};
var history = new ChatHistory();
history.AddSystemMessage("You're a friendly host at a restaurant. Always answer in Korean.");
var input = default(string);
var message = default(string);
while (true)
{
    Console.WriteLine("Hi, I'm your host today. How can I help you today?");
    input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        break;
    }

    Console.Write("Assistant: ");
    history.AddUserMessage(input);

    var response = agent.InvokeStreamingAsync(history);
    await foreach (var content in response)
    {
        await Task.Delay(20);
        message += content;
        Console.Write(content);
    }
    history.AddAssistantMessage(message!);
    Console.WriteLine();

    Console.WriteLine();
}