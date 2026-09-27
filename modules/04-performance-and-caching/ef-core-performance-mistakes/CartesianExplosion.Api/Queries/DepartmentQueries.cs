using CartesianExplosion.Api.Data;
using CartesianExplosion.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CartesianExplosion.Api.Queries;

public static class DepartmentQueries
{
    /// <summary>The query most of us write: sibling collection Includes in the default single-query mode.</summary>
    public static IQueryable<Department> Single(AppDbContext db, bool withDocuments)
    {
        IQueryable<Department> query = db.Departments
            .Include(d => d.Projects)
            .Include(d => d.Employees);

        return withDocuments ? query.Include(d => d.Documents) : query;
    }

    /// <summary>The fix: one extra query per included collection instead of JOINs.</summary>
    public static IQueryable<Department> Split(AppDbContext db, bool withDocuments) =>
        Single(db, withDocuments).AsSplitQuery();
}
