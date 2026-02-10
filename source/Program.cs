using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Domain.Services;
using MinimalAPI.Enums;
using MinimalAPI.Infrastructure;
using Scalar.AspNetCore;
using System.Text;

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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("InternalFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "https://localhost:7082") // Substitua pelo URL do seu frontend
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// EDITOR DE DOCUMENTAÇÃO OPENAPI (Swagger/Scalar)
builder.Services.AddOpenApi(options =>
{
    // Transformação do documento OpenAPI para adicionar informações e suporte a JWT
    options.AddDocumentTransformer((document, context, CancellationToken) =>
    {
        // Metadados básicos do documento OpenAPI
        document.Info = new()
        {
            Title = "Minimal API - Sistema de Veiculos",
            Version = "v1"
        };

        // Adicionando suporte a JWT no Scalar
        document.Components ??= new();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        // Definição do esquema de segurança para JWT Bearer
        // Isso descreve como o JWT deve ser enviado (no header Authorization) e o formato esperado (Bearer)
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http, // Autenticação HTTP
            Scheme = "bearer", // padrão JWT
            BearerFormat = "JWT", // formato do token
            In = ParameterLocation.Header, // Vai no Header
            Name = "Authorization", // Nome do header
            Description = "JWT Authorization header using the Bearer scheme."
        });

        // 🔐 Exige JWT globalmente (necessário para o Scalar)
        // Esse trecho indica que todas as operações na API exigem autenticação JWT, a menos que seja explicitamente permitido o acesso anônimo.
        document.Security = new List<OpenApiSecurityRequirement>
        {
            new OpenApiSecurityRequirement
            {
                {
                    // Referência ao esquema de segurança definido acima (Bearer)
                    new OpenApiSecuritySchemeReference(
                        "Bearer", // referenceId
                        document // OpenApiDocument (can be null, but here we pass the current document)                      
                    ),
                    new List<string>()
                }
            }
        };

        return Task.CompletedTask;
    });
});

// DbContext
builder.Services.AddDbContext<MinimalApiContext>(options =>
{
    options.UseMySql(connectionString, serverVersion);
});

builder.Services.AddHttpContextAccessor();

// Services
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserContext, HttpContextUserContext>();
builder.Services.AddScoped<IHttpContext, HttpContextService>();

// Jwt
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        var secretKey = builder.Configuration["Jwt:Key"]!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Permitir leitura do token JWT do cookie
                context.Token = context.Request.Cookies["access_token"];
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(Cargo.Admin.ToString()));
});

#endregion

#region APP
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

/* ========= MIDDLEWARE CORS ========
 * O Cors é um mecanismo de segurança que restringe os reponses a requisições feitas por scripts rodando em um domínio diferente do domínio da API.
 * Mas o problema é que o Cors apenas bloqueia a resposta, ou seja, a requisição ainda chega na API, 
 *      e isso pode ser um problema para ataques CSRF (Cross-Site Request Forgery).
 * 
 * Onde se o um usuário malicioso criar um site com um script que faz requisições para a API, o Cors não irá bloquear essas requisições. 
 */

/* ======== MIDDLEWARE CSRF ========
 * O CSRF é um tipo de ataque onde um usuário malicioso engana um usuário autenticado para fazer uma requisição indesejada em uma aplicação web.
 * Pois como estamos enviado e recebendo o token JWT via cookie, 
 *      o navegador irá enviar automaticamente o cookie em todas as requisições para a API, mesmo que a requisição seja feita por um site malicioso.
 * 
 * Portanto, criamos um Middleware para validar o token CSRF, onde o cliente deve enviar um token CSRF no header X-CSRF-Token, 
 *      e esse token deve ser igual ao token CSRF armazenado no cookie.
 * 
 * Assim conseguimos validar se a requisição é legítima (feita pelo cliente) ou se é um ataque CSRF (feita por um site malicioso).
 */

/* ======= RESUMO ======
 * O CORS serve para bloquear ataques de XSS (Cross-Site Scripting), onde um site malicioso tenta acessar a API diretamente do navegador.
 * Impedindo que o script malicioso consiga ler a resposta da API, mas ele ainda pode fazer requisições para a API.
 * 
 * Então o CRSF Token serve para bloquear ataques de CSRF (Cross-Site Request Forgery),
 *      onde um site malicioso engana um usuário autenticado para fazer uma requisição indesejada em uma aplicação web.
 * 
 */
