using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Director = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ReleaseDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Genre = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Synopsis = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AverageRating = table.Column<double>(type: "double precision", nullable: true),
                    RatingCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movies", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "movies");
        }
    }
}
