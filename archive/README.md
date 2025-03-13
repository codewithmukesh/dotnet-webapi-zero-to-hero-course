# Mukesh's .NET Zero to Hero Series (.NET 8)

### The Ultimate .NET Zero to Hero Series for Web Developers!

The end game of this series is to build scalable **.NET applications with Clean Architecture**! I am building the content one by one to reach to the finale - _Practical Clean Architecture in ASP.NET Core_!

![.NET Zero to Hero Series](/assets/NET%20Zero%20to%20Hero%20Series%20Banner.png)

#### By Mukesh Murugan | [LinkedIn](https://www.linkedin.com/in/iammukeshm/) | [Blog](https://www.codewithmukesh.com) | [Youtube](https://www.youtube.com/@codewithmukesh?sub_confirmation=1)

### Join the Series along with 5,000+ other .NET Developers! 🎉 - [Subscribe](https://newsletter.codewithmukesh.com/subscribe)

### Subscribe to my YouTube Channel for .NET Content! 🎉 - [Subscribe](https://www.youtube.com/@codewithmukesh?sub_confirmation=1)

---

## Structured Logging with Serilog in ASP.NET Core

When building a new ASP.NET Core project, prioritizing the setup of logging is crucial for robust monitoring and debugging capabilities right from the start. **Serilog** is the most popular logging library for ASP.NET Core applications.

In this article, we will:

- Explore the importance of logging in ASP.NET Core projects.
- Learn everything you need to know to master structured logging using Serilog.

By the end of this tutorial, you will have a solid understanding of how to implement and utilize Serilog for effective structured logging in your ASP.NET Core application.

[Read Article](https://codewithmukesh.com/blog/structured-logging-with-serilog-in-aspnet-core/?utm_source=github&utm_medium=social&utm_campaign=repository)

## Global Exception Handling in ASP.NET Core

Exception handling is vital for applications of all types and traffic volumes. If exceptions are not managed well within the application, it can break the entire system or even lead to data loss. In ASP.NET Core, there are multiple ways to handle exceptions effectively.

In this article, we will discuss:

- **Try-Catch Blocks**
- **Default Exception Handling Middleware**
- **Custom Exception Middleware**
- **IExceptionHandler**

Starting from .NET 8, `IExceptionHandler` is the recommended and cleaner approach for handling exceptions. This method ensures a more robust and maintainable exception handling strategy for your applications.

[Read Article](https://codewithmukesh.com/blog/global-exception-handling-in-aspnet-core/?utm_source=github&utm_medium=social&utm_campaign=repository)

## FluentValidation in ASP.NET Core - Super Powerful Validations

When it comes to validating models and incoming HTTP requests, many of us tend to lean toward using **Data Annotations**. Although setting up Data Annotations on your models can be quick and easy, there are a few drawbacks to this approach, especially for larger projects.

In this article, we will:

- Understand the problems with using Data Annotations in larger projects.
- Explore an alternative approach using **FluentValidation** in our ASP.NET Core application.

**FluentValidation** can elevate your validation process, giving you total control and enabling you to write much cleaner and more maintainable code.

[Read Article](https://codewithmukesh.com/blog/fluentvalidation-in-aspnet-core/?utm_source=github&utm_medium=social&utm_campaign=repository)

## CQRS and MediatR in ASP.NET Core - Building Scalable Systems

**CQRS (Command Query Responsibility Segregation)** is a software architectural pattern that separates the read and write operations of a system into two distinct parts.

### Key Concepts of CQRS:

- **Write Operations (Commands)** and **Read Operations (Queries)** are handled separately.
- Different models are optimized for each type of operation.

This separation can lead to simpler and more scalable architectures, especially in complex systems where the read and write patterns differ significantly.

In this article, we will:

- Build an ASP.NET Core 8 Web API with CRUD functionalities.
- Implement the CQRS pattern.
- Use the MediatR library for managing commands and queries.

By the end of this tutorial, you'll have a deeper understanding of how to create clean, scalable .NET systems using CQRS.

[Read Article](https://codewithmukesh.com/blog/cqrs-and-mediatr-in-aspnet-core/?utm_source=github&utm_medium=social&utm_campaign=repository)

## Validation with MediatR Pipeline Behavior & FluentValidation

Validation is super essential for your application to ensure the sanity of the incoming requests. In this article, we will learn a clean way to implement validation in your ASP.NET Core projects using the MediatR and FluentValidation libraries. This is powerful in projects that has already adapted the CQRS / CQS patterns using MediatR.

We will learn about MediatR Pipeline Behavior in ASP.NET Core, the idea of Pipelines, and How to intersect the pipeline to add custom service, and most importantly the validation behavior using FluentValidation.

[Read Article](https://codewithmukesh.com/blog/validation-with-mediatr-pipeline-behavior-and-fluentvalidation/)
