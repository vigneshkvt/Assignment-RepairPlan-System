using System.Text.Json;
using Microsoft.AspNetCore.OData;
using RL.Data;
using MediatR;
using FluentValidation;
using RL.Backend.Validators;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddMediatR(typeof(Program));
builder.Services.AddSqlite<RLContext>("Data Source=Database.db");
builder.Services.AddControllers()
    .AddOData(options => options.Select().Filter().Expand().OrderBy())
    .AddJsonOptions(options => options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters(); // Enable FluentValidation integration

builder.Services.AddValidatorsFromAssemblyContaining<AddUserToProducerValidator>(); // Register all validators

builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<EnableQueryFiler>();
});
var corsPolicy = "allowLocal";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicy,
    policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                return origin.StartsWith("http://localhost");
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RL v1");
    c.RoutePrefix = string.Empty;
});


app.UseCors(corsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();