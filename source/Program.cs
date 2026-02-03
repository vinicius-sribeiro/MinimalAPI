using Scalar.AspNetCore;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using MinimalAPI.Domain.Models;
using System.Security.Authentication;
using MinimalAPI.Enums;

var builder = WebApplication.CreateBuilder(args);

#region EXPLANATION THEMES

#region .NET CONTAINER DI 
/*
 *  Esse container serve para a criação das instâncias dos serviços ou objetos que os endpoints irão utilizar.
 *  
 *  Ele é responsável por gerenciar o ciclo de vida desses objetos (SERVICE LIFETIME).
 *  
 *  Ele também resolve as dependências entre os objetos, ou seja, quando um objeto depende de outro (DEPENDENCY INJECTION). 
 *  
 *  Por causa dele, em vez de criar as instâncias manualmente, os endpoints podem simplesmente solicitar o objeto necessário ao container.
 *  
 *  - Manualmente:
 *      app.MapGet("/endpoint", () => {
 *          var servico = new MeuServico();
 *          return servico.FazerAlgo();
 *      });
 *      
 *  - Container DI:
 *      app.MapGet("/endpoint", (IMeuServico servico) => { 
 *          return servico.FazerAlgo();
 *      });
 */
#endregion

#region DEPENDENCY INJECTION (DI)
/* 
 * A injeção de dependência (DI) é um padrão de design que permite a um objeto receber suas dependências de fontes externas,
 *   em vez de criar essas dependências internamente.
 */
#endregion

#region SERVICE LIFETIME
/* 
 * 
 * - Transient: Uma nova instância é criada cada vez que o serviço é solicitado.
 *              Pode criar mais de uma instância por requisição.
 *              Tempo de vida curto (tempo da instânciação do objeto).
 *             
 * - Scoped: Uma única instância é criada por requisição.
 *           Serviços que dependem do DbContext geralmente são registrados como Scoped.
 *           Tempo de vida intermediário (tempo da requisição).
 *           
 * - Singleton: Uma única instância é criada e compartilhada durante toda a vida útil da aplicação.
 *              Tempo de vida longo (tempo da aplicação até ela acabar).
 * 
 * Ordem de dependência:
 *      - Transient pode depender de Scoped e Singleton.
 *      - Scoped pode depender de Singleton.
 *      - Singleton não deve depender de Scoped ou Transient.
 */
#endregion

#endregion

#region BUILDER AND SERVICES
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
     throw new InvalidOperationException("Connection string 'DefaultConnection' not found."); ;

var serverVersion = ServerVersion.AutoDetect(connectionString);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<MinimalApiContext>(options =>
{
    options.UseMySql(connectionString, serverVersion);
});


builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();
#endregion

#region APP
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
        .WithTitle("My Minimal API Documentation")
        .WithTheme(ScalarTheme.Purple)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();

app.UseHttpsRedirection();
#endregion

#region ROUTES

#region Usuarios
/* ===================
 *  USUÁRIOS
 * ===================
 */

// CADASTRO USUÁRIO
app.MapPost("/user/sign-in", (IUserService service, [FromBody] LoginDTO dto) =>
{
    var newUser = service.CreateUser(dto);

    return Results.CreatedAtRoute("GetUserById", new { id = newUser.Id }, newUser);
}).WithTags("Usuários");

// LOGIN USUÁRIO
app.MapPost("/user/login", (IUserService service, [FromBody] LoginDTO dto) =>
{
    if (!service.ValidateUserLogin(dto))
    {
        return Results.NotFound();
    }

    return Results.Ok(new { message = "Login com sucesso!" });
}).WithTags("Usuários");

// BUSCAR USUÁRIO POR ID
app.MapGet("/user/{id}", (IUserService service, int id) =>
{
    var user = service.GetUserById(id);
    if (user is null) return Results.NotFound();
    return Results.Ok(user);
}).WithTags("Usuários").WithName("GetUserById");
#endregion

#region Admins
/* ===================
 *  ADMINS
 * ===================
 */

// CADASTRO ADMIN
app.MapPost("/admin/sign-in", (IAdminService service, [FromBody] LoginDTO dto) =>
{
    var newAdmin = service.CreateAdmin(dto);

    return Results.CreatedAtRoute("GetAdminById", new { id = newAdmin.Id }, newAdmin);
}).WithTags("Admins");

// LOGIN ADMIN
app.MapPost("/admin/login", (IAdminService service, [FromBody] LoginDTO dto) =>
{
    if (!service.ValidateAdminLogin(dto))
        return Results.Unauthorized();

    return Results.Ok(new { message = "Login com sucesso!" });

}).WithTags("Admins");

// BUSCAR ADMIN POR ID
app.MapGet("/admin/{id}", (IAdminService service, int id) =>
{
    var admin = service.GetAdminById(id);

    if (admin is null) return Results.NotFound();

    return Results.Ok(admin);
}).WithTags("Admins").WithName("GetAdminById");

// LISTAR TODOS USUÁRIOS (COM FILTROS)
app.MapGet("/admin/users",
    (   IAdminService service,
        int pagina = 1,
        int pageSize = 10,
        bool ordenarCrescente = false,
        string? email = null,
        Cargos? cargo = null) =>
{
    var filters = new AllUserListFilterDTO(
        pagina,
        pageSize,
        email,
        cargo,
        ordenarCrescente
    );

    var resultado = service.ListAllUsers(filters);

    return Results.Ok(resultado);
}).WithTags("Admins");
#endregion

#region Veiculos
/* ===================
 *  VEICULOS
 * ===================
 */

// CRIAR 
app.MapPost("/veiculos", (IVeiculoService service, [FromBody] AddVeiculoDTO dto) =>
{
    var veiculo = service.AddVeiculo(dto);

    return Results.CreatedAtRoute("GetVeiculoById", new { id = veiculo.Id }, veiculo);
}).WithTags("Veiculos");

// LISTAR VEICULOS COM FILTROS
app.MapGet("/veiculos",
    (IVeiculoService service,
    int pagina = 1,
    int pageSize = 10,
    bool ordenarCrescente = false,
    string? nome = null,
    string? marca = null,
    string? cor = null,
    int? ano = null
    ) =>
{
    var filters = new VeiculoListFilterDTO(
        pagina,
        pageSize,
        nome,
        marca,
        cor,
        ano,
        ordenarCrescente
    );

    var resultado = service.ListAll(filters);

    return Results.Ok(resultado);
}).WithTags("Veiculos");

// BUSCAR UM VEICULO ESPECIFICO
app.MapGet("/veiculos/{id}", (IVeiculoService service, [FromRoute] int id) =>
{
    var resultado = service.GetSpecificById(id);

    if (resultado is null) return Results.NotFound();

    return Results.Ok(resultado);
}).WithTags("Veiculos").WithName("GetVeiculoById");


// DELETAR
app.MapDelete("/veiculos/{id}", (IVeiculoService service, int id) =>
{
    try
    {
        service.RemoveById(id);
    }
    catch
    {
        return Results.NotFound();
    }

    return Results.Ok(new { message = "Veiculo removido com sucesso!" });
}).WithTags("Veiculos");

// ATUALIZAR
app.MapPatch("/veiculos/{id}", (IVeiculoService service, int id, [FromBody] UpdateVeiculoDTO dto) =>
{
    try
    {
        service.UpdateVeiculo(id, dto);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }

    return Results.NoContent();

}).WithTags("Veiculos");

#endregion

#endregion

app.Run();