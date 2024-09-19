using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StarterKit.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admin",
                columns: table => new
                {
                    AdminId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin", x => x.AdminId);
                });

            migrationBuilder.InsertData(
                table: "Admin",
                columns: new[] { "AdminId", "Email", "Password", "UserName" },
                values: new object[,]
                {
                    { 1, "admin1@example.com", "^�H��(qQ��o��)'s`=\rj���*�rB�", "admin1" },
                    { 2, "admin2@example.com", "\\N@6��G��Ae=j_��a%0�QU��\\", "admin2" },
                    { 3, "admin3@example.com", "�j\\��f������x�s+2��D�o���", "admin3" },
                    { 4, "admin4@example.com", "�].��g��Պ��t��?��^�T��`aǳ", "admin4" },
                    { 5, "admin5@example.com", "E�=���:�-����gd����bF��80]�", "admin5" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admin_UserName",
                table: "Admin",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin");
        }
    }
}
