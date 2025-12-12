using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OxideBrowserBack.Migrations
{
    /// <inheritdoc />
    public partial class InitialPortfolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PortfolioData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThemePrimary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThemeBackgroundDark = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThemeSurfaceDark = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileShortName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileInitials = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileRoleEs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileRoleEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileGithub = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileLinkedin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileCvUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HeroGreetingEs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HeroGreetingEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HeroDescriptionEs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HeroDescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatsYearsValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatsProjectsValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatsTechValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatsExpValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactTitleEs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactTitleEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactDescEs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactDescEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FooterBuiltWithEs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FooterBuiltWithEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SidebarAvailabilityEs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SidebarAvailabilityEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SidebarOpenToWorkEs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SidebarOpenToWorkEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SidebarDownloadCvEs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SidebarDownloadCvEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Technologies = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Experiences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioDataId = table.Column<int>(type: "int", nullable: false),
                    ExperienceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TitleEs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionEs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Experiences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Experiences_PortfolioData_PortfolioDataId",
                        column: x => x.PortfolioDataId,
                        principalTable: "PortfolioData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioDataId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionEs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Technologies = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DemoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DownloadUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gradient = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_PortfolioData_PortfolioDataId",
                        column: x => x.PortfolioDataId,
                        principalTable: "PortfolioData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Experiences_PortfolioDataId",
                table: "Experiences",
                column: "PortfolioDataId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_PortfolioDataId",
                table: "Projects",
                column: "PortfolioDataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Experiences");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "PortfolioData");
        }
    }
}
