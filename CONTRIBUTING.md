# Contributing to TraceLogic

Thank you for your interest in contributing to TraceLogic! We truly appreciate community contributions. We welcome bug reports, feature requests, and pull requests.

## How to Contribute

### Reporting Bugs

If you find a bug, please open an issue on our GitHub repository. A well-defined bug report will help us fix it faster. Please include the following:
* A clear and descriptive title.
* A detailed description of the problem, including what you expected to happen and what actually happened.
* Clear steps to reproduce the bug.
* If possible, a sample `.trc` file that causes the issue (please ensure it contains no sensitive data).

### Suggesting Enhancements

If you have an idea for a new feature or an improvement to an existing one, please open an issue. Describe your idea clearly, including the problem it solves and why it would be valuable to other TraceLogic users.

## Getting Started with Development

Before you start writing code, you'll need to set up your development environment.

1.  Fork the repository on GitHub and then clone your fork locally:
    ```sh
    git clone [https://github.com/VerisFlow/TraceLogicLocal.git](https://github.com/VerisFlow/TraceLogicLocal.git)
    cd TraceLogicLocal
    ```
2.  Open the `TraceLogic.sln` solution file in Visual Studio 2022.
3.  Build the solution (from the menu `Build > Build Solution` or press `Ctrl+Shift+B`). This will automatically restore all the required NuGet packages.
4.  You can now run the project and are ready to create a new branch to start making your changes!

## Submitting Pull Requests

We welcome code contributions! To avoid spending time on work that might not be merged, it's a great idea to open an issue first to discuss any significant new features or architectural changes.

Please follow these steps to submit a contribution:

1.  **Fork the repository** and create a new branch from `main`. Give your branch a descriptive name (e.g., `feature/parse-error-events` or `fix/calculation-bug`).
2.  **Make your changes** in your new branch.
    * We follow the standard [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions). Using an IDE like Visual Studio with its default settings will generally ensure your code meets these standards.
    * Please ensure new code is well-commented, especially for complex logic.
3.  **Ensure the project builds** successfully and all existing functionality works as expected.
4.  **Submit a pull request** to the `main` branch of the original repository. Provide a clear title and a detailed description of the changes you have made and why you made them.

We will review your pull request as soon as possible and provide feedback. Thank you for your contribution!