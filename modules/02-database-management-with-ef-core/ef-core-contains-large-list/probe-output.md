# What EF Core actually sends

- EF Core assembly version: `10.0.11.0`
- Generated: 2026-08-31 14:47 UTC

## Padding: list size vs parameters actually sent

| List size | Parameters sent | Padding added | Translation |
|---|---|---|---|
| 1 | 1 | +0 | multiple scalar parameters |
| 2 | 2 | +0 | multiple scalar parameters |
| 3 | 3 | +0 | multiple scalar parameters |
| 5 | 5 | +0 | multiple scalar parameters |
| 6 | 10 | +4 | multiple scalar parameters |
| 8 | 10 | +2 | multiple scalar parameters |
| 10 | 10 | +0 | multiple scalar parameters |
| 11 | 20 | +9 | multiple scalar parameters |
| 20 | 20 | +0 | multiple scalar parameters |
| 50 | 50 | +0 | multiple scalar parameters |
| 100 | 100 | +0 | multiple scalar parameters |
| 150 | 150 | +0 | multiple scalar parameters |
| 151 | 200 | +49 | multiple scalar parameters |
| 200 | 200 | +0 | multiple scalar parameters |
| 500 | 500 | +0 | multiple scalar parameters |
| 751 | 800 | +49 | multiple scalar parameters |
| 800 | 800 | +0 | multiple scalar parameters |
| 810 | 900 | +90 | multiple scalar parameters |
| 1,000 | 1,000 | +0 | multiple scalar parameters |
| 1,010 | 1,100 | +90 | multiple scalar parameters |
| 1,500 | 1,500 | +0 | multiple scalar parameters |
| 1,501 | 1,600 | +99 | multiple scalar parameters |
| 1,990 | 2,000 | +10 | multiple scalar parameters |
| 2,000 | 2,000 | +0 | multiple scalar parameters |
| 2,001 | 2,010 | +9 | multiple scalar parameters |
| 2,050 | 2,050 | +0 | multiple scalar parameters |
| 2,069 | 2,070 | +1 | multiple scalar parameters |
| 2,090 | 2,090 | +0 | multiple scalar parameters |
| 2,091 | 2,091 | +0 | multiple scalar parameters |
| 2,094 | 2,094 | +0 | multiple scalar parameters |
| 2,098 | 2,098 | +0 | multiple scalar parameters |

## The boundary: what happens as the list crosses the parameter ceiling

| List size | Outcome | Parameters sent | Translation |
|---|---|---|---|
| 2,000 | OK (2,000 rows) | 2,000 | multiple scalar parameters |
| 2,050 | OK (2,050 rows) | 2,050 | multiple scalar parameters |
| 2,090 | OK (2,090 rows) | 2,090 | multiple scalar parameters |
| 2,094 | OK (2,094 rows) | 2,094 | multiple scalar parameters |
| 2,095 | OK (2,095 rows) | 2,095 | multiple scalar parameters |
| 2,096 | OK (2,096 rows) | 2,096 | multiple scalar parameters |
| 2,097 | OK (2,097 rows) | 2,097 | multiple scalar parameters |
| 2,098 | OK (2,098 rows) | 2,098 | multiple scalar parameters |
| 2,099 | OK (2,099 rows) | 1 | single JSON parameter (`OPENJSON`) |
| 2,100 | OK (2,100 rows) | 1 | single JSON parameter (`OPENJSON`) |
| 2,101 | OK (2,101 rows) | 1 | single JSON parameter (`OPENJSON`) |
| 2,200 | OK (2,200 rows) | 1 | single JSON parameter (`OPENJSON`) |
| 2,500 | OK (2,500 rows) | 1 | single JSON parameter (`OPENJSON`) |
| 5,000 | OK (5,000 rows) | 1 | single JSON parameter (`OPENJSON`) |
| 10,000 | OK (10,000 rows) | 1 | single JSON parameter (`OPENJSON`) |
| 50,000 | OK (50,000 rows) | 1 | single JSON parameter (`OPENJSON`) |
| 100,000 | OK (100,000 rows) | 1 | single JSON parameter (`OPENJSON`) |

