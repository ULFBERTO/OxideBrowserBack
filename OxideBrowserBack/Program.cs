using OxideBrowserBack.Services;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:4200", "http://localhost:4201", "http://127.0.0.1:4200")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

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
