using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OxideBrowserBack.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAdminKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminKey",
                table: "PortfolioData");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminKey",
                table: "PortfolioData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
