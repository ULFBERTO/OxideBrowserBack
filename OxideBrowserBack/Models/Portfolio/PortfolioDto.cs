namespace OxideBrowserBack.Models.Portfolio;

// DTO para recibir/enviar datos en formato JSON del frontend
public class PortfolioDto
{

    public ThemeDto Theme { get; set; } = new();
    public ProfileDto Profile { get; set; } = new();
    public HeroDto Hero { get; set; } = new();
    public StatsDto Stats { get; set; } = new();
    public List<ExperienceDto> Experience { get; set; } = new();
    public List<string> Technologies { get; set; } = new();
    public List<ProjectDto> Projects { get; set; } = new();
    public ContactDto Contact { get; set; } = new();
    public SidebarDto Sidebar { get; set; } = new();
    public FooterDto Footer { get; set; } = new();
}

public class ThemeDto
{
    public string Primary { get; set; } = "#2bee79";
    public string BackgroundDark { get; set; } = "#102217";
    public string SurfaceDark { get; set; } = "#162e21";
}

public class ProfileDto
{
    public string Name { get; set; } = "";
    public string ShortName { get; set; } = "";
    public string Initials { get; set; } = "";
    public LocalizedTextDto Role { get; set; } = new();
    public string AvatarUrl { get; set; } = "";
    public string Email { get; set; } = "";
    public string Github { get; set; } = "";
    public string Linkedin { get; set; } = "";
    public string CvUrl { get; set; } = "";
}

public class HeroDto
{
    public LocalizedTextDto Greeting { get; set; } = new();
    public LocalizedTextDto Description { get; set; } = new();
}

public class StatsDto
{
    public StatItemDto YearsActive { get; set; } = new();
    public StatItemDto Projects { get; set; } = new();
    public StatItemDto TechStack { get; set; } = new();
    public StatItemDto Experience { get; set; } = new();
}

public class StatItemDto
{
    public string Value { get; set; } = "";
    public LocalizedTextDto Label { get; set; } = new();
    public LocalizedTextDto Sublabel { get; set; } = new();
}

public class ExperienceDto
{
    public string Id { get; set; } = "";
    public string Icon { get; set; } = "work";
    public LocalizedTextDto Title { get; set; } = new();
    public string Date { get; set; } = "";
    public LocalizedTextDto Description { get; set; } = new();
    public bool IsCurrent { get; set; }
}

public class ProjectDto
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string CategoryColor { get; set; } = "primary";
    public string Year { get; set; } = "";
    public LocalizedTextDto Description { get; set; } = new();
    public List<string> Technologies { get; set; } = new();
    public string DemoUrl { get; set; } = "";
    public string RepoUrl { get; set; } = "";
    public string DownloadUrl { get; set; } = "";
    public string Icon { get; set; } = "code";
    public string Gradient { get; set; } = "";
    public string ImageUrl { get; set; } = "";
}

public class ContactDto
{
    public LocalizedTextDto Title { get; set; } = new();
    public LocalizedTextDto Description { get; set; } = new();
}

public class SidebarDto
{
    public LocalizedTextDto Availability { get; set; } = new();
    public LocalizedTextDto OpenToWork { get; set; } = new();
    public LocalizedTextDto DownloadCV { get; set; } = new();
}

public class FooterDto
{
    public LocalizedTextDto BuiltWith { get; set; } = new();
}

public class LocalizedTextDto
{
    public string Es { get; set; } = "";
    public string En { get; set; } = "";
}
