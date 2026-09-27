# Server-side errors during the load runs

Tallied from the API's console log for each run (the raw logs were mostly repeated stack traces, so only the tally is kept). `load-first-error.txt` holds one full error body verbatim.

| run | exceptions logged by the API |
|---|---|
| single-c100 | none |
| single-c200 | 159 × Npgsql.NpgsqlException (0x80004005): The connection pool has been exhausted |
| single-c400 | 642 × Npgsql.NpgsqlException (0x80004005): The connection pool has been exhausted |
| split-c100 | none |
| split-c200 | none |
| split-c400 | none |

Each failed request is logged more than once (EF Core and ASP.NET Core both log the exception), so these counts are higher than the client-side `http-500` counts in the `load-*.json` files. Use the JSON files for request counts; use this table only for *which* error happened.
