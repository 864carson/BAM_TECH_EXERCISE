using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Logging;
using StargateAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
AppConfig? preciseConfig = builder.Configuration.GetSection("AppConfig").Get<AppConfig>();
builder.Services.AddSingleton<AppConfig>();

builder.Services.AddTransient<IStargateRepository, StargateRepository>();
builder.Services.AddTransient(typeof(ILoggingRepository<,>), typeof(LoggingRepository<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<StargateContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("StarbaseApiDatabase")));

builder.Services.AddMediatR(cfg =>
{
    cfg.AddRequestPreProcessor<CreatePersonPreProcessor>();
    cfg.AddRequestPreProcessor<UpdatePersonPreProcessor>();
    cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});

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
