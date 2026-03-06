using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Models;
using MinimalAPI.Domain.Services;
using MinimalAPI.Enums;
using MinimalAPI_Test.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MinimalAPI_Test.Requests
{
    [TestClass]
    public class UsuarioRequestTest
    {
        public TestContext TestContext { get; set; }
        public Setup Setup { get; set; } = new Setup();     

        [TestInitialize]
        public void Inicializar()
        {
            Setup.Initialize(TestContext);          
        }        

        [TestMethod]       
        public async Task Get_UsuarioById()
        {
            //Arrange
            var dtoLogin = new LoginDTO("admin@teste.com", "123456");

            var content = new StringContent(JsonSerializer.Serialize(dtoLogin), Encoding.UTF8, "Application/json");

            // Act

            var response = await Setup.HttpClient.PostAsync("/api/auth/login", content);

            // Assert
            Assert.AreEqual(200, (int)response.StatusCode);

            var restult = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<TokenResponseDTO>(restult, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });      
            
            Assert.IsNotNull(token);             
            Assert.IsNotNull(token.Usuario.Email);
            Assert.IsNotNull(token.Usuario?.Id ?? -1);


        }
    }
}
