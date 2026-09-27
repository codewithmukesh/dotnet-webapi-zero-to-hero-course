# Memory and the container OOM experiment

Seed: Article size with documents (50 departments, 20 projects, 30 employees, 10 documents each), so the single query returns 300,000 rows. Each mode ran three times, each in a fresh process (`memory` command); the three runs agreed within 0.5 MB.

| mode | rows sent | allocated per request | peak working set |
|---|---|---|---|
| single | 300,000 | ~309 MB | ~103.5 MB |
| split | 3,050 | ~10.6 MB | ~82.4 MB |

**Finding:** the single query allocates about 29x more memory per request, but its *peak* memory is only about 21 MB higher. EF Core reads a single query as a stream: it materializes each row, fixes up the entities, and the garbage collector reclaims the row data as it goes. So the cost shows up as allocation and GC work (CPU), not as one big spike that would push a container over its memory limit.

**The OOM kill was not attempted.** To make the single query get killed while split survives, the container limit would have to sit between ~83 MB and ~103 MB. That is well below what a real ASP.NET Core container runs with, so a "killed" result would be staged, not representative. The video must not claim a container kill from one request.
