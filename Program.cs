using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductImportApi.Services;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom settings
builder.Services.Configure<CsvSettings>(builder.Configuration.GetSection("CsvSettings"));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IConfiguration>().GetSection("CsvSettings").Get<CsvSettings>());

// Register dependencies
builder.Services.AddSingleton<ProductService>();
builder.Services.AddSingleton<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();