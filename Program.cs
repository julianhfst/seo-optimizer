using Microsoft.AspNetCore.Cors.Infrastructure;
using SEO_Optimizer.Interfaces;
using SEO_Optimizer.Services;
using SEO_Optimizer.Utils;

var builder = WebApplication.CreateBuilder(args);


// var corsBuilder = new CorsPolicyBuilder();
// //corsBuilder.WithOrigins("http://localhost:7193"); // for a specific url. Don't add a forward slash on the end!
// // corsBuilder.AllowCredentials();
//
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("SiteCorsPolicy", corsBuilder.Build());
// });

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var openAiSettings = builder.Configuration.GetSection("OpenAiSettings").Get<OpenAiSettings>();
var apiKey = builder.Configuration.GetValue<string>("openAIKey");
builder.Services.AddScoped<IOpenAiInterface>((serviceProvider) =>
{
    var logger = serviceProvider.GetRequiredService<ILogger<OpenAiService>>();
    return new OpenAiService(apiKey, logger, openAiSettings);
});
builder.Services.AddSingleton<PromptBuilder>();


builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//HTML Serve
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors("SiteCorsPolicy"); //CORS

app.UseAuthorization();

app.MapControllers();

app.Run();