## Translation modes at a 100-item list

| Mode | Parameters sent | Translation |
|---|---|---|
| default (MultipleParameters) | 100 | multiple scalar parameters |
| global: MultipleParameters | 100 | multiple scalar parameters |
| global: Parameter | 1 | single JSON parameter (`OPENJSON`) |
| global: Constant | 0 | inlined constants |
| per-query: EF.Parameter | 1 | single JSON parameter (`OPENJSON`) |
| per-query: EF.Constant | 0 | inlined constants |

## Translation modes at a 5,000-item list

| Mode | Parameters sent | Translation |
|---|---|---|
| default (MultipleParameters) | 1 | single JSON parameter (`OPENJSON`) |
| global: MultipleParameters | 1 | single JSON parameter (`OPENJSON`) |
| global: Parameter | 1 | single JSON parameter (`OPENJSON`) |
| global: Constant | 0 | inlined constants |
| per-query: EF.Parameter | 1 | single JSON parameter (`OPENJSON`) |
| per-query: EF.Constant | 0 | inlined constants |

## Sample generated SQL

### 3 items, default

Parameters sent: **3**

```sql
SELECT COUNT(*)
FROM [Products] AS [p]
WHERE [p].[Id] IN (@ids1, @ids2, @ids3)
```

### 8 items, default (padding visible)

Parameters sent: **10**

```sql
SELECT COUNT(*)
FROM [Products] AS [p]
WHERE [p].[Id] IN (@ids1, @ids2, @ids3, @ids4, @ids5, @ids6, @ids7, @ids8, @ids9, @ids10)
```

### 3 items, EF.Parameter

Parameters sent: **1**

```sql
SELECT COUNT(*)
FROM [Products] AS [p]
WHERE [p].[Id] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ids) WITH ([value] int '$') AS [i]
)
```

### 3 items, EF.Constant

Parameters sent: **0**

```sql
SELECT COUNT(*)
FROM [Products] AS [p]
WHERE [p].[Id] IN (1, 333334, 666667)
```

### 2,098 items, default (last multi-parameter size)

Parameters sent: **2,098**

```sql
SELECT COUNT(*)
FROM [Products] AS [p]
WHERE [p].[Id] IN (@ids1, @ids2, @ids3, @ids4, @ids5, @ids6, @ids7, @ids8, @ids9, @ids10, @ids11, @ids12, @ids13, @ids14, @ids15, @ids16, @ids17, @ids18, @ids19, @ids20, @ids21, @ids22, @ids23, @ids24, @ids25, @ids26, @ids27, @ids28, @ids29, @ids30, @ids31, @ids32, @ids33, @ids34, @ids35, @ids36, @ids37, @id
    ...
 @ids2074, @ids2075, @ids2076, @ids2077, @ids2078, @ids2079, @ids2080, @ids2081, @ids2082, @ids2083, @ids2084, @ids2085, @ids2086, @ids2087, @ids2088, @ids2089, @ids2090, @ids2091, @ids2092, @ids2093, @ids2094, @ids2095, @ids2096, @ids2097, @ids2098)
```

### 2,099 items, default (first fallback size)

Parameters sent: **1**

```sql
SELECT COUNT(*)
FROM [Products] AS [p]
WHERE [p].[Id] IN (
    SELECT [__openjson0].[Value]
    FROM OPENJSON(@ids) WITH ([Value] int '$') AS [__openjson0]
)
```

## EF.Constant: what runs vs what gets logged

Executed against the database (seen from a `DbCommandInterceptor`):

```sql
WHERE [p].[Id] IN (1, 333334, 666667)
```

EF Core 10 redacts these inlined values in its own logs unless `EnableSensitiveDataLogging()` is on.

