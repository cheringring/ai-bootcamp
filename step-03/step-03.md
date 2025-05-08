# Step 03: AI 에이전트와 멀티 에이전트 시스템 구현하기

이 단계에서는 Semantic Kernel을 활용하여 단일 에이전트와 멀티 에이전트 시스템을 구현하는 방법을 학습합니다. 에이전트 프레임워크를 통해 더 복잡하고 지능적인 AI 시스템을 구축할 수 있습니다.

## 주요 개념 정리

### AI 에이전트(Agent)
에이전트는 특정 역할과 지시사항을 가진 AI 시스템으로, 플러그인과 LLM을 결합하여 특정 작업을 수행합니다. Semantic Kernel의 에이전트 프레임워크를 사용하면 에이전트의 역할, 행동 방식, 지식 등을 정의할 수 있습니다.

### 멀티 에이전트 시스템
여러 에이전트가 협업하여 복잡한 작업을 수행하는 시스템입니다. 각 에이전트는 특정 역할과 전문성을 가지고 있으며, 서로 상호작용하여 더 나은 결과를 도출합니다. 예를 들어, 프로젝트 매니저, 카피라이터, 마케터 등 여러 역할의 에이전트가 협업할 수 있습니다.

### 에이전트 그룹 채팅(AgentGroupChat)
여러 에이전트가 참여하는 대화 시스템으로, 각 에이전트가 차례로 대화에 참여하며 협업합니다. 선택 전략(SelectionStrategy)과 종료 전략(TerminationStrategy)을 통해 대화의 흐름과 종료 조건을 제어할 수 있습니다.

## 코드 리뷰

### 단일 에이전트 구현 (single-agent)

```csharp
var agent = new ChatCompletionAgent()
{
    Kernel = kernel,
    Arguments = new KernelArguments(settings),
    Instructions = "Answer questions about the menu.",
    Name = "Host",
};
```

- **ChatCompletionAgent**: 단일 에이전트를 생성하는 클래스
- **Kernel**: 에이전트가 사용할 Semantic Kernel 인스턴스
- **Arguments**: 에이전트에게 전달할 인자 (함수 호출 설정 포함)
- **Instructions**: 에이전트의 행동 방식을 정의하는 지시사항
- **Name**: 에이전트의 이름

```csharp
kernel.Plugins.AddFromType();
```

- 에이전트가 사용할 플러그인 등록 (메뉴 관련 기능 제공)

```csharp
var history = new ChatHistory();
history.AddSystemMessage("You're a friendly host at a restaurant. Always answer in Korean.");
```

- **ChatHistory**: 대화 기록을 관리하는 클래스
- **AddSystemMessage**: 에이전트에게 시스템 메시지로 추가 지시사항 제공

### 멀티 에이전트 구현 (multi-agent)

```csharp
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
```

- 프로젝트 매니저 역할의 에이전트 생성
- 상세한 지시사항을 통해 에이전트의 역할과 행동 방식 정의

```csharp
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
```

- 카피라이터 역할의 에이전트 생성
- 전문성과 스타일을 상세히 정의한 지시사항

```csharp
var terminationFunction =
    AgentGroupChat.CreatePromptFunctionForStrategy(
        """
        Determine if the copy has been approved. If so, respond with a single word: yes

        History:
        {{$history}}
        """,
        safeParameterNames: "history");
```

- **terminationFunction**: 대화 종료 조건을 정의하는 함수
- 카피가 승인되면 대화를 종료하도록 설정

```csharp
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
```

- **selectionFunction**: 다음 발언자를 선택하는 함수
- 카피라이터와 프로젝트 매니저가 번갈아가며 대화하도록 설정

```csharp
var chat = new AgentGroupChat(agentWriter, agentReviewer)
{
    ExecutionSettings = new AgentGroupChatSettings()
    {
        SelectionStrategy = new KernelFunctionSelectionStrategy(selectionFunction, kernel)
        {
            InitialAgent = agentWriter,
            ResultParser = (result) => result.GetValue() ?? copywriterName,
            HistoryVariableName = "history",
            HistoryReducer = strategyReducer,
            EvaluateNameOnly = true,
        },
        TerminationStrategy = new KernelFunctionTerminationStrategy(terminationFunction, kernel)
        {
            Agents = [ agentReviewer ],
            ResultParser = (result) => result.GetValue()?.Contains("yes", StringComparison.InvariantCultureIgnoreCase) ?? false,
            HistoryVariableName = "history",
            MaximumIterations = 10,
            HistoryReducer = strategyReducer,
            AutomaticReset = true,
        },
    }
};
```

- **AgentGroupChat**: 여러 에이전트가 참여하는 그룹 채팅 생성
- **SelectionStrategy**: 다음 발언자 선택 전략 설정
- **TerminationStrategy**: 대화 종료 조건 설정
- **InitialAgent**: 첫 번째 발언자 지정
- **MaximumIterations**: 최대 대화 반복 횟수 제한

### 스토리텔러 에이전트 구현 (storyteller.cs)

```csharp
var definition = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "../../..", "Plugins", "StoryTellerAgent", "StoryTeller.yaml"));
var template = KernelFunctionYaml.ToPromptTemplateConfig(definition);
var agent = new ChatCompletionAgent(template, new KernelPromptTemplateFactory())
            {
                Kernel = kernel
            };
```

- YAML 파일에서 에이전트 정의를 로드하여 스토리텔러 에이전트 생성
- 프롬프트 템플릿을 통해 에이전트의 행동 방식 정의

```csharp
var arguments = new KernelArguments()
{
    { "topic", input },
    { "length", 3 }
};

var response = agent.InvokeStreamingAsync(history, arguments);
```

- 에이전트에게 전달할 인자 설정 (주제와 길이)
- 스트리밍 방식으로 에이전트 응답 받기

## 에이전트 구현 과정

1. 에이전트 정의
   - 에이전트의 역할, 지시사항, 이름 등 설정
   - 필요한 플러그인 등록

2. 대화 기록 설정
   - 시스템 메시지를 통해 기본 지시사항 제공
   - 사용자 메시지 추가

3. 단일 에이전트 실행
   ```csharp
   var response = agent.InvokeStreamingAsync(history);
   ```

4. 멀티 에이전트 시스템 구성
   - 여러 에이전트 생성
   - 선택 전략과 종료 전략 설정
   - 에이전트 그룹 채팅 생성

5. 멀티 에이전트 실행
   ```csharp
   var response = chat.InvokeStreamingAsync();
   ```

## 주요 학습 포인트

1. **에이전트 설계**: 특정 역할과 전문성을 가진 에이전트 설계 방법
2. **지시사항 작성**: 에이전트의 행동 방식을 결정하는 상세한 지시사항 작성 방법
3. **멀티 에이전트 협업**: 여러 에이전트가 협업하여 복잡한 작업을 수행하는 시스템 구현 방법
4. **대화 흐름 제어**: 선택 전략과 종료 전략을 통한 대화 흐름 제어 방법

## 다음 단계

다음 단계에서는 벡터 검색(RAG)과 모니터링(Aspire Dashboard 및 OpenTelemetry)을 활용하여 AI 애플리케이션의 성능을 향상시키는 방법을 학습합니다.
