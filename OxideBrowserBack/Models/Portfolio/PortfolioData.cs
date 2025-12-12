namespace OxideBrowserBack.Models.Portfolio;

public class PortfolioData
{
    public int Id { get; set; }

    
    // Theme
    public string ThemePrimary { get; set; } = "#2bee79";
    public string ThemeBackgroundDark { get; set; } = "#102217";
    public string ThemeSurfaceDark { get; set; } = "#162e21";
    
    // Profile
    public string ProfileName { get; set; } = "";
    public string ProfileShortName { get; set; } = "";
    public string ProfileInitials { get; set; } = "";
    public string ProfileRoleEs { get; set; } = "";
    public string ProfileRoleEn { get; set; } = "";
    public string ProfileEmail { get; set; } = "";
    public string ProfileGithub { get; set; } = "";
    public string ProfileLinkedin { get; set; } = "";
    public string ProfileCvUrl { get; set; } = "";
    
    // Hero
    public string HeroGreetingEs { get; set; } = "";
    public string HeroGreetingEn { get; set; } = "";
    public string HeroDescriptionEs { get; set; } = "";
    public string HeroDescriptionEn { get; set; } = "";
    
    // Stats
    public string StatsYearsValue { get; set; } = "3+";
    public string StatsProjectsValue { get; set; } = "5+";
    public string StatsTechValue { get; set; } = "10+";
    public string StatsExpValue { get; set; } = "3";
    
    // Contact
    public string ContactTitleEs { get; set; } = "";
    public string ContactTitleEn { get; set; } = "";
    public string ContactDescEs { get; set; } = "";
    public string ContactDescEn { get; set; } = "";
    
    // Footer
    public string FooterBuiltWithEs { get; set; } = "";
    public string FooterBuiltWithEn { get; set; } = "";
    
    // Sidebar
    public string SidebarAvailabilityEs { get; set; } = "";
    public string SidebarAvailabilityEn { get; set; } = "";
    public string SidebarOpenToWorkEs { get; set; } = "";
    public string SidebarOpenToWorkEn { get; set; } = "";
    public string SidebarDownloadCvEs { get; set; } = "";
    public string SidebarDownloadCvEn { get; set; } = "";
    
    // Technologies (comma separated)
    public string Technologies { get; set; } = "";
    
    // Navigation properties
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Experience> Experiences { get; set; } = new List<Experience>();
}

public class Project
{
    public int Id { get; set; }
    public int PortfolioDataId { get; set; }
    public string ProjectId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string CategoryColor { get; set; } = "primary";
    public string Year { get; set; } = "";
    public string DescriptionEs { get; set; } = "";
    public string DescriptionEn { get; set; } = "";
    public string Technologies { get; set; } = ""; // comma separated
    public string DemoUrl { get; set; } = "";
    public string RepoUrl { get; set; } = "";
    public string DownloadUrl { get; set; } = "";
    public string Icon { get; set; } = "code";
    public string Gradient { get; set; } = "from-purple-900/50 to-blue-900/50";
    public string ImageUrl { get; set; } = "";
    
    public PortfolioData? PortfolioData { get; set; }
}

public class Experience
{
    public int Id { get; set; }
    public int PortfolioDataId { get; set; }
    public string ExperienceId { get; set; } = "";
    public string Icon { get; set; } = "work";
    public string TitleEs { get; set; } = "";
    public string TitleEn { get; set; } = "";
    public string Date { get; set; } = "";
    public string DescriptionEs { get; set; } = "";
    public string DescriptionEn { get; set; } = "";
    public bool IsCurrent { get; set; }
    
    public PortfolioData? PortfolioData { get; set; }
}