app.UseCors("InternalFrontend");

app.Use(async (context, next) =>
{
    if (HttpMethods.IsPost(context.Request.Method) ||
        HttpMethods.IsPut(context.Request.Method) ||
        HttpMethods.IsPatch(context.Request.Method))
    {
        var csrfCookie = context.Request.Cookies["XSRF-TOKEN"];
        var csrfHeader = context.Request.Headers["X-CSRF-Token"].FirstOrDefault();

        if (string.IsNullOrEmpty(csrfCookie) || string.IsNullOrEmpty(csrfHeader) || csrfCookie != csrfHeader)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;            
            return;
        }

        await next();
    }
});

app.UseAuthentication();

/* ========= MIDDLEWARE AUTHENTICATION =========
 * ====== RESUMO ======
 * Toda requisição que for feita pelo Cliente, irá passar por esse Middleware.
 * Ele irá validar o Token JWT com base nas configurações que definimos no **builder**.
 * Após a validação, ele extrai as claims, criando um **ClaimsPrincipal**
 * Define HttpContext.User = ClaimsPrincipal

 ### 📚 O que é ClaimsPrincipal?
 ClaimsPrincipal = Objeto que representa o **usuário autenticado** com todas as suas **claims**.
 
 ### Fluxo Completo:
 * 1. Cliente faz requisição com token JWT no header
   Authorization: Bearer eyJhbGci...
   ↓
 * 2.Middleware de Autenticação intercepta
   ↓
 * 3. Middleware DECODIFICA o token JWT
   ↓
 * 4. Middleware EXTRAI as claims do token
   ↓
 * 5. Middleware CRIA um ClaimsPrincipal com essas claims
   ↓
 * 6. Middleware ADICIONA ClaimsPrincipal ao HttpContext.User
   ↓
 * 7. Seu código acessa HttpContext.User
*/

app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
#endregion

#region ENDPOINTS

#region === AUTH ===

var auth = app.MapGroup("api/auth").WithTags("Autenticação");

// CADASTRO USUÁRIO
auth.MapPost("/register", (IAuthService service, [FromBody] RegisterDto dto) =>
{
    var result = service.RegisterUser(dto, Cargo.User);

    if (!result.Success)
    {
        switch (result.ErrorType)
        {
            case AuthErrorType.EmailAlreadyExists:
                return Results.Conflict(new { message = result.Message });
            default:
                return Results.Unauthorized();
        }
    }

    return Results.Created($"/api/users/{result.Data?.Id}", result.TokenResponse);
})
    .AllowAnonymous()
    .WithName("Registro")
    .WithSummary("Criar nova conta de usuário.");

// CADASTRO ADMIN
auth.MapPost("/admin/register", (IAuthService service, [FromBody] RegisterDto dto) =>
{
    var result = service.RegisterUser(dto, Cargo.Admin);

    if (!result.Success)
    {
        switch (result.ErrorType)
        {
            case AuthErrorType.EmailAlreadyExists:
                return Results.Conflict(new { message = result.Message });
            default:
                return Results.Unauthorized();
        }
    }

    return Results.Created($"/api/admin/{result.Data?.Id}", result.TokenResponse);
})
    .RequireAuthorization("AdminOnly")
    .WithName("Registro Admins")
    .WithSummary("Criar uma nova conta de administrador.");

// LOGIN
auth.MapPost("/login", (IAuthService service, IHttpContext httpContext, [FromBody] LoginDTO dto) =>
{
    var result = service.ValidateLogin(dto);

    if (!result.Success)
    {
        switch (result.ErrorType)
        {
            case AuthErrorType.InvalidCredentials:
                return Results.Unauthorized();
            case AuthErrorType.UserNotFound:
                return Results.Unauthorized();
            case AuthErrorType.InactiveAccount:
                return Results.Problem(
                    statusCode: 403,
                    title: "Conta inativa",
                    detail: result.Message
                );
            default:
                return Results.Unauthorized();
        }
    }

    // JWT - HttpOnly
    httpContext.GerenateCookie(
        key: "access_token",
        jwt: result.TokenResponse!.Token,
        cookieOptions: new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = result.TokenResponse.ExpiresAt
        }
    );

    // CSRF - Não HttpOnly
    httpContext.GerenateCookie(
        key: "XSRF-TOKEN",
        jwt: httpContext.GenerateCsrfToken(),
        cookieOptions: new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"            
        }
    );

    return Results.Ok(result.TokenResponse);
})
    .AllowAnonymous()
    .WithName("Login")
    .WithSummary("Fazer login e obter token JWT.");

