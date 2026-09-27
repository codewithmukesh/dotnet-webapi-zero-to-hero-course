# Test environment

All numbers in this folder were measured on 2026-09-27 on one machine:

| | |
|---|---|
| CPU | Intel Core Ultra 9 275HX (24 cores / 24 threads) |
| RAM | 31.4 GB |
| OS | Windows 11 Pro 10.0.26200 |
| Docker | Docker Desktop, engine 29.5.3 (VM: 24 CPUs, ~15.3 GB) |
| Database | `postgres:17-alpine` in Docker, default settings |
| .NET | SDK 10.0.401, EF Core 10.0.12, Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3 |
| Build | Release |

Things to keep in mind when reading the numbers:

- **The database is local.** The API and Postgres run on the same machine, so a round trip costs almost nothing. That flatters split queries, which make one extra round trip per collection. Against a cloud database with a few milliseconds of latency per trip, the gap between single and split shrinks for small results.
- **The machine was not idle.** A browser and an editor were open during the runs. Medians over repeated runs are reported to smooth that out, but treat small differences (a few ms) as noise.
- **"Result size" is Postgres's row size** (`pg_column_size` summed over the rows each statement returns, including a small per-row header). It tracks what goes over the wire closely, but it is not an exact byte count of the network traffic.
