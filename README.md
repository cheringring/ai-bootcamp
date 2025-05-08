
# AI Bootcamp: Semantic Kernel과 AI 오케스트레이션 실습

> 📝 **이 레포지토리는 [Semantic Kernel 과 AI 오케스트레이션 : 실습 정리](https://velog.io/@cheringring/Semantic-Kernel-%EA%B3%BC-AI-%EC%98%A4%EC%BC%80%EC%8A%A4%ED%8A%B8%EB%A0%88%EC%9D%B4%EC%85%98-%EC%8B%A4%EC%8A%B5-%EC%A0%95%EB%A6%AC) 벨로그 포스팅을 기반으로 작성되었습니다.**

이 레포지토리는 Semantic Kernel을 활용한 AI 오케스트레이션 실습 내용을 담고 있습니다. 각 단계별로 코드와 설명을 확인할 수 있습니다.

## [Step 01: Semantic Kernel과 AI 모델 연결하기](https://github.com/cheringring/ai-bootcamp/tree/main/step-01)

- **Semantic Kernel 개념 설명**: Microsoft에서 개발한 오픈소스 프레임워크로, 다양한 AI 모델과 도구들을 유기적으로 결합하여 AI 오케스트레이션을 구현
- **AI 오케스트레이션 정의**: 여러 AI 모델과 도구를 조합하여 복잡한 작업을 수행하는 과정
- **Google Gemini와 GitHub Models(Azure OpenAI) 연결 코드 분석**: 다양한 AI 모델을 Semantic Kernel에 연결하는 방법
- **다중 AI 모델 연결 및 스트리밍 응답 처리 방법 설명**: 여러 AI 모델을 동시에 활용하고 실시간으로 응답을 처리하는 방법

## [Step 02: 플러그인과 에이전트 구현하기](https://github.com/cheringring/ai-bootcamp/tree/main/step-02)

- **플러그인과 에이전트 개념 설명**: AI가 외부 기능을 실행할 수 있게 해주는 플러그인과 특정 목적을 가진 AI 시스템인 에이전트
- **멀티 에이전트 오케스트레이션 개념 설명**: 역할별로 최적화된 리소스를 활용하여 여러 에이전트가 협업하는 구조
- **TrainBookingPlugin 코드 분석**: 기차 예약 기능을 제공하는 플러그인 구현 방법
- **플러그인 구현 과정 및 함수 호출 방법 설명**: AI가 플러그인의 함수를 호출하고 결과를 활용하는 과정

## [Step 03: AI 에이전트와 멀티 에이전트 시스템 구현하기](https://github.com/cheringring/ai-bootcamp/tree/main/step-03)

- **단일 에이전트와 멀티 에이전트 시스템 설명**: 하나의 에이전트와 여러 에이전트가 협업하는 시스템의 차이점
- **에이전트 그룹 채팅 개념 설명**: 여러 에이전트가 참여하는 대화 시스템 구현 방법
- **프로젝트 매니저와 카피라이터 에이전트 구현 코드 분석**: 각 역할에 맞는 에이전트 설계 및 구현 방법
- **에이전트 간 대화 흐름 제어 방법 설명**: 선택 전략과 종료 전략을 통한 대화 흐름 제어 방법

## [Step 04: 벡터 검색(RAG)과 모니터링 구현하기](https://github.com/cheringring/ai-bootcamp/tree/main/step-04)

- **RAG(Retrieval Augmented Generation) 개념 설명**: AI 모델에게 추가적인 자료를 제공하여 응답의 정확성과 관련성을 높이는 기술
- **벡터 데이터베이스와 비정형 데이터 처리 방법**: 텍스트, 이미지, 오디오 등 다양한 데이터를 벡터로 변환하여 저장하고 검색하는 방법
- **OpenTelemetry와 Aspire Dashboard를 활용한 모니터링 구현**: 애플리케이션의 성능과 동작을 모니터링하기 위한 프레임워크 활용 방법
- **벡터 검색과 모니터링 코드 분석**: 벡터 검색 서비스 구현 및 모니터링 설정 방법

## 관련 자료

- [Semantic Kernel 과 AI 오케스트레이션 : 실습 정리](https://velog.io/@cheringring/Semantic-Kernel-%EA%B3%BC-AI-%EC%98%A4%EC%BC%80%EC%8A%A4%ED%8A%B8%EB%A0%88%EC%9D%B4%EC%85%98-%EC%8B%A4%EC%8A%B5-%EC%A0%95%EB%A6%AC) - 본인 벨로그 
- [Semantic Kernel 공식 문서](https://learn.microsoft.com/ko-kr/semantic-kernel/overview/)
- [Global AI Bootcamp](https://globalai.community/bootcamp/)

## 기술 스택

- .NET SDK 9.0 이상
- Semantic Kernel
- Google Gemini API
- Azure OpenAI / GitHub Models
- OpenTelemetry
- .NET Aspire


## 라이센스

MIT
