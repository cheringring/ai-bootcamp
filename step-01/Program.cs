using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

var config = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                 .AddUserSecrets<Program>()
                 .Build();

var input = default(string);
var kernel = Kernel.CreateBuilder()
                   .AddGoogleAIGeminiChatCompletion(
                        modelId: config["Google:Gemini:ModelName"]!,
                        apiKey: config["Google:Gemini:ApiKey"]!,
                        serviceId: "google")
                   .Build();
var message = default(string);
while (true)
{
    Console.Write("User: ");
    input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        break;
    }

    Console.Write("Assistant: ");
Console.WriteLine();
Console.WriteLine("--- Response from Google Gemini ---");
var responseGoogle = kernel.InvokePromptStreamingAsync(
        promptTemplate: input,
        arguments: new KernelArguments(new PromptExecutionSettings() { ServiceId = "google" }));
await foreach (var content in responseGoogle)
{
    await Task.Delay(20);
    message += content;
    Console.Write(content);
}
Console.WriteLine();
}