// PEGAR USUÁRIO
auth.MapGet("/me", (IAuthService service) =>
{
    var result = service.GetMe();

    if (!result.Success)
    {
        switch (result.ErrorType)
        {
            case AuthErrorType.Unauthorized:
                return Results.Unauthorized();
            case AuthErrorType.UserNotFound:
                return Results.NotFound();
        }
    }

    return Results.Ok(result.Data);

})
    .RequireAuthorization()
    .WithName("GetMe")
    .WithSummary("Obter dados do usuário autenticado.");
#endregion

#region === USUÁRIOS ===

var users = app.MapGroup("api/users").WithTags("Usuários").RequireAuthorization();

// BUSCAR USUÁRIO POR ID
users.MapGet("/{id}", (IUserService service, int id) =>
{
    var user = service.GetUserById(id);
    if (user is null) return Results.NotFound();
    return Results.Ok(user);
})
   .WithName("GetUserById")
   .WithSummary("Pegar um usuário pelo seu Id.");
#endregion

#region === ADMINS ===

var admin = app.MapGroup("api/admin").WithTags("Administradores").RequireAuthorization("AdminOnly");

// BUSCAR ADMIN POR ID
admin.MapGet("/{id}", (IAdminService service, int id) =>
{
    var admin = service.GetAdminById(id);

    if (admin is null) return Results.NotFound();

    return Results.Ok(new
    {
        id = admin.Id,
        email = admin.Email,
        cargo = admin.Cargo.ToString()
    });
})
    .WithName("GetAdminById")
    .WithSummary("Pegar um admin pelo seu Id.");

// LISTAR TODOS USUÁRIOS (COM FILTROS)
admin.MapGet("/all", (IAdminService service,
                        int pagina = 1,
                        int pageSize = 10,
                        bool ordenarCrescente = false,
                        string? email = null,
                        Cargo? cargo = null) =>
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
})
    .WithName("ListAllUsers")
    .WithSummary("Lista dos os usuários e admins.");
#endregion

#region === VEÍCULOS ===

var veiculos = app.MapGroup("api/veiculos").WithTags("Veiculos");

// CRIAR 
veiculos.MapPost("/", (IVeiculoService service, IUserContext userContext, [FromBody] AddVeiculoDTO dto) =>
{
    if (!userContext.TryGetUserId(out int userId))
        return Results.Unauthorized();

    var veiculo = service.AddVeiculo(dto);

    return Results.Created($"api/veiculos/{veiculo.Id}", veiculo);
})
    .RequireAuthorization()
    .WithName("CreateVeiculo")
    .WithSummary("Cadastrar veículos.");

// LISTAR VEICULOS COM FILTROS
veiculos.MapGet("/", (IVeiculoService service,
                        int pagina = 1,
                        int pageSize = 10,
                        bool ordenarCrescente = false,
                        string? nome = null,
                        string? marca = null,
                        string? cor = null,
                        int? ano = null) =>
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
})
    .AllowAnonymous()
    .WithName("ListAllVeiculos")
    .WithSummary("Listagem dos veículos.");

// BUSCAR UM VEICULO ESPECIFICO
veiculos.MapGet("/{id}", (IVeiculoService service, [FromRoute] int id) =>
{
    var resultado = service.GetSpecificById(id);

    if (resultado is null) return Results.NotFound();

    return Results.Ok(resultado);
})
    .AllowAnonymous()
    .WithName("GetVeiculosById")
    .WithSummary("Obter veículo específico.");


// DELETAR
veiculos.MapDelete("/{id}", (IVeiculoService service, int id) =>
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
})
    .RequireAuthorization(policy => policy.RequireRole(Cargo.Admin.ToString()))
    .WithName("DeleteVeiculos")
    .WithSummary("Remoção dos veículos.");

// ATUALIZAR
veiculos.MapPatch("/veiculos/{id}", (IVeiculoService service, int id, [FromBody] UpdateVeiculoDTO dto) =>
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

})
    .RequireAuthorization()
    .WithName("UpdateVeiculos")
    .WithSummary("Atualização dos veículos.");

#endregion

#endregion

app.Run();