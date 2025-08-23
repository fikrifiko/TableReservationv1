using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Table_Reservation.Migrations
{
    /// <inheritdoc />
    public partial class AddCanvasModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
       name: "CanvasModels",
       columns: table => new
       {
           Id = table.Column<int>(type: "int", nullable: false)
               .Annotation("SqlServer:Identity", "1, 1"),
           Width = table.Column<int>(type: "int", nullable: false),
           Height = table.Column<int>(type: "int", nullable: false)
       },
       constraints: table =>
       {
           table.PrimaryKey("PK_CanvasModels", x => x.Id);
       });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
        name: "CanvasModels");
        }
    }
}
