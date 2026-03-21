# .NET Web API Zero to Hero - The Complete ASP.NET Core REST API Course

[![GitHub stars](https://img.shields.io/github/stars/codewithmukesh/dotnet-webapi-zero-to-hero-course?style=social)](https://github.com/codewithmukesh/dotnet-webapi-zero-to-hero-course)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![YouTube](https://img.shields.io/badge/YouTube-codewithmukesh-red?logo=youtube)](https://www.youtube.com/@codewithmukesh?sub_confirmation=1)

![.NET Web API Zero to Hero](/assets/dotnet-webapi-zero-to-hero-banner.png)

The most comprehensive **FREE course** for mastering **ASP.NET Core Web API** development — from first principles to production deployment. Every module includes an in-depth article, a YouTube video walkthrough, and a fully runnable .NET 10 project you can clone and experiment with.

This isn't another surface-level CRUD tutorial. You'll learn the patterns, architectures, and practices that real-world .NET teams use to ship production APIs — **Clean Architecture**, **CQRS**, **Domain-Driven Design**, **Docker**, **observability**, **testing**, and more.

> **[Start the Course](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)** — Full syllabus, articles, and video walkthroughs.

## Course Modules

### Module 01 — Getting Started

Build a strong foundation in ASP.NET Core Web API fundamentals.

| # | Lesson | Article | Code |
|---|--------|---------|------|
| 1 | REST API Best Practices | [Read](https://codewithmukesh.com/blog/restful-api-best-practices-for-dotnet-developers/) | [`restful-api-best-practices`](./modules/01-getting-started/restful-api-best-practices-for-dotnet-developers/) |
| 2 | File-Based Apps in .NET 10 | [Read](https://codewithmukesh.com/blog/file-based-apps-dotnet-10/) | [`file-based-apps`](./modules/01-getting-started/file-based-apps/) |
| 3 | Migrate from .sln to .slnx | [Read](https://codewithmukesh.com/blog/slnx-solution-format-dotnet/) | — |
| 4 | Minimal API Endpoints | [Read](https://codewithmukesh.com/blog/minimal-apis-aspnet-core/) | [`minimal-apis`](./modules/01-getting-started/minimal-apis-aspnet-core/) |
| 5 | Swagger is Dead — OpenAPI Alternatives | [Read](https://codewithmukesh.com/blog/dotnet-swagger-alternatives-openapi/) | — |
| 6 | Global Exception Handling | [Read](https://codewithmukesh.com/blog/global-exception-handling-in-aspnet-core/) | [`global-exception-handling`](./modules/01-getting-started/global-exception-handling/) |
| 7 | FluentValidation | [Read](https://codewithmukesh.com/blog/fluentvalidation-in-aspnet-core/) | [`fluentvalidation`](./modules/01-getting-started/fluentvalidation-in-aspnet-core/) |
| 8 | Structured Logging with Serilog | [Read](https://codewithmukesh.com/blog/structured-logging-with-serilog-in-aspnet-core/) | [`serilog-logging`](./modules/01-getting-started/structured-logging-with-serilog-in-aspnet-core/) |
| 9 | Middleware & Request Pipeline | [Read](https://codewithmukesh.com/blog/middlewares-in-aspnet-core/) | — |
| 10 | Filters in ASP.NET Core | [Read](https://codewithmukesh.com/blog/filters-in-aspnet-core/) | — |
| 11 | Options Pattern | [Read](https://codewithmukesh.com/blog/options-pattern-in-aspnet-core/) | — |
| 12 | Dependency Injection Deep Dive | Coming Soon | — |

### Module 02 — Database Management with EF Core

Master data access, migrations, and advanced EF Core patterns with PostgreSQL.

| # | Lesson | Article | Code |
|---|--------|---------|------|
| 1 | Web API CRUD with EF Core | [Read](https://codewithmukesh.com/blog/aspnet-core-webapi-crud-with-entity-framework-core-full-course/) | [`ef-core-crud`](./modules/02-database-management-with-ef-core/aspnet-core-webapi-crud-with-entity-framework-core-full-course/) |
| 2 | EF Core Relationships (1:1, 1:N, M:N) | [Read](https://codewithmukesh.com/blog/ef-core-relationships-one-to-one-one-to-many-many-to-many/) | — |
| 3 | Pagination, Sorting & Searching | [Read](https://codewithmukesh.com/blog/pagination-sorting-searching-aspnet-core-webapi/) | [`pagination-sorting`](./modules/02-database-management-with-ef-core/pagination-sorting-searching-aspnet-core-webapi/) |
| 4 | Global Query Filters | [Read](https://codewithmukesh.com/blog/global-query-filters-efcore/) | — |
| 5 | Soft Deletes | [Read](https://codewithmukesh.com/blog/soft-deletes-efcore/) | [`soft-deletes`](./modules/02-database-management-with-ef-core/soft-deletes-efcore/) |
| 6 | Bulk Operations | [Read](https://codewithmukesh.com/blog/bulk-operations-efcore/) | [`bulk-operations`](./modules/02-database-management-with-ef-core/bulk-operations-efcore/) |
| 7 | Multiple DbContext | [Read](https://codewithmukesh.com/blog/multiple-dbcontext-efcore/) | [`multiple-dbcontext`](./modules/02-database-management-with-ef-core/multiple-dbcontext-efcore/) |
| 8 | Concurrency Control (Optimistic Locking) | Coming Soon | [`concurrency-control`](./modules/02-database-management-with-ef-core/concurrency-control-optimistic-locking-efcore/) |
| 9 | Running Migrations | Coming Soon | [`running-migrations`](./modules/02-database-management-with-ef-core/running-migrations-efcore/) |
| 10 | Seeding Initial Data | Coming Soon | [`seeding-data`](./modules/02-database-management-with-ef-core/seeding-initial-data-efcore/) |
| 11 | Cleaning Migrations | Coming Soon | — |
| 12 | Tracking vs No-Tracking Queries | Coming Soon | — |
| 13 | Compiled Queries | Coming Soon | — |
| 14 | LeftJoin & RightJoin in LINQ | Coming Soon | — |

### Upcoming Modules

| Module | Topics |
|--------|--------|
| 03 — Security & Authentication | JWT authentication, OAuth 2.0, authorization policies, API key auth |
| 04 — Advanced Patterns | CQRS with MediatR, AutoMapper, background jobs, rate limiting |
| 05 — Performance & Caching | Redis, output caching, response compression, EF Core performance mistakes |
| 06 — HTTP Clients & Resilience | Refit, Polly, retry policies, circuit breakers |
| 07 — Architecture | Clean Architecture, Domain-Driven Design, Vertical Slice Architecture |
| 08 — Observability | OpenTelemetry, distributed tracing, health checks, Grafana dashboards, .NET Aspire |
| 09 — Testing | Unit tests, integration tests with TestContainers, mocking, TDD |
| 10 — Deployment | [Containerize .NET Without a Dockerfile](https://codewithmukesh.com/blog/containerize-dotnet-without-dockerfile/), CI/CD with GitHub Actions, production hardening |

## Quick Start

```bash
# Clone the repository
git clone https://github.com/codewithmukesh/dotnet-webapi-zero-to-hero-course.git
cd dotnet-webapi-zero-to-hero-course

# Pick any lesson and run it
cd modules/01-getting-started/minimal-apis-aspnet-core/MinimalApis.ProductCatalog.Api
dotnet run

# Open the API docs
# http://localhost:5000/scalar/v1
```

Most database lessons require PostgreSQL via Docker:

```bash
# From any lesson folder that has a docker-compose.yml
docker compose up -d
dotnet run --project <ProjectName>.Api
```

## What You'll Learn

- Build production-ready **REST APIs** with **ASP.NET Core 10** and **Minimal APIs**
- Master **Entity Framework Core 10** — CRUD, migrations, seeding, relationships, bulk ops, soft deletes, concurrency
- Implement **global exception handling** with RFC 9457 ProblemDetails
- Validate requests with **FluentValidation** and built-in .NET 10 validation
- Set up **structured logging** with Serilog, Seq, and correlation IDs
- Secure APIs with **JWT tokens**, **OAuth 2.0**, and role-based authorization
- Apply **Clean Architecture**, **CQRS**, and **Domain-Driven Design**
- Add **Redis caching** and performance optimization
- Build resilient HTTP clients with **Refit** and **Polly**
- Write **unit tests** and **integration tests** with xUnit and TestContainers
- Monitor APIs with **OpenTelemetry**, health checks, and metrics
- Deploy with **Docker** and **CI/CD pipelines**

## Who Is This For?

| Audience | Why This Course |
|----------|-----------------|
| **Beginners** | Start from REST fundamentals and build up to production patterns step by step |
| **C# Developers** | Go beyond basic CRUD — learn the architecture and patterns used in enterprise .NET |
| **Students** | Build a portfolio of real-world API projects and prepare for .NET developer interviews |
| **Framework Upgraders** | Migrate from .NET Framework to modern .NET 10 with confidence |
| **Full-Stack Developers** | Learn to build robust backend APIs to power your frontend applications |

## Tech Stack

| Technology | Purpose |
|------------|---------|
| .NET 10 | Runtime & SDK |
| ASP.NET Core | Web API framework |
| Entity Framework Core 10 | ORM & data access |
| PostgreSQL 17 | Primary database |
| Serilog | Structured logging |
| FluentValidation | Request validation |
| Scalar | OpenAPI documentation UI |
| Docker | Containerization |

## Repository Structure

```
modules/
├── 01-getting-started/                    # REST, Minimal APIs, error handling, logging
│   ├── restful-api-best-practices-for-dotnet-developers/
│   ├── file-based-apps/
│   ├── minimal-apis-aspnet-core/
│   ├── global-exception-handling/
│   ├── fluentvalidation-in-aspnet-core/
│   └── structured-logging-with-serilog-in-aspnet-core/
│
├── 02-database-management-with-ef-core/   # EF Core CRUD, migrations, advanced patterns
│   ├── aspnet-core-webapi-crud-with-entity-framework-core-full-course/
│   ├── pagination-sorting-searching-aspnet-core-webapi/
│   ├── bulk-operations-efcore/
│   ├── soft-deletes-efcore/
│   ├── multiple-dbcontext-efcore/
│   ├── concurrency-control-optimistic-locking-efcore/
│   ├── running-migrations-efcore/
│   └── seeding-initial-data-efcore/
│
archive/                                   # Legacy .NET 8 samples (for reference)
```

## Prerequisites

- **C# fundamentals** — basic syntax, classes, async/await
- **[.NET 10 SDK](https://dotnet.microsoft.com/download)** installed
- **[Docker Desktop](https://www.docker.com/products/docker-desktop)** for database lessons
- **IDE** — Visual Studio 2022+, VS Code with C# Dev Kit, or JetBrains Rider

## FAQ

<details>
<summary><strong>Is this course really free?</strong></summary>

Yes — 100% free, forever. All articles, videos, and source code are open and accessible. No hidden paywalls, no premium tiers.
</details>

<details>
<summary><strong>What version of .NET does this use?</strong></summary>

All current modules target **.NET 10**. The `archive/` folder contains legacy .NET 8 samples for reference. The course will continue to be updated as new .NET versions release.
</details>

<details>
<summary><strong>Do I need prior ASP.NET Core experience?</strong></summary>

No. The course starts from REST fundamentals and builds up progressively. Basic C# knowledge is the only prerequisite.
</details>

<details>
<summary><strong>How is this different from other .NET API courses?</strong></summary>

Most courses stop at CRUD. This one covers what you actually need in production — global exception handling, structured logging, concurrency control, bulk operations, soft deletes, Clean Architecture, testing, Docker deployment, and observability. Every lesson includes production-ready code you can use in real projects.
</details>

<details>
<summary><strong>Can I use this code in my own projects?</strong></summary>

Absolutely. Everything is MIT licensed. Use it to learn, reference, or build upon.
</details>

<details>
<summary><strong>How often is new content added?</strong></summary>

New lessons are published regularly. Star the repo and subscribe to the newsletter to get notified.
</details>

## Stay Connected

- **Course Page**: [codewithmukesh.com/courses/dotnet-webapi-zero-to-hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)
- **Newsletter**: [newsletter.codewithmukesh.com](https://newsletter.codewithmukesh.com/)
- **YouTube**: [codewithmukesh](https://www.youtube.com/@codewithmukesh?sub_confirmation=1)
- **LinkedIn**: [iammukeshm](https://linkedin.com/in/iammukeshm)
- **Website**: [codewithmukesh.com](https://codewithmukesh.com)

## Support the Project

If this course helps you become a better .NET developer, give it a star — it helps others discover this free resource.

[![Star this repo](https://img.shields.io/github/stars/codewithmukesh/dotnet-webapi-zero-to-hero-course?style=social)](https://github.com/codewithmukesh/dotnet-webapi-zero-to-hero-course)

---

**[Start Learning Now](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)** | Built by [Mukesh Murugan](https://codewithmukesh.com)

## License

This project is licensed under the [MIT License](LICENSE).
