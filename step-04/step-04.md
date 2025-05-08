# Step 04: 벡터 검색(RAG)과 모니터링 구현하기

이 단계에서는 Semantic Kernel을 활용하여 벡터 검색(RAG, Retrieval Augmented Generation)과 모니터링(Aspire Dashboard 및 OpenTelemetry)을 구현하는 방법을 학습합니다.

## 주요 개념 정리

### RAG(Retrieval Augmented Generation)
검색 증강 생성은 AI 모델에게 추가적인 자료를 제공하여 응답의 정확성과 관련성을 높이는 기술입니다. RAG의 주요 장점은 다음과 같습니다:
- **최신 데이터 적용**: 학습 데이터 이후의 최신 정보를 AI에게 제공할 수 있습니다.
- **보안 및 기밀 유지**: 민감한 정보를 직접 모델에 학습시키지 않고 필요할 때만 검색하여 제공합니다.
- **정확성 향상**: 특정 도메인의 정보를 벡터 데이터베이스에 저장하고 검색하여 정확한 응답을 생성합니다.

### 벡터 데이터베이스
텍스트, 이미지, 오디오 등 다양한 데이터를 벡터(수치 배열)로 변환하여 저장하고 검색할 수 있는 데이터베이스입니다. 주요 벡터 데이터베이스로는 Azure Cosmos DB, Redis, MongoDB, Elasticsearch 등이 있습니다.

### 비정형 데이터 처리
벡터 검색은 다음과 같은 비정형 데이터를 효과적으로 처리할 수 있습니다:
- 텍스트 문서
- 이미지
- 동영상
- 코드 등

### OpenTelemetry
애플리케이션의 성능과 동작을 모니터링하기 위한 오픈소스 관측성 프레임워크입니다. 트레이스(Trace), 메트릭(Metric), 로그(Log)를 수집하여 시스템의 상태를 파악할 수 있습니다.

### Aspire Dashboard
.NET Aspire의 모니터링 대시보드로, OpenTelemetry를 통해 수집된 데이터를 시각화하여 애플리케이션의 성능과 상태를 모니터링할 수 있습니다.

## 코드 리뷰

### 벡터 검색 구현 (vector.cs)

```csharp
var service = new TextSearchService(config);
var collection = await service.GetVectorStoreRecordCollectionAsync("records");
var search = await service.GetVectorStoreTextSearchAsync(collection);
```

- **TextSearchService**: 벡터 검색 서비스를 제공하는 클래스
- **GetVectorStoreRecordCollectionAsync**: 벡터 저장소에서 레코드 컬렉션을 가져오는 메서드
- **GetVectorStoreTextSearchAsync**: 텍스트 검색 기능을 제공하는 객체를 생성하는 메서드

```csharp
var searchResponse = await search.GetTextSearchResultsAsync(input, new TextSearchOptions() { Top = 2, Skip = 0 });
```

- **GetTextSearchResultsAsync**: 사용자 입력을 기반으로 벡터 검색을 수행하는 메서드
- **TextSearchOptions**: 검색 옵션 설정 (상위 2개 결과 반환, 건너뛰기 없음)

