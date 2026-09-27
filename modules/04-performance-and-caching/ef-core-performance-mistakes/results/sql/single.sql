SELECT d."Id", d."Description", d."Name", p."Id", p."DepartmentId", p."Name", p."Summary", e."Id", e."DepartmentId", e."Email", e."FullName", e."Title"
FROM "Departments" AS d
LEFT JOIN "Projects" AS p ON d."Id" = p."DepartmentId"
LEFT JOIN "Employees" AS e ON d."Id" = e."DepartmentId"
ORDER BY d."Id", p."Id";
