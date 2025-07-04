using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Adiciona controllers
builder.Services.AddControllers();

// Necessário para gerar descrição de endpoints
builder.Services.AddEndpointsApiExplorer();


string titulo_api = "PdvNet - API V1";

// Configura o SwaggerGen
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = titulo_api,
        Version = "v1",
        Description = "Documentação gerada pelo Swagger"
    });
});


builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Habilita middleware do Swagger JSON e UI
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", titulo_api);
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();