![image](https://github.com/user-attachments/assets/9f0308ec-804f-4d76-bf06-5d730d29117b)


### 함수 호출과 벡터 검색 통합 (vector-call.cs)

```csharp
var settings = new PromptExecutionSettings()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};
```

- **PromptExecutionSettings**: 프롬프트 실행 관련 설정
- **FunctionChoiceBehavior.Auto()**: AI가 자동으로 적절한 함수를 선택하도록 설정

```csharp
var functionCallingArguments = new KernelArguments(settings);

var functionCalingResponse = kernel.InvokePromptStreamingAsync(
    promptTemplate: input,
    arguments: functionCallingArguments);
```

- 벡터 검색 결과를 표시한 후, 함수 호출 기능을 통해 AI 모델의 응답을 생성
- 사용자 입력과 벡터 검색 결과를 함께 고려하여 더 정확한 응답 생성

![image](https://github.com/user-attachments/assets/30e945ce-7362-4d2f-b097-c6446ab18bdd)
![image](https://github.com/user-attachments/assets/f343a8ff-ad8d-49a9-acf5-a2e399d097f8)


### 모니터링 구현 (monitor.cs)

```csharp
var resourceBuilder = ResourceBuilder.CreateDefault()
                                     .AddService("SKOpenTelemetry");

// Enable model diagnostics with sensitive data.
AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);
```

- **ResourceBuilder**: OpenTelemetry 리소스 설정
- **EnableOTelDiagnosticsSensitive**: 민감한 데이터를 포함한 모델 진단 활성화

```csharp
using var traceProvider = Sdk.CreateTracerProviderBuilder()
                             .SetResourceBuilder(resourceBuilder)
                             .AddSource("Microsoft.SemanticKernel*")
                             .AddConsoleExporter()
                             .AddOtlpExporter(options => options.Endpoint = new Uri(dashboardEndpoint))
                             .Build();
```

- **TracerProviderBuilder**: 트레이스 제공자 설정
- **AddSource**: Semantic Kernel 관련 트레이스 소스 추가
- **AddConsoleExporter**: 콘솔에 트레이스 출력
- **AddOtlpExporter**: Aspire Dashboard로 트레이스 전송

```csharp
using var meterProvider = Sdk.CreateMeterProviderBuilder()
                             .SetResourceBuilder(resourceBuilder)
                             .AddMeter("Microsoft.SemanticKernel*")
                             .AddConsoleExporter()
                             .AddOtlpExporter(options => options.Endpoint = new Uri(dashboardEndpoint))
                             .Build();
```

- **MeterProviderBuilder**: 메트릭 제공자 설정
- **AddMeter**: Semantic Kernel 관련 메트릭 추가

```csharp
using var loggerFactory = LoggerFactory.Create(builder =>
{
    // Add OpenTelemetry as a logging provider
    builder.AddOpenTelemetry(options =>
    {
        options.SetResourceBuilder(resourceBuilder);
        options.AddConsoleExporter();
        options.AddOtlpExporter(options => options.Endpoint = new Uri(dashboardEndpoint));
        // Format log messages. This is default to false.
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
    });
    builder.SetMinimumLevel(LogLevel.Information);
});
```

- **LoggerFactory**: 로깅 제공자 설정
- **AddOpenTelemetry**: OpenTelemetry를 로깅 제공자로 추가
- **IncludeFormattedMessage**: 형식화된 로그 메시지 포함
- **IncludeScopes**: 로그 스코프 포함

## RAG 구현 과정

1. 벡터 검색 서비스 설정
   ```csharp
   var service = new TextSearchService(config);
   var collection = await service.GetVectorStoreRecordCollectionAsync("records");
   var search = await service.GetVectorStoreTextSearchAsync(collection);
   ```

2. 사용자 입력을 기반으로 벡터 검색 수행
   ```csharp
   var searchResponse = await search.GetTextSearchResultsAsync(input, new TextSearchOptions() { Top = 2, Skip = 0 });
   ```

3. 검색 결과 표시
   ```csharp
   await foreach (var result in searchResponse.Results)
   {
       Console.WriteLine($"Name:  {result.Name}");
       Console.WriteLine($"Value: {result.Value}");
       Console.WriteLine($"Link:  {result.Link}");
       Console.WriteLine();
   }
   ```

4. 검색 결과를 고려한 AI 응답 생성
   ```csharp
   var functionCalingResponse = kernel.InvokePromptStreamingAsync(
       promptTemplate: input,
       arguments: functionCallingArguments);
   ```

## 모니터링 구현 과정

1. OpenTelemetry 리소스 설정
   ```csharp
   var resourceBuilder = ResourceBuilder.CreateDefault()
                                        .AddService("SKOpenTelemetry");
   ```

2. 트레이스, 메트릭, 로그 제공자 설정
   - 트레이스 제공자 설정
   - 메트릭 제공자 설정
   - 로깅 제공자 설정

3. Semantic Kernel에 로거 팩토리 등록
   ```csharp
   builder.Services.AddSingleton(loggerFactory);
   ```

4. Aspire Dashboard로 데이터 전송
   ```csharp
   .AddOtlpExporter(options => options.Endpoint = new Uri(dashboardEndpoint))
   ```

## 주요 학습 포인트

1. **RAG 구현**: 벡터 검색을 통해 AI 모델에게 추가적인 정보를 제공하는 방법
2. **벡터 데이터베이스 활용**: 비정형 데이터를 벡터로 변환하여 저장하고 검색하는 방법
3. **OpenTelemetry 설정**: 애플리케이션의 성능과 동작을 모니터링하기 위한 트레이스, 메트릭, 로그 설정 방법
4. **Aspire Dashboard 연동**: 수집된 모니터링 데이터를 Aspire Dashboard로 전송하여 시각화하는 방법

## 활용 사례

1. **기업 내부 지식베이스 검색**: 회사 내부 문서, 정책, 매뉴얼 등을 벡터 데이터베이스에 저장하고 검색하여 정확한 정보 제공
2. **개인화된 추천 시스템**: 사용자의 선호도와 유사한 콘텐츠를 벡터 검색을 통해 추천
3. **코드 검색 및 자동 완성**: 코드베이스를 벡터화하여 관련 코드 검색 및 자동 완성 기능 구현
4. **실시간 성능 모니터링**: AI 시스템의 응답 시간, 정확도, 리소스 사용량 등을 실시간으로 모니터링
