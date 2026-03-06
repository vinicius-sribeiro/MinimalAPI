using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MinimalAPI_Test.Domain.FakeServices
{
    internal class TokenServiceFake : ITokenService
    {
        public string GenerateToken(Usuario usuario)
        {
            return "fake-token";
        }

        public DateTime ObterDataExpiracao()
        {
            return DateTime.UtcNow.AddHours(1);
        }
    }
}
