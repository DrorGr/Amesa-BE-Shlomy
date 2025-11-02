using AMESA_be.LotteryDevice.Data;
using AMESA_BE.LotteryService.Helpers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#region ConnectionString
var connectionString = builder.Configuration.GetConnectionString("LotteryDbConnection");
builder.Services.AddDbContext<LotteryDbContext>(options =>
    options.UseNpgsql(connectionString));
#endregion
// Add services to the container.

#region Services injection
builder.Services.AddLotteryServices();
#endregion

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
