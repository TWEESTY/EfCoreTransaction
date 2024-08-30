using EfCoreUnitOfWork.Context;
using EfCoreUnitOfWork.Repositories;
using EfCoreUnitOfWork.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Zejji.Entity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite("Data Source=:memory:"));
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IFakeService, FakeService>();

var dbContextFactory = new RegisteredDbContextFactory();
dbContextFactory.RegisterDbContextType<AppDbContext>(() =>
{
    var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();
    var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

    return new AppDbContext(options);
});

builder.Services.AddSingleton<IDbContextFactory>(dbContextFactory);



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
