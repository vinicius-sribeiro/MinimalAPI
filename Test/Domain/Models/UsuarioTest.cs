using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalAPI.Domain.Models;
using MinimalAPI.Enums;
using MinimalAPI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace MinimalAPI_Test.Domain.Models
{
    [TestClass]
    public sealed class UsuarioTest
    {  

        [TestMethod]
        public void TestGetSetProperties()
        {
            // Arrange
            // Onde criaremos todas as variáveis necessárias para o teste, como instâncias de objetos, valores de entrada, etc.
            var dataCriacao = DateTime.Now;

            var adm = new Usuario
            {
                Id = 10,
                Nome = "Paulo",
                Email = "paulo@teste.com",
                SenhaHash = "senha123",
                Cargo = Cargo.Admin,
                isAtivo = true,
                DataCriacao = dataCriacao
            };

            // Act
            // Onde executaremos a ação que queremos testar, como chamar um método ou acessar uma propriedade.                        

            // Assert
            // Onde verificaremos se o resultado da ação é o esperado, usando asserções para comparar            
            Assert.AreEqual(10, adm.Id);
            Assert.AreEqual("Paulo", adm.Nome);
            Assert.AreEqual("paulo@teste.com", adm.Email);
            Assert.AreEqual("senha123", adm.SenhaHash);
            Assert.AreEqual(Cargo.Admin, adm.Cargo);            
            Assert.IsTrue(adm.isAtivo);
            Assert.AreEqual(dataCriacao, adm.DataCriacao);

            // Assert.IsLessThan(1, Math.Abs((adm.DataCriacao - dataCriacao).TotalSeconds));
            // Caso o adm.DataCriacao seja diferente do dataCriacao, pois o Datetime.Now pode ter uma pequena diferença de milissegundos, o teste ainda passará, desde que a diferença seja menor que 1 segundo.
        }
    }
}
