SELECT d."Id", d."Description", d."Name"
FROM "Departments" AS d
ORDER BY d."Id";

SELECT p."Id", p."DepartmentId", p."Name", p."Summary", d."Id"
FROM "Departments" AS d
INNER JOIN "Projects" AS p ON d."Id" = p."DepartmentId"
ORDER BY d."Id";

SELECT e."Id", e."DepartmentId", e."Email", e."FullName", e."Title", d."Id"
FROM "Departments" AS d
INNER JOIN "Employees" AS e ON d."Id" = e."DepartmentId"
ORDER BY d."Id";

SELECT d0."Id", d0."DepartmentId", d0."Path", d0."Title", d."Id"
FROM "Departments" AS d
INNER JOIN "Documents" AS d0 ON d."Id" = d0."DepartmentId"
ORDER BY d."Id";
