using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Infrastructure;
using MinimalAPI_Test.Mocks;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using MinimalAPI.Domain.Services;

namespace MinimalAPI_Test.Helpers;

/* Por que criar uma classe de configuração para os testes de integração?
    Classe de configuração para os testes de integração. Ela é responsável por configurar o ambiente de teste, incluindo a criação do servidor web simulado e a configuração do banco de dados em memória.

    Aqui está sendo definida para realizarmos testes de HTTP contra a aplicação, sem precisar iniciar um servidor real.
 */

public class Setup
{
    public const string PORT = "5001";
    public TestContext TestContext { get; private set; } = default!;
    /*TestContext:
        É um objeto fornecido pelo MSTest que contém informações sobre o contexto de execução do teste.
    
        Exmplo de uso:
            • Nome do teste atual
            • Logs
            • Arquivos de output
        
        === Porque ele não pode ser static? ===

        Por que os testes podem rodar em paralelo.
        
        Se fosse <<static>>:
            Teste A altera TestContext
            Teste B altera TestContext

        Isso quebraria o isolamento dos testes.
    
        The error MSTEST0024 indicates that TestContext should not be stored in static members because it can cause issues with test isolation and parallel test execution. 
        Each test should have its own instance of TestContext.
     */
    public WebApplicationFactory<Startup> Http { get; private set; } = default!;
    /* WebApplicationFactory:
        Esse é o componente que simula o servidor web para os testes de integração. 
        Ele permite que você faça requisições HTTP para a aplicação como se fosse um cliente real, mas sem precisar iniciar um servidor real.

        • Inicia a aplicação em um ambiente de teste
        • Cria o pipeline HTTP completo, incluindo middlewares, roteamento e controladores
        • Executa tudo na memória, sem abrir portas de rede reais.

        Nesse exemplo, ele usa a classe Startup da aplicação para configurar o ambiente de teste, mas você pode personalizar a configuração para usar bancos de dados em memória, serviços mockados, etc.
     */
    public HttpClient HttpClient { get; private set; } = default!;
    /* HttpClient:
        Esse é o cliente HTTP que será usado para enviar requisições para a aplicação durante os testes de integração.       
     */

    public void Initialize(TestContext testContext)
    {
        TestContext = testContext;
        Http = new WebApplicationFactory<Startup>();

        // Configura o ambiente de teste para a aplicação
        Http = Http.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("https_port", PORT);

            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Clear existing configuration sources
                config.Sources.Clear();

                // Add for only test configuration
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=test;User=test;Password=test;",
                    ["Jwt:Key"] = "test-secret-key-for-jwt-token-generation-must-be-long-enough",
                    ["Jwt:Issuer"] = "test-issuer",
                    ["Jwt:Audience"] = "test-audience"
                }!);
            });

            builder.ConfigureServices(services => {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<MinimalApiContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<MinimalApiContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });

                // Replace with mock services
                services.AddScoped<IUsuarioService, UsuarioServiceMock>();
                services.AddScoped<IAuthService, AuthServiceMock>();             
            });
        });

        HttpClient = Http.CreateClient();
    }
}