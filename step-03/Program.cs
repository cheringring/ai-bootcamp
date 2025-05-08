using System.ClientModel;

using Azure;
using Azure.AI.OpenAI;

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

var background = "I really enjoy food and outdoor activities.";
var prompt = """
    You are a helpful travel guide. 
    I'm visiting {{$city}}. {{$background}}. What are some activities I should do today?
    """;
    // 사용자 입력 city와 고정된 background 정보를 바탕으로 프롬프트를 구성함
    
var input = default(string);
var message = default(string);
while (true)
{
    Console.WriteLine("Tell me about a city you are visiting.");
    Console.Write("User: ");
    input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        break;
    }

    Console.Write("Assistant: ");
    
    
    // prompt를 기반으로 함수 생성 → city와 background를 인자로 전달 → AI 응답을 스트리밍 방식으로 출력
    
    var function = kernel.CreateFunctionFromPrompt(prompt);
    var arguments = new KernelArguments()
{
    { "city", input },
    { "background", background }
    
    // 사용자가 도시명을 입력하면 AI가 해당 도시와 개인 배경에 기반한 활동을 추천
};

    var response = kernel.InvokeStreamingAsync(function, arguments);
    await foreach (var content in response)
    {
        await Task.Delay(20);
        message += content;
        Console.Write(content);
    }
    Console.WriteLine();
}