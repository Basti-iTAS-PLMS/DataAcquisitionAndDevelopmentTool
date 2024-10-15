![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white)
![Python](https://img.shields.io/badge/python-3670A0?style=for-the-badge&logo=python&logoColor=ffdd54)
![InfluxDB](https://img.shields.io/badge/InfluxDB-22ADF6?style=for-the-badge&logo=InfluxDB&logoColor=white)
![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)
![Figma](https://img.shields.io/badge/figma-%23F24E1E.svg?style=for-the-badge&logo=figma&logoColor=white)
![Jira](https://img.shields.io/badge/jira-%230A0FFF.svg?style=for-the-badge&logo=jira&logoColor=white)
![Visual Studio](https://img.shields.io/badge/Visual%20Studio-5C2D91.svg?style=for-the-badge&logo=visual-studio&logoColor=white)
![Emacs](https://img.shields.io/badge/Emacs-%237F5AB6.svg?&style=for-the-badge&logo=gnu-emacs&logoColor=white)

# Overview 
*Data Acquisition and Development Tool* 

This Project aims to configure and monitor sensors as well as handle data
storage, for the research project "Smart Pacifier" by Piro, Neltje
Emma and Moosbauer, Sebastian.

# Table of Contents
- [Installation](#installation)
- [Setup](#setup)
- [Documentation](#documentation)
- [Project Structure](#project-structure)
- <details><summary> <a href="#style-guide">Style Guide</a> </summary>

	- [Branching Strategy](#branch-strategy)
	- [Indentation](#indentation)
	- [Naming Convention](#naming-convention)
	- [Braces](#braces)
	- [Line Length](#line-length)
	- [Comments](#comments)
	- [Example](#example)
</details>

# Installation
*TODO*

# Setup


# Documentation
For Documentation see: [Documentation](/Documentation)

# Project Structure
We have 4 individual projects:
- Smart Pacifier: The Frontent Project
- BackEndService: Providing access to Sensor data, DataBase & MQTT
  Protocoll
- AlogirthmLayer: Providing the capabilities for implementing
  Algorithms
- Interface: Providing the Interfaces to the FrontEnd/AlogrithmLayer

The "Main Project" is the Smart Pacifcier Project, that Sets up all
other projects.

# Style Guide

## Branching Strategy
We are following the Feature Branching strategy.

Features are implemented separately in a feature branch additionally personal
branches may be created.

- `Staging-Environment` branch: This branch contains the uptodate source code
- `main-Production-Environment` branch: This branch contains the Deployment
  ready source code after all Test were pased. This contains the current
  protoype where not all features may be implemented
  
### Feature Branching Workflow

1. Create `feature` branches: Create a new branch for each feature or task
   you’re working on. This branch should be created from the
   `Staging-Environment` branch. The branch must be named as follows:
   `feature-{JIRA TASK Number}-{FEATURE-TITLE}`
2. Work on the feature: After creating the `feature` branch, you can start
   implementing the new feature by making as many commits as necessary. The
   branch should only contain changes relating to that particular feature.
3. Create a pull request: When you’re finished working on the `feature` branch,
   you create a pull request to merge the changes into the main branch.
4. Review and approve: Other developers review the changes in the pull request
   and approve them if they are satisfied with the changes. Code review can help
   catch issues or mistakes before they are merged into the main branch.
5. Merge the `feature` branch: Once you’re done working on the feature, you can
   merge the `feature` branch back into the `Staging-Environment` branch.
6. Clean up: After merging, you can delete the `feature` branch, as it is no
   longer needed.

## Indentation
Use 4 spaces for indentation
	
We use spaces for indentation. Do not use tabs in your code. You should set your
editor to emit spaces when you hit the tab key.

## Naming Convention
- Classes: PascalCase (e.g., `MyClass`)
- Methods: PascalCase (e.g., `MyMethod`)
- Variables: camelCase (e.g.,`myVariable`)
- Private Member Variables: _startingWithAUnderscore (e.g., `_privateVariable`)
- Constants: UPPERCASE_WITH_UNDERSCORES (e.g.,`MAX_VALUE`)

## Braces
Each Braces goes in a separate line, starting after the statement.

## Line Length
Each line of text in your code should be at most 80 characters long.

A line may exceed 80 characters if it is:

- a comment line which is not feasible to split without harming readability,
  ease of cut and paste or auto-linking -- e.g., if a line contains an example
  command or a literal URL longer than 80 characters.
- a string literal that cannot easily be wrapped at 80 columns. This may be
  because it contains URIs or other semantically-critical pieces, or because the
  literal contains an embedded language, or a multiline literal whose newlines
  are significant like help messages. In these cases, breaking up the literal
  would reduce readability, searchability, ability to click links, etc. 
- an include statement.

## Comments
- Purpose: Explain the intent of code that is not immediately obvious.
- Style: Use clear and concise language.
- Types:
  - Single-line comments: Use //.
  - Multi-line comments: Use /* */.
  - XML comments: Use /// for documentation.
  
## Example
```c#
// Good code example
/// <summary>
/// Represents a backend service that creates and handles a database.
/// </summary>
public class BackendService
{
    public const string DATABASE_URL = "url";
    /// <summary>
    /// The underlying database connection.
    /// </summary>
    private readonly IDatabaseConnection _databaseConnection;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackendService"/> class.
    /// </summary>
    /// <param name="databaseConnection">The database connection to use.</param>
    public BackendService(IDatabaseConnection databaseConnection)
    {
        _databaseConnection = databaseConnection;
    }
}

// Bad code example
public class BackendService
{
	public const string DATABASE_URL = "url";
	
	/* Database Connection */ 
    private readonly IDatabaseConnection databaseConnection;

    public BackendService(IDatabaseConnection databaseConnection){
        databaseConnection = databaseConnection;
    }
}
```
