using EBookBuilder.Api.Data;
using EBookBuilder.Api.Services.Books;
using EBookBuilder.Api.Services.Cleaning;
using EBookBuilder.Api.Services.Pdf;
using EBookBuilder.Api.Services.Word;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License =
    LicenseType.Community;

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"
        )
    );
});

builder.Services.AddScoped<
    IBookService,
    BookService
>();

builder.Services.AddScoped<
    IWordDocumentService,
    WordDocumentService
>();

builder.Services.AddScoped<
    IContactCleanerService,
    ContactCleanerService
>();

builder.Services.AddScoped<
    IPdfDocumentService,
    PdfDocumentService
>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");

app.UseStaticFiles();

app.MapControllers();

app.Run();