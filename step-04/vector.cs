using System.ClientModel;

using Azure;
using Azure.AI.OpenAI;

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;

using Workshop.ConsoleApp.Services;
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
              deploymentName: config["Azure:OpenAI:DeploymentNames:ChatCompletion"]!,
              azureOpenAIClient: client);
}
else
{
  var client = new OpenAIClient(
      credential: new ApiKeyCredential(config["GitHub:Models:AccessToken"]!),
      options: new OpenAIClientOptions { Endpoint = new Uri(config["GitHub:Models:Endpoint"]!) });

  builder.AddOpenAIChatCompletion(
              modelId: config["GitHub:Models:ModelIds:ChatCompletion"]!,
              openAIClient: client);
}
var kernel = builder.Build();

var service = new TextSearchService(config);
var collection = await service.GetVectorStoreRecordCollectionAsync("records");
var search = await service.GetVectorStoreTextSearchAsync(collection);

var input = default(string);
var message = default(string);
while (true)
{
  Console.WriteLine("Ask a question about semantic kernel.");
  Console.Write("User: ");
  input = Console.ReadLine();

  if (string.IsNullOrWhiteSpace(input))
  {
      break;
  }

  Console.Write("Assistant: ");

  var searchResponse = await search.GetTextSearchResultsAsync(input, new TextSearchOptions() { Top = 2, Skip = 0 });
  Console.WriteLine("\n--- Text Search Results ---\n");
  await foreach (var result in searchResponse.Results)
{
  Console.WriteLine($"Name:  {result.Name}");
  Console.WriteLine($"Value: {result.Value}");
  Console.WriteLine($"Link:  {result.Link}");
  Console.WriteLine();
}
  Console.WriteLine();

  Console.WriteLine();
}