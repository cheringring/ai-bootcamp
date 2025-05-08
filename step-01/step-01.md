# Step 01: Semantic Kernel과 AI 모델 연결하기

이 단계에서는 Semantic Kernel을 활용하여 Google Gemini와 GitHub Models(Azure OpenAI)에 연결하는 방법을 학습합니다.

## 주요 개념 정리

### Semantic Kernel
Semantic Kernel은 다양한 AI 모델과 도구들을 유기적으로 결합하여 AI 오케스트레이션을 구현하는 오픈소스 프레임워크입니다. Microsoft에서 개발한 이 프레임워크는 여러 종류의 AI 모델을 손쉽게 교체하거나 동시에 활용할 수 있는 장점이 있습니다.

### AI 오케스트레이션
여러 AI 모델과 도구를 조합하여 복잡한 작업을 수행하는 과정을 말합니다. Semantic Kernel은 이러한 오케스트레이션을 쉽게 구현할 수 있도록 도와줍니다.

## 코드 리뷰

### Program.cs 분석

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using OpenAI;
using System.ClientModel;
```

- **Microsoft.Extensions.Configuration**: 애플리케이션 설정을 관리하는 라이브러리
- **Microsoft.SemanticKernel**: Semantic Kernel의 핵심 기능을 제공하는 네임스페이스
- **OpenAI**: OpenAI API 연결을 위한 클라이언트 라이브러리
- **System.ClientModel**: API 인증 관련 기능 제공

```csharp
var config = new ConfigurationBuilder()
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
             .AddUserSecrets()
             .Build();
```

- **ConfigurationBuilder**: 애플리케이션 설정을 구성하는 빌더 패턴
- **AddJsonFile**: appsettings.json 파일에서 설정 로드
- **AddUserSecrets**: 로컬 환경의 비밀 저장소에서 API 키 등의 민감한 정보 로드
- **Build**: 설정 객체 생성

```csharp
var client = new OpenAIClient(
    credential: new ApiKeyCredential(config["GitHub:Models:AccessToken"]!),
    options: new OpenAIClientOptions { Endpoint = new Uri(config["GitHub:Models:Endpoint"]!) });
```

- **OpenAIClient**: OpenAI API에 접근하기 위한 클라이언트
- **ApiKeyCredential**: API 키 기반 인증 정보
- **OpenAIClientOptions**: API 엔드포인트 등 클라이언트 옵션 설정

```csharp
var kernel = Kernel.CreateBuilder()
               .AddGoogleAIGeminiChatCompletion(
                    modelId: config["Google:Gemini:ModelName"]!,
                    apiKey: config["Google:Gemini:ApiKey"]!,
                    serviceId: "google")
               .Build();
```

- **Kernel.CreateBuilder()**: Semantic Kernel 인스턴스 생성을 위한 빌더 패턴
- **AddGoogleAIGeminiChatCompletion**: Google Gemini 모델 연결 설정
- **modelId**: 사용할 Gemini 모델 ID
- **apiKey**: Gemini API 키
- **serviceId**: 서비스 식별자 (여러 모델 구분에 사용)

```csharp
var responseGoogle = kernel.InvokePromptStreamingAsync(
        promptTemplate: input,
        arguments: new KernelArguments(new PromptExecutionSettings() { ServiceId = "google" }));
```

- **InvokePromptStreamingAsync**: 스트리밍 방식으로 프롬프트 실행
- **promptTemplate**: 사용자 입력을 프롬프트로 사용
- **KernelArguments**: 프롬프트 실행 시 필요한 인자 설정
- **PromptExecutionSettings**: 프롬프트 실행 관련 설정 (어떤 서비스를 사용할지 지정)

## 실습 과정

1. Semantic Kernel SDK 추가하기
   ```bash
   dotnet add ./Workshop.ConsoleApp package Microsoft.SemanticKernel
   ```

2. Google Gemini 연결을 위한 커넥터 추가
   ```bash
   dotnet add ./Workshop.ConsoleApp package Microsoft.SemanticKernel.Connectors.Google --prerelease
   ```

3. API 키 설정 (user-secrets 사용)
   ```bash
   dotnet user-secrets --project ./Workshop.ConsoleApp/ set Google:Gemini:ApiKey {{Google Gemini API Key}}
   ```

4. GitHub Models(Azure OpenAI) 연결을 위한 API 키 설정
   ```bash
   dotnet user-secrets --project ./Workshop.ConsoleApp/ set GitHub:Models:AccessToken {{GitHub Models Access Token}}
   ```

## 주요 학습 포인트

1. **다중 AI 모델 연결**: Semantic Kernel을 통해 여러 AI 모델(Google Gemini, GitHub Models)을 동시에 연결하고 활용하는 방법
2. **스트리밍 응답 처리**: `InvokePromptStreamingAsync`를 통해 AI 모델의 응답을 실시간으로 스트리밍하는 방법
3. **보안 관리**: API 키와 같은 민감한 정보를 `user-secrets`를 통해 안전하게 관리하는 방법

## 다음 단계

다음 단계에서는 플러그인과 에이전트를 활용하여 AI 모델의 기능을 확장하는 방법을 학습합니다.
