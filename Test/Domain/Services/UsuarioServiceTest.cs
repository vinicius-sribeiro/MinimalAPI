using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Configuration;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Models;
using MinimalAPI.Domain.Services;
using MinimalAPI.Enums;
using MinimalAPI.Infrastructure;
using MinimalAPI_Test.Domain.FakeServices;
using MySqlConnector;

namespace MinimalAPI_Test;


// === Integration Test com banco real ===
[TestClass]
public class UsuarioServiceTest
{
    private static string _connectionString = string.Empty;
    private static IConfiguration _configuration = default!;

    [ClassInitialize]
    public static void Setup(TestContext testContext)
    {
        _configuration = new ConfigurationBuilder()
            .AddUserSecrets(typeof(UsuarioServiceTest).Assembly)
            .Build();

        _connectionString = _configuration.GetConnectionString("TestConnection") ??
            throw new InvalidOperationException("The connectionString was not found.");

        EnsureDatabaseExists(_connectionString);

        var options = new DbContextOptionsBuilder<MinimalApiContext>()
            .UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString))
            .Options;

        using var db = new MinimalApiContext(options);

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        /* O que isso faz?

            • Lê o modelo atual do DbContext
            • Gera o SQL necessário
            • Cria TODAS as tabelas
            • Cria chaves, índices e relacionamentos

        Sem migrations.
        Sem histórico.
        Direto.
         */
    }

    private static void EnsureDatabaseExists(string connectionString)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;

        builder.Database = "";

        /* Por que tiramos o "Database=..." da connectionString?
            Pois primeiro queremos nós conectar apenas no servidor, para depois no Database.
            Se deixassemos "Database=...", ele tentaria conectar direto no Database, o que poderia gerar erro.
         */

        using var connection = new MySqlConnection(builder.ConnectionString);
        connection.Open(); // Faz a conexão com o servidor 

        using var command = connection.CreateCommand();
        command.CommandText =
            $"CREATE DATABASE IF NOT EXISTS `{databaseName}`";
        command.ExecuteNonQuery();
        /* ExecuteNonQuery()
         
        <<ExecuteNonQuery()>> é usado quando o SQL não retorna **resultado tabular**.
        Esse método é usado quando queremos apenas executar uma ação que não retornará nenhum dataset, apenas executar algo.

        -- O que ele faz internamente?

        O driver MySQL:
           1. Envia o SQL pelo protocolo MySQL.
           2. O servidor executa.
           3. Retorna:
              • Número de linhas afetadas (para INSERT/UPDATE/DELETE).
              • 0 para CREATE/DROP.
           4. O método retorna um int
          
        O objetivo é:
            |   Disparar o comando no servidor.
         */
    }

    private MinimalApiContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MinimalApiContext>()
            .UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString))
            .Options;

        return new MinimalApiContext(options);
    }

    [TestMethod]
    [DoNotParallelize]
    public void Register_DeveCriarUsuario()
    {
        //Arrange
        using var context = CreateContext();

        // test isolation cleanup (pode ser feito com Transaction Rollback)
        context.Usuarios.RemoveRange(context.Usuarios);
        context.SaveChanges();
        
        //var tokenService = new TokenService(_configuration!);
        var tokenService = new TokenServiceFake();
        var httpContextAccessor = new HttpContextAccessor();
        var userContext = new HttpContextUserContext(httpContextAccessor);

        var dataCriacao = DateTime.Now;
        var dto = new RegisterDto("Paulo", "paulo@teste.com", "senha123");
        var authService = new AuthService(context, tokenService, userContext);

        // Act

        var result = authService.RegisterUser(dto, Cargo.Admin);

        var adm = result.Data!;

        // Assert
        Assert.AreEqual(1, context.Usuarios.Count());
        Assert.AreEqual("Paulo", adm.Nome);
        Assert.AreEqual("paulo@teste.com", adm.Email);
        // Assert.AreEqual("senha123", adm.SenhaHash);
        Assert.IsTrue(BCrypt.Net.BCrypt.Verify("senha123", adm.SenhaHash));
        Assert.AreEqual(Cargo.Admin, adm.Cargo);
        Assert.IsTrue(adm.isAtivo);
    }

    [TestMethod]
    [DoNotParallelize]
    public void Get_UsuarioById()
    {
        //Arrange
        using var context = CreateContext();
        context.Usuarios.RemoveRange(context.Usuarios);
        context.SaveChanges();
        
        var tokenService = new TokenService(_configuration!);
        var httpContextAccessor = new HttpContextAccessor();
        var userContext = new HttpContextUserContext(httpContextAccessor);

        var dataCriacao = DateTime.Now;
        var dto = new RegisterDto("Paulo", "paulo@teste.com", "senha123");
        var authService = new AuthService(context, tokenService, userContext);

        var userService = new UsuarioService(context);
        // Act

        var result = authService.RegisterUser(dto, Cargo.Admin);

        var adm = result.Data!;

        var user = userService.GetUserById(adm.Id);

        // Assert
        Assert.AreEqual(1, user?.Id);
    }
}

