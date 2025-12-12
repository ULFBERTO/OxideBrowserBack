using Microsoft.EntityFrameworkCore;
using OxideBrowserBack.Data;
using OxideBrowserBack.Services;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("https://www.oxidebrowserback.somee.com", "https://oxide-browser.vercel.app", "http://localhost:4200", "http://localhost:4201", "http://127.0.0.1:4200", "http://localhost:5173", "https://port-folio-mp-9vsr.vercel.app")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// Add Entity Framework with SQL Server
builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PortfolioDb")));

// Register HttpClient factory
builder.Services.AddHttpClient();

// Register custom services for the search engine
builder.Services.AddSingleton<IndexService>();
builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<CrawlerService>();
builder.Services.AddScoped<ContentService>();
builder.Services.AddScoped<AiService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
