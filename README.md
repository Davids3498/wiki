# Wikipedia Playwright Automation

[![tests](https://github.com/Davids3498/wiki/actions/workflows/tests.yml/badge.svg)](https://github.com/Davids3498/wiki/actions/workflows/tests.yml)

C# + Playwright automation framework that validates the **Debugging features** section of the [Playwright (software) Wikipedia article](https://en.wikipedia.org/wiki/Playwright_(software)).

Built with NUnit, Microsoft.Playwright, and Allure for reporting.

---

## Prerequisites

| Tool | Version |
| --- | --- |
| .NET SDK | 9.0 (LTS) |
| Node.js | Only required by the Playwright CLI via `pwsh` below; auto-installed browsers handle the rest |
| Allure CLI | 2.x (only to render HTML reports) |


Install Allure CLI (optional — required only to render the HTML report):

```bash
# macOS / Linux
brew install allure
# Windows
scoop install allure
```

## Project layout

```
.
├── Config/               # TestSettings DTO + appsettings.json (base URL, browser, API)
├── Models/               # DTOs for Wiki Parse API responses
├── Helpers/              # Text normalizer, HTML stripper
│   └── Api/              # HttpClient-based Wikipedia Parse API client
├── Pages/                # POM (constructor injection of IPage)
│   ├── BasePage.cs
│   ├── WikipediaArticlePage.cs
│   └── AppearancePanelComponent.cs
├── Tests/                          # WikipediaAutomation project — the 3 Playwright task tests
│   ├── Base/PlaywrightTestBase.cs  # fixture + context + screenshot/trace on failure
│   ├── Support/TestAttachments.cs  # helper to attach intermediate state to Allure
│   ├── DebuggingFeaturesParityTests.cs         # Task 1
│   ├── MicrosoftDevelopmentToolsLinksTests.cs  # Task 2
│   └── AppearancePanelDarkModeTests.cs         # Task 3
├── Tests.Unit/                     # WikipediaAutomation.Tests.Unit project — browser-less helper tests
│   ├── TextNormalizerTests.cs
│   ├── HtmlStripperTests.cs
│   └── WikipediaAutomation.Tests.Unit.csproj
├── .github/workflows/tests.yml  # CI
├── allureConfig.json
├── .runsettings
├── WikipediaAutomation.csproj   # main (Playwright) test project
└── WikipediaAutomation.sln      # ties both test projects together
```

## Setup

```bash
# Restore NuGet packages
dotnet restore

# Install Playwright's browser binaries (Chromium by default)
dotnet build
pwsh bin/Debug/net9.0/playwright.ps1 install chromium --with-deps
```

If you don't have `pwsh`, you can install it from [the PowerShell docs](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) or run `dotnet tool install --global Microsoft.Playwright.CLI` and use `playwright install chromium`.

## Run the tests

The solution contains **two test projects**: the main `WikipediaAutomation` project holds the three Playwright-based assignment tests, and the sibling `WikipediaAutomation.Tests.Unit` project holds fast browser-less tests for the helper classes.

```bash
# Run only the three assignment tests (Task 1, 2, 3 — launches a browser)
dotnet test WikipediaAutomation.csproj

# Run only the helper unit tests (fast, no browser)
dotnet test Tests.Unit/WikipediaAutomation.Tests.Unit.csproj

# Run both test projects at once
dotnet test

# Run a single fixture from the main project
dotnet test WikipediaAutomation.csproj --filter "FullyQualifiedName~DebuggingFeaturesParityTests"
dotnet test WikipediaAutomation.csproj --filter "FullyQualifiedName~MicrosoftDevelopmentToolsLinksTests"
dotnet test WikipediaAutomation.csproj --filter "FullyQualifiedName~AppearancePanelDarkModeTests"

# Run head-full (override the headless flag for local debugging)
WA_HEADLESS=false dotnet test WikipediaAutomation.csproj
```

### Configuration overrides

`Config/appsettings.json` holds base URL, browser, viewport, timeouts, API user-agent, and tracing. Any field can be overridden with environment variables prefixed `WA_`, e.g. `WA_TestSettings__Browser__Headless=false`. For persistent local overrides, create `Config/appsettings.Local.json` (gitignored).

## Generate the Allure report

```bash
# After running the tests, raw Allure results land in ./allure-results/
allure generate allure-results --clean -o allure-report
allure open allure-report
```

