SELECT d."Id", d."Description", d."Name", p."Id", p."DepartmentId", p."Name", p."Summary", e."Id", e."DepartmentId", e."Email", e."FullName", e."Title", d0."Id", d0."DepartmentId", d0."Path", d0."Title"
FROM "Departments" AS d
LEFT JOIN "Projects" AS p ON d."Id" = p."DepartmentId"
LEFT JOIN "Employees" AS e ON d."Id" = e."DepartmentId"
LEFT JOIN "Documents" AS d0 ON d."Id" = d0."DepartmentId"
ORDER BY d."Id", p."Id", e."Id";
