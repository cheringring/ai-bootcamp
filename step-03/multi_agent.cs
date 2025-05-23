using System.ClientModel;

using Azure;
using Azure.AI.OpenAI;

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;

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
var reviewerName = "ProjectManager";
var reviewerInstructions =
  """
  You are a project manager who has opinions about copywriting born of a love for David Ogilvy.
  The goal is to determine if the given copy is acceptable to print.
  If so, state that it is approved.
  If not, provide insight on how to refine suggested copy without examples.
  """;

var agentReviewer = new ChatCompletionAgent()
{
  Name = reviewerName,
  Instructions = reviewerInstructions,
  Kernel = kernel
};
var copywriterName = "Copywriter";
var copywriterInstructions =
  """
  You are a copywriter with ten years of experience and are known for brevity and a dry humor.
  The goal is to refine and decide on the single best copy as an expert in the field.
  Only provide a single proposal per response.
  Never delimit the response with quotation marks.
  You're laser focused on the goal at hand.
  Don't waste time with chit chat.
  Consider suggestions when refining an idea.
  """;

var agentWriter = new ChatCompletionAgent()
{
  Name = copywriterName,
  Instructions = copywriterInstructions,
  Kernel = kernel
};
var terminationFunction =
  AgentGroupChat.CreatePromptFunctionForStrategy(
      """
      Determine if the copy has been approved. If so, respond with a single word: yes

      History:
      {{$history}}
      """,
      safeParameterNames: "history");
var selectionFunction =
  AgentGroupChat.CreatePromptFunctionForStrategy(
      $$$"""
      Determine which participant takes the next turn in a conversation based on the the most recent participant.
      State only the name of the participant to take the next turn.
      No participant should take more than one turn in a row.
      
      Choose only from these participants:
      - {{{reviewerName}}}
      - {{{copywriterName}}}
      
      Always follow these rules when selecting the next participant:
      - After {{{copywriterName}}}, it is {{{reviewerName}}}'s turn.
      - After {{{reviewerName}}}, it is {{{copywriterName}}}'s turn.

      History:
      {{$history}}
      """,
      safeParameterNames: "history");
var strategyReducer = new ChatHistoryTruncationReducer(1);
var chat = new AgentGroupChat(agentWriter, agentReviewer)
{
  ExecutionSettings = new AgentGroupChatSettings()
  {
      SelectionStrategy = new KernelFunctionSelectionStrategy(selectionFunction, kernel)
      {
          InitialAgent = agentWriter,
          ResultParser = (result) => result.GetValue<string>() ?? copywriterName,
          HistoryVariableName = "history",
          HistoryReducer = strategyReducer,
          EvaluateNameOnly = true,
      },
      TerminationStrategy = new KernelFunctionTerminationStrategy(terminationFunction, kernel)
      {
          Agents = [ agentReviewer ],
          ResultParser = (result) => result.GetValue<string>()?.Contains("yes", StringComparison.InvariantCultureIgnoreCase) ?? false,
          HistoryVariableName = "history",
          MaximumIterations = 10,
          HistoryReducer = strategyReducer,
          AutomaticReset = true,
      },
  }
};
      

var input = default(string);
var message = default(string);
while (true)
{
  Console.WriteLine("Hi, I'm your project manager today. What product do you have in mind advertising?");

  input = Console.ReadLine();

  if (string.IsNullOrWhiteSpace(input))
  {
      break;
  }

  Console.Write("Assistant: ");

  chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));

  var agentName = default(string);
  var isAgentChanged = false;
  var response = chat.InvokeStreamingAsync();
  await foreach (var content in response)
  {
      await Task.Delay(20);
      if (content.AuthorName?.Equals(agentName, StringComparison.InvariantCultureIgnoreCase) == false)
  {
      isAgentChanged = true;
      agentName = content.AuthorName;
      Console.WriteLine();
  }
  else
  {
      isAgentChanged = false;
  }

  message += isAgentChanged ? $"{content.AuthorName}: {content.Content}" : content.Content;
  Console.Write(isAgentChanged ? $"{content.AuthorName}: {content.Content}" : content.Content);
  }
  Console.WriteLine();

  Console.WriteLine();
}