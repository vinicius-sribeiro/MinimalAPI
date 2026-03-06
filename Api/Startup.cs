using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MinimalAPI;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Domain.Services;
using MinimalAPI.Enums;
using MinimalAPI.Infrastructure;
using Scalar.AspNetCore;
using System.Text;

/* Por que usar a classe Startup?
    Essa classe é criada para podermos separar arquiteturalmente o inicilização da aplicação: Program.cs, das configurações de serviços e middlewares: Startup.cs.
    
    Assim podemos usar a classe Startup tanto para o ambiente de desenvolvimento, quanto para os testes de integração, garantindo que a configuração seja a mesma em ambos os cenários.

    E separando as responsabilidades, deixando o Program.cs mais limpo e focado apenas em iniciar a aplicação, enquanto a Startup.cs fica responsável por toda a configuração de serviços, middlewares e endpoints.
 */

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");        

        // DbContext
        services.AddDbContext<MinimalApiContext>(options =>
        {
            options.UseMySql(
                connectionString, 
                ServerVersion.AutoDetect(connectionString)
            );
        });

        // CORS
        services.AddCors(options =>
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
        services.AddOpenApi(options =>
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

        services.AddHttpContextAccessor();

        // Services
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IVeiculoService, VeiculoService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserContext, HttpContextUserContext>();
        services.AddScoped<IHttpContext, HttpContextService>();

        // Jwt
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                var secretKey = Configuration["Jwt:Key"]!;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Audience"],
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


        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole(Cargo.Admin.ToString()));
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {        
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("InternalFrontend");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            if (env.IsDevelopment())
            {
                endpoints.MapOpenApi();

                endpoints.MapScalarApiReference(options =>
                {
                    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
                    options.Title = "API Documentation";
                    options.Theme = ScalarTheme.Default;
                    options.Authentication = new ScalarAuthenticationOptions
                    {
                        PreferredSecuritySchemes = new[] { "Bearer" }
                    };
                });

                using (var scope = app.ApplicationServices.CreateScope())
                {
                    // Usamos isso para simular uma chamada de serviço HTTP, para podermos usar o Seed
                    var context = scope.ServiceProvider.GetRequiredService<MinimalApiContext>();

                    context.Database.Migrate();
                    /*
                        • Cria o banco
                        • Aplica todas migrations
                        Então não precisa detectar manualmente se o banco existe.
                     */

                    if (env.IsDevelopment() && !context.Usuarios.Any())
                    {
                        DbInitializer.Seed(context);
                    }
                }

            }

            #region =========== ENDPOINTS ===========

            #region === AUTH ===

            var auth = endpoints.MapGroup("api/auth").WithTags("Autenticação");

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
                            return Results.NotFound();
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

            var users = endpoints.MapGroup("api/users").WithTags("Usuários").RequireAuthorization();

            // BUSCAR USUÁRIO POR ID
            users.MapGet("/{id}", (IUsuarioService service, int id) =>
            {
                var user = service.GetUserById(id);
                if (user is null) return Results.NotFound();
                return Results.Ok(user);
            })
               .WithName("GetUserById")
               .WithSummary("Pegar um usuário pelo seu Id.");
            #endregion

            #region === ADMINS ===

            var admin = endpoints.MapGroup("api/admin").WithTags("Administradores").RequireAuthorization("AdminOnly");

            // BUSCAR ADMIN POR ID
            admin.MapGet("/{id}", (IUsuarioService service, int id) =>
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
            admin.MapGet("/all", (IUsuarioService service,
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

            var veiculos = endpoints.MapGroup("api/veiculos").WithTags("Veiculos");

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
        });
    }
}
