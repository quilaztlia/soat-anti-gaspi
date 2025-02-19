using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using SendGrid.Extensions.DependencyInjection;
using Soat.AntiGaspi.Api.BackgroundJobs;
using Soat.AntiGaspi.Api.Constants;
using Soat.AntiGaspi.Api.Repository;
using Soat.AntiGaspi.Api.Time;

namespace Soat.AntiGaspi.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddControllers()
            .AddFluentValidation(s => s.RegisterValidatorsFromAssemblyContaining<Program>());

        builder.Services.AddSingleton<IDateTimeOffset, DateTimeOffsetProvider>();

        builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSendGrid(options => options.ApiKey = builder.Configuration[AppSettingKeys.SendGridApiKey]);

        builder.Services.AddHostedService<CleanContactsJob>();

        builder.Services.AddDbContext<AntiGaspiContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("AntiGaspiContext")));

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}