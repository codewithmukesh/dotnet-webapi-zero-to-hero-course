# .NET Web API Zero to Hero - The Ultimate .NET Course for Web Developers!
> by Mukesh Murugan - [codewithmukesh.com](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)
>

Course Landing Page: [https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)

Complete YouTube Playlist: [https://youtube.com/playlist?list=PLigUNuP4l_bMl6PaucIB5A_eE5_gJTSY3](https://youtube.com/playlist?list=PLigUNuP4l_bMl6PaucIB5A_eE5_gJTSY3)

# Course Syllabus

# Chapter 1: Getting Started with .NET Web API

- REST Principles for .NET Developers – Understanding RESTful architecture and best practices.  
- Middleware & Request Pipeline – How ASP.NET Core processes incoming requests.  
- Dependency Injection (DI) in ASP.NET Core – Deep dive into DI and its benefits.  
- Service Lifetimes – Differences between Transient, Scoped, and Singleton lifetimes.  
- Keyed Services in .NET – Advanced DI techniques using keyed services.  
- Scrutor – Auto-registering dependencies for cleaner DI management.  
- ProblemDetails in ASP.NET Core – Standardizing error responses in APIs.  
- Global Exception Handling – Centralized error handling for better maintainability.  
- Structured Logging with Serilog – Implementing structured logging in .NET APIs.  
- Minimal API Endpoints – Understanding and using Minimal APIs in ASP.NET Core.  
- API Documentation – Generating OpenAPI docs and exploring Swagger alternatives.  

# Chapter 2: Database Management with Entity Framework Core

- CRUD with EF Core – Implementing basic CRUD operations in ASP.NET Core 9.  
- Relationships in EF Core – Configuring One-to-One, One-to-Many, and Many-to-Many.  
- Pagination, Sorting & Searching – Optimizing queries for large datasets.  
- Global Query Filters – Applying filters globally to avoid repetitive queries.  
- Soft Deletes – Implementing logical deletes without removing data.  
- Bulk Operations – Optimizing insert, update, and delete operations in EF Core.  
- Concurrency Control – Preventing data conflicts with optimistic locking.  
- Multiple DB Contexts – Using multiple database contexts in a single application.  
- Running Migrations – Best practices for applying database migrations.  
- Cleaning Migrations – Managing and reducing clutter in migration files.  
- Tracking vs. No-Tracking Queries – Understanding performance implications.  
- Transactions in EF Core – Ensuring data consistency with database transactions.  
- Interceptors in EF Core – Customizing query behavior dynamically.  
- Seeding Initial Data – Populating the database with default data.  
- Stored Procedures – Executing raw SQL and stored procedures efficiently.  
- Lazy, Eager & Explicit Loading – Managing how related entities are retrieved.  

# Chapter 3: API Security & Authentication  

- API Key Authentication – Securing APIs with API keys.  
- Implementing JWT Authentication – Adding token-based authentication.  
- Refresh Tokens in ASP.NET Core – Extending authentication session securely.  
- Role-Based Authorization – Controlling access to API endpoints.  
- OAuth 2.0 & OpenID Connect – Implementing modern authentication.  
- Identity Endpoints – Managing users in .NET 8+.  
- Keycloak Integration – Authentication & authorization with Keycloak.  
- CORS in ASP.NET Core – Handling cross-origin requests securely.  
- Rate Limiting in ASP.NET Core – Protecting APIs from abuse and overuse.  

## Chapter 4: Advanced API Features & Design Patterns  

- Options Pattern in ASP.NET Core – Managing Configurations Effectively  
- FluentValidation – Writing Clean & Reusable Request Validations  
- CQRS & MediatR – Building Scalable, Decoupled APIs  
- Validation with MediatR Pipeline and FluentValidation
- Validating Options Pattern with FluentValidation
- Feature Flags – Enabling & Disabling Features Dynamically  
- Implementing API Versioning – Managing Breaking Changes
- Background Jobs in .NET APIs – Using Hangfire & Worker Services  
- Webhooks – Enabling Event-Driven API Communication  
- Real-Time APIs with SignalR – Implementing WebSockets  
- PDF Generation in .NET – Creating Reports & Documents  

## Chapter 5: Performance Optimization & Caching  
- Benchmarking .NET APIs – Measuring & Optimizing Performance  
- Rate Limiting in ASP.NET Core – Protecting APIs from Abuse  
- Response Compression & Content Negotiation – Optimizing Payload Size  
- In-Memory Caching – Reducing Database Calls  
- Distributed Caching with Redis – Scaling API Performance
- Response Caching with MediatR in ASP.NET Core - Powerful Pipeline Behavior
- Hybrid Caching – Combining In-Memory & Distributed Caching  

## Chapter 6: HTTP Clients & Resilient API Communication

- Best Way to Use HTTP Clients in .NET APIs  
- Using Refit for Simplified API Calls in .NET  
- Resilient API Calls with Polly – Handling Failures Gracefully  
- Solving HttpClient Authentication with Delegating Handlers
- Logging & Monitoring HTTP Calls – Tracking and debugging outgoing requests in .NET.
- HttpClient Factory in .NET – Managing HTTP clients efficiently with dependency injection.

## Chapter 7: Architecting .NET APIs - Clean Code & Best Practices  
- Understanding API Architecture – Monolith vs Modular Monolith vs Microservices  
- Domain-Driven Design (DDD) in .NET – Applying DDD principles to API development.
- Implementing Clean Architecture in .NET – Step-by-Step Guide  
- Vertical Slice Architecture – Organizing Features the Right Way

## Chapter 8: Tracing, Monitoring & Observability  
- Understanding HTTP Status Codes – Returning Proper API Responses  
- Health Checks in ASP.NET Core – Ensuring API Reliability  
- OpenTelemetry in .NET – Distributed Tracing & Observability  
- Metrics Collection with Prometheus – Capturing API performance metrics for monitoring.
- Distributed Tracing with Jaeger & Zipkin – Visualizing API request flows across services.

## Chapter 9: Testing & Quality Assurance  
- Writing Unit Tests for .NET APIs – Best Practices  
- Integration Testing in .NET Web APIs – Step-by-Step Guide  
- Using TestContainers for API Testing in .NET  
- Automated Test Pipelines – Running CI/CD Tests for APIs  
- Handling API Contracts – Preventing Breaking Changes

## Chapter 9: Deployment, DevOps & Scaling  
- Getting Started with Docker – Containerizing Your API  
- Built-In Docker Support for .NET Applications
- Building & Deploying .NET APIs with GitHub Actions  
- Managing API Gateway with YARP in .NET
- Final API Checklist – Best Practices Before Going Live  