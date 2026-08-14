using DocumentManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentManager.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202608140001_InitialCreate")]
public sealed class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "FolioSequences",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false),
                LastValue = table.Column<long>(type: "INTEGER", nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_FolioSequences", item => item.Id));

        migrationBuilder.CreateTable(
            name: "ServiceRecords",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                ServiceOrderFolio = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                InternalFolio = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                FinalPdfPath = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_ServiceRecords", item => item.Id));

        migrationBuilder.CreateIndex(
            name: "IX_ServiceRecords_Date",
            table: "ServiceRecords",
            column: "Date");

        migrationBuilder.CreateIndex(
            name: "IX_ServiceRecords_InternalFolio",
            table: "ServiceRecords",
            column: "InternalFolio",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ServiceRecords_ServiceOrderFolio",
            table: "ServiceRecords",
            column: "ServiceOrderFolio");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ServiceRecords");
        migrationBuilder.DropTable(name: "FolioSequences");
    }
}
