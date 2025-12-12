using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OxideBrowserBack.Data;
using OxideBrowserBack.Models.Portfolio;

namespace OxideBrowserBack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    private readonly PortfolioDbContext _context;

    public PortfolioController(PortfolioDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PortfolioDto>> Get()
    {
        var data = await _context.PortfolioData
            .Include(p => p.Projects)
            .Include(p => p.Experiences)
            .FirstOrDefaultAsync();

        if (data == null)
        {
            // Return default data if none exists
            return Ok(GetDefaultPortfolioDto());
        }

        return Ok(MapToDto(data));
    }

    [HttpPost]
    public async Task<ActionResult<PortfolioDto>> Save([FromBody] PortfolioDto dto)
    {
        var existing = await _context.PortfolioData
            .Include(p => p.Projects)
            .Include(p => p.Experiences)
            .FirstOrDefaultAsync();

        if (existing == null)
        {
            var newData = MapFromDto(dto);
            _context.PortfolioData.Add(newData);
        }
        else
        {
            UpdateFromDto(existing, dto);
        }

        await _context.SaveChangesAsync();
        return Ok(dto);
    }



    private PortfolioDto MapToDto(PortfolioData data)
    {
        return new PortfolioDto
        {
            Theme = new ThemeDto
            {
                Primary = data.ThemePrimary,
                BackgroundDark = data.ThemeBackgroundDark,
                SurfaceDark = data.ThemeSurfaceDark
            },
            Profile = new ProfileDto
            {
                Name = data.ProfileName,
                ShortName = data.ProfileShortName,
                Initials = data.ProfileInitials,
                Role = new LocalizedTextDto { Es = data.ProfileRoleEs, En = data.ProfileRoleEn },
                Email = data.ProfileEmail,
                Github = data.ProfileGithub,
                Linkedin = data.ProfileLinkedin,
                CvUrl = data.ProfileCvUrl
            },
            Hero = new HeroDto
            {
                Greeting = new LocalizedTextDto { Es = data.HeroGreetingEs, En = data.HeroGreetingEn },
                Description = new LocalizedTextDto { Es = data.HeroDescriptionEs, En = data.HeroDescriptionEn }
            },
            Stats = new StatsDto
            {
                YearsActive = new StatItemDto { Value = data.StatsYearsValue, Label = new LocalizedTextDto { Es = "Años Activo", En = "Years Active" }, Sublabel = new LocalizedTextDto { Es = "Desde 2022", En = "Since 2022" } },
                Projects = new StatItemDto { Value = data.StatsProjectsValue, Label = new LocalizedTextDto { Es = "Proyectos", En = "Projects" }, Sublabel = new LocalizedTextDto { Es = "Proyectos principales", En = "Major projects" } },
                TechStack = new StatItemDto { Value = data.StatsTechValue, Label = new LocalizedTextDto { Es = "Stack Tecnológico", En = "Tech Stack" }, Sublabel = new LocalizedTextDto { Es = "Tecnologías", En = "Technologies" } },
                Experience = new StatItemDto { Value = data.StatsExpValue, Label = new LocalizedTextDto { Es = "Experiencia", En = "Experience" }, Sublabel = new LocalizedTextDto { Es = "Freelance, Empresa, Personal", En = "Freelance, Company, Personal" } }
            },
            Technologies = data.Technologies.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList(),
            Projects = data.Projects.Select(p => new ProjectDto
            {
                Id = p.ProjectId,
                Title = p.Title,
                Category = p.Category,
                CategoryColor = p.CategoryColor,
                Year = p.Year,
                Description = new LocalizedTextDto { Es = p.DescriptionEs, En = p.DescriptionEn },
                Technologies = p.Technologies.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList(),
                DemoUrl = p.DemoUrl,
                RepoUrl = p.RepoUrl,
                DownloadUrl = p.DownloadUrl,
                Icon = p.Icon,
                Gradient = p.Gradient,
                ImageUrl = p.ImageUrl
            }).ToList(),
            Experience = data.Experiences.Select(e => new ExperienceDto
            {
                Id = e.ExperienceId,
                Icon = e.Icon,
                Title = new LocalizedTextDto { Es = e.TitleEs, En = e.TitleEn },
                Date = e.Date,
                Description = new LocalizedTextDto { Es = e.DescriptionEs, En = e.DescriptionEn },
                IsCurrent = e.IsCurrent
            }).ToList(),
            Contact = new ContactDto
            {
                Title = new LocalizedTextDto { Es = data.ContactTitleEs, En = data.ContactTitleEn },
                Description = new LocalizedTextDto { Es = data.ContactDescEs, En = data.ContactDescEn }
            },
            Sidebar = new SidebarDto
            {
                Availability = new LocalizedTextDto { Es = data.SidebarAvailabilityEs, En = data.SidebarAvailabilityEn },
                OpenToWork = new LocalizedTextDto { Es = data.SidebarOpenToWorkEs, En = data.SidebarOpenToWorkEn },
                DownloadCV = new LocalizedTextDto { Es = data.SidebarDownloadCvEs, En = data.SidebarDownloadCvEn }
            },
            Footer = new FooterDto
            {
                BuiltWith = new LocalizedTextDto { Es = data.FooterBuiltWithEs, En = data.FooterBuiltWithEn }
            }
        };
    }

    private PortfolioData MapFromDto(PortfolioDto dto)
    {
        var data = new PortfolioData
        {
            ThemePrimary = dto.Theme.Primary,
            ThemeBackgroundDark = dto.Theme.BackgroundDark,
            ThemeSurfaceDark = dto.Theme.SurfaceDark,
            ProfileName = dto.Profile.Name,
            ProfileShortName = dto.Profile.ShortName,
            ProfileInitials = dto.Profile.Initials,
            ProfileRoleEs = dto.Profile.Role.Es,
            ProfileRoleEn = dto.Profile.Role.En,
            ProfileEmail = dto.Profile.Email,
            ProfileGithub = dto.Profile.Github,
            ProfileLinkedin = dto.Profile.Linkedin,
            ProfileCvUrl = dto.Profile.CvUrl,
            HeroGreetingEs = dto.Hero.Greeting.Es,
            HeroGreetingEn = dto.Hero.Greeting.En,
            HeroDescriptionEs = dto.Hero.Description.Es,
            HeroDescriptionEn = dto.Hero.Description.En,
            StatsYearsValue = dto.Stats.YearsActive.Value,
            StatsProjectsValue = dto.Stats.Projects.Value,
            StatsTechValue = dto.Stats.TechStack.Value,
            StatsExpValue = dto.Stats.Experience.Value,
            ContactTitleEs = dto.Contact.Title.Es,
            ContactTitleEn = dto.Contact.Title.En,
            ContactDescEs = dto.Contact.Description.Es,
            ContactDescEn = dto.Contact.Description.En,
            FooterBuiltWithEs = dto.Footer.BuiltWith.Es,
            FooterBuiltWithEn = dto.Footer.BuiltWith.En,
            SidebarAvailabilityEs = dto.Sidebar.Availability.Es,
            SidebarAvailabilityEn = dto.Sidebar.Availability.En,
            SidebarOpenToWorkEs = dto.Sidebar.OpenToWork.Es,
            SidebarOpenToWorkEn = dto.Sidebar.OpenToWork.En,
            SidebarDownloadCvEs = dto.Sidebar.DownloadCV.Es,
            SidebarDownloadCvEn = dto.Sidebar.DownloadCV.En,
            Technologies = string.Join(",", dto.Technologies)
        };

        foreach (var p in dto.Projects)
        {
            data.Projects.Add(new Project
            {
                ProjectId = p.Id,
                Title = p.Title,
                Category = p.Category,
                CategoryColor = p.CategoryColor,
                Year = p.Year,
                DescriptionEs = p.Description.Es,
                DescriptionEn = p.Description.En,
                Technologies = string.Join(",", p.Technologies),
                DemoUrl = p.DemoUrl,
                RepoUrl = p.RepoUrl,
                DownloadUrl = p.DownloadUrl,
                Icon = p.Icon,
                Gradient = p.Gradient,
                ImageUrl = p.ImageUrl
            });
        }

        foreach (var e in dto.Experience)
        {
            data.Experiences.Add(new Experience
            {
                ExperienceId = e.Id,
                Icon = e.Icon,
                TitleEs = e.Title.Es,
                TitleEn = e.Title.En,
                Date = e.Date,
                DescriptionEs = e.Description.Es,
                DescriptionEn = e.Description.En,
                IsCurrent = e.IsCurrent
            });
        }

        return data;
    }

    private void UpdateFromDto(PortfolioData data, PortfolioDto dto)
    {
        data.ThemePrimary = dto.Theme.Primary;
        data.ThemeBackgroundDark = dto.Theme.BackgroundDark;
        data.ThemeSurfaceDark = dto.Theme.SurfaceDark;
        data.ProfileName = dto.Profile.Name;
        data.ProfileShortName = dto.Profile.ShortName;
        data.ProfileInitials = dto.Profile.Initials;
        data.ProfileRoleEs = dto.Profile.Role.Es;
        data.ProfileRoleEn = dto.Profile.Role.En;
        data.ProfileEmail = dto.Profile.Email;
        data.ProfileGithub = dto.Profile.Github;
        data.ProfileLinkedin = dto.Profile.Linkedin;
        data.ProfileCvUrl = dto.Profile.CvUrl;
        data.HeroGreetingEs = dto.Hero.Greeting.Es;
        data.HeroGreetingEn = dto.Hero.Greeting.En;
        data.HeroDescriptionEs = dto.Hero.Description.Es;
        data.HeroDescriptionEn = dto.Hero.Description.En;
        data.StatsYearsValue = dto.Stats.YearsActive.Value;
        data.StatsProjectsValue = dto.Stats.Projects.Value;
        data.StatsTechValue = dto.Stats.TechStack.Value;
        data.StatsExpValue = dto.Stats.Experience.Value;
        data.ContactTitleEs = dto.Contact.Title.Es;
        data.ContactTitleEn = dto.Contact.Title.En;
        data.ContactDescEs = dto.Contact.Description.Es;
        data.ContactDescEn = dto.Contact.Description.En;
        data.FooterBuiltWithEs = dto.Footer.BuiltWith.Es;
        data.FooterBuiltWithEn = dto.Footer.BuiltWith.En;
        data.SidebarAvailabilityEs = dto.Sidebar.Availability.Es;
        data.SidebarAvailabilityEn = dto.Sidebar.Availability.En;
        data.SidebarOpenToWorkEs = dto.Sidebar.OpenToWork.Es;
        data.SidebarOpenToWorkEn = dto.Sidebar.OpenToWork.En;
        data.SidebarDownloadCvEs = dto.Sidebar.DownloadCV.Es;
        data.SidebarDownloadCvEn = dto.Sidebar.DownloadCV.En;
        data.Technologies = string.Join(",", dto.Technologies);

        // Clear and re-add projects
        data.Projects.Clear();
        foreach (var p in dto.Projects)
        {
            data.Projects.Add(new Project
            {
                PortfolioDataId = data.Id,
                ProjectId = p.Id,
                Title = p.Title,
                Category = p.Category,
                CategoryColor = p.CategoryColor,
                Year = p.Year,
                DescriptionEs = p.Description.Es,
                DescriptionEn = p.Description.En,
                Technologies = string.Join(",", p.Technologies),
                DemoUrl = p.DemoUrl,
                RepoUrl = p.RepoUrl,
                DownloadUrl = p.DownloadUrl,
                Icon = p.Icon,
                Gradient = p.Gradient,
                ImageUrl = p.ImageUrl
            });
        }

        // Clear and re-add experiences
        data.Experiences.Clear();
        foreach (var e in dto.Experience)
        {
            data.Experiences.Add(new Experience
            {
                PortfolioDataId = data.Id,
                ExperienceId = e.Id,
                Icon = e.Icon,
                TitleEs = e.Title.Es,
                TitleEn = e.Title.En,
                Date = e.Date,
                DescriptionEs = e.Description.Es,
                DescriptionEn = e.Description.En,
                IsCurrent = e.IsCurrent
            });
        }
    }

    private PortfolioDto GetDefaultPortfolioDto()
    {
        return new PortfolioDto
        {
            Theme = new ThemeDto { Primary = "#2bee79", BackgroundDark = "#102217", SurfaceDark = "#162e21" },
            Profile = new ProfileDto
            {
                Name = "Mario Alejandro Patiño Ortiz",
                ShortName = "Mario Patiño",
                Initials = "MA",
                Role = new LocalizedTextDto { Es = "Desarrollador FullStack", En = "FullStack Developer" },
                Email = "mario.patino@example.com",
                Github = "https://github.com/ULFBERTO",
                Linkedin = "https://linkedin.com/in/mario-patino"
            },
            Hero = new HeroDto
            {
                Greeting = new LocalizedTextDto { Es = "Hola, soy", En = "Hello, I'm" },
                Description = new LocalizedTextDto
                {
                    Es = "Desarrollador FullStack especializado en tecnologías web modernas.",
                    En = "FullStack Developer specializing in modern web technologies."
                }
            },
            Stats = new StatsDto
            {
                YearsActive = new StatItemDto { Value = "3+", Label = new LocalizedTextDto { Es = "Años Activo", En = "Years Active" }, Sublabel = new LocalizedTextDto { Es = "Desde 2022", En = "Since 2022" } },
                Projects = new StatItemDto { Value = "5+", Label = new LocalizedTextDto { Es = "Proyectos", En = "Projects" }, Sublabel = new LocalizedTextDto { Es = "Proyectos principales", En = "Major projects" } },
                TechStack = new StatItemDto { Value = "10+", Label = new LocalizedTextDto { Es = "Stack Tecnológico", En = "Tech Stack" }, Sublabel = new LocalizedTextDto { Es = "Tecnologías", En = "Technologies" } },
                Experience = new StatItemDto { Value = "3", Label = new LocalizedTextDto { Es = "Experiencia", En = "Experience" }, Sublabel = new LocalizedTextDto { Es = "Freelance, Empresa, Personal", En = "Freelance, Company, Personal" } }
            },
            Technologies = new List<string> { "Angular", ".NET", "Rust", "C++", "Python", "TypeScript" },
            Projects = new List<ProjectDto>(),
            Experience = new List<ExperienceDto>(),
            Contact = new ContactDto
            {
                Title = new LocalizedTextDto { Es = "¿Trabajamos juntos?", En = "Let's work together?" },
                Description = new LocalizedTextDto { Es = "Estoy disponible para proyectos freelance.", En = "I'm available for freelance projects." }
            },
            Sidebar = new SidebarDto
            {
                Availability = new LocalizedTextDto { Es = "Disponibilidad", En = "Availability" },
                OpenToWork = new LocalizedTextDto { Es = "Disponible para trabajar", En = "Open to work" },
                DownloadCV = new LocalizedTextDto { Es = "Descargar CV", En = "Download CV" }
            },
            Footer = new FooterDto
            {
                BuiltWith = new LocalizedTextDto { Es = "Construido con código y café.", En = "Built with code & coffee." }
            }
        };
    }
}
