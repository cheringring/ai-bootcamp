# Step 02: 플러그인과 에이전트 구현하기

이 단계에서는 Semantic Kernel의 플러그인 기능을 활용하여 AI 모델이 외부 기능을 실행할 수 있도록 구현하는 방법을 학습합니다.
<br>

## 주요 개념 정리

### 플러그인(Plugin)
플러그인은 AI가 외부 기능을 실행할 수 있게 해주는 강력한 개념입니다. 여러 함수를 묶은 클래스로 구성되며, 최신 LLM들은 함수 호출 기능을 통해 플러그인을 직접 호출할 수 있습니다. Kernel은 함수 실행 결과를 다시 모델에게 전달하여 최종 응답을 생성합니다.

### 에이전트(Agent)
에이전트는 특정 목적을 가진 AI 시스템으로, 플러그인과 LLM을 결합하여 특정 작업을 수행합니다. 멀티 에이전트 시스템에서는 여러 에이전트가 각기 다른 도구와 LLM을 사용해 협업하는 구조를 구현할 수 있습니다.

### 멀티 에이전트 오케스트레이션
멀티 에이전트 구성의 핵심은 역할별로 최적화된 리소스를 활용하는 것입니다. 하나의 거대한 모델에 모든 일을 시키기보다, 정보검색에는 검색 특화 도구를, 데이터 조회에는 벡터DB를, 콘텐츠 생성에는 LLM을, 검증에는 또 다른 LLM이나 규칙 기반 검증기를 사용하는 방식으로 구현합니다.

## 코드 리뷰

### Program.cs 분석

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Workshop.ConsoleApp.Plugins.BookingAgent;
```

- **Microsoft.SemanticKernel**: Semantic Kernel의 핵심 기능을 제공하는 네임스페이스
- **Microsoft.SemanticKernel.ChatCompletion**: 채팅 완성 기능을 제공하는 네임스페이스
- **Workshop.ConsoleApp.Plugins.BookingAgent**: 기차 예약 플러그인 네임스페이스

```csharp
var kernel = builder.Build();
kernel.Plugins.AddFromType("TrainBooking");
```

- **kernel.Plugins.AddFromType**: 플러그인 클래스를 커널에 등록하는 메서드
- **TrainBookingPlugin**: 기차 예약 기능을 제공하는 플러그인 클래스
- **"TrainBooking"**: 플러그인의 이름 (AI가 이 이름으로 플러그인을 참조)

```csharp
var settings = new PromptExecutionSettings()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};
```

- **PromptExecutionSettings**: 프롬프트 실행 관련 설정
- **FunctionChoiceBehavior.Auto()**: AI가 자동으로 적절한 함수를 선택하도록 설정

```csharp
var history = new ChatHistory();
history.AddSystemMessage("The year is 2025 and the current month is February");
```

- **ChatHistory**: 채팅 기록을 관리하는 클래스
- **AddSystemMessage**: 시스템 메시지를 추가하여 AI에게 컨텍스트 제공

### TrainBookingPlugin.cs 분석

```csharp
[KernelFunction("search_trains")]
[Description("Searches for available trains based on the destination and departure date in the format YYYY-MM-DD")]
[return: Description("A list of available trains")]
public List SearchTrains(string destination, string departureDate)
```

- **KernelFunction**: 이 메서드를 Kernel 함수로 등록하는 어트리뷰트
- **Description**: 함수의 목적과 매개변수를 설명하는 어트리뷰트 (AI가 이 설명을 이해하고 적절히 함수를 호출)
- **return: Description**: 반환값에 대한 설명

```csharp
[KernelFunction("book_train")]
[Description("Books a train based on the train ID provided")]
[return: Description("Booking confirmation message")]
public string BookTrain(int trainId)
```

- **book_train**: AI가 호출할 수 있는 함수 이름
- 이 함수는 trainId를 받아 해당 기차를 예약하고 확인 메시지를 반환

## 플러그인 구현 과정

1. 플러그인 클래스 생성
   - 특정 기능을 수행하는 메서드들을 포함한 클래스 작성
   - 각 메서드에 `[KernelFunction]`과 `[Description]` 어트리뷰트 추가

2. 데이터 모델 정의
   - 플러그인이 처리할 데이터 모델 클래스 정의 (예: TrainModel)
   - 필요한 속성과 기본값 설정

3. 커널에 플러그인 등록
   ```csharp
   kernel.Plugins.AddFromType<TrainBookingPlugin>("TrainBooking");
   ```

4. 채팅 기록 설정 및 시스템 메시지 추가
   ```csharp
   var history = new ChatHistory();
   history.AddSystemMessage("The year is 2025 and the current month is February");
   ```

5. 채팅 완성 서비스 설정
   ```csharp
   var service = kernel.GetRequiredService<IChatCompletionService>();
   ```

## 멀티 에이전트 시나리오 예시

1. **사용자 질문 입력**: 프론트엔드에서 사용자 질문을 받음
2. **Researcher 에이전트**: 웹 검색 플러그인을 활용해 필요한 정보를 검색하고 요약
3. **Marketing 에이전트**: 벡터 DB를 사용하여 인덱싱된 제품 데이터 조회
4. **Writer 에이전트**: 사용자 지시, 리서칭, 마케팅 결과를 모아 LLM을 사용해 초안 작성
5. **Editor 에이전트**: 작성된 초안을 검토 및 수정하여 최종 결과 도출

## 주요 학습 포인트

1. **플러그인 개발**: AI가 외부 기능을 호출할 수 있도록 플러그인을 개발하는 방법
2. **함수 설명 작성**: AI가 함수의 목적과 사용법을 이해할 수 있도록 적절한 설명 작성하기
3. **채팅 기록 관리**: 대화 컨텍스트를 유지하기 위한 채팅 기록 관리 방법
4. **멀티 에이전트 구성**: 여러 에이전트가 협업하여 복잡한 작업을 수행하는 구조 이해하기

## 다음 단계

다음 단계에서는 벡터 검색(RAG)과 모니터링(Aspire Dashboard 및 OpenTelemetry)을 활용하여 AI 애플리케이션의 성능을 향상시키는 방법을 학습합니다.
