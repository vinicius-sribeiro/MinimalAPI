using Microsoft.AspNetCore.Http;
using MinimalAPI.Enums;
using System.Security.Claims;

namespace MinimalAPI.Extensions;

/* ======== Extension Method ========
 * Os métodos de extensão servem para criar novos métodos para um tipo de dado (Refêrencia ou Valor).
 * Elas precisam ser **static** tanto a classe quantos os métodos.
 * 
 * Exemplo:
 * 
 * int x = 10;
 * bool isParNoExtension = NormalFunctions.EhPar(x); -> uma função normal.
 * bool isPar = x.EhPar(); -> Extension Method.
 * 
 * Em uma função Extension Method, nós usamos o parâmetro com **this**.
 * É como se indicasse que vamos usar um objeto que chamar essa função e que seja daquele tipo de dado.
 * É como se falassemos: "Vou passar como parâmetro objetos que chamavam essa função e forem desse tipo."
 * 
 * Exemplo de estrutura:
 * 
 * public static class IntExtensions {
 *      
 *      public static bool EhPar(this int numero) {
 *          return numero % 2 == 0;
 *      }
 * }
 * 
 * bool isPar = x.EhPar();
 * 
 * O compilador entende assim: 
 * 
 * bool isPar = IntExtensions.EhPar(x);
 */


// Um Extension Method para **ClaimsPrincipal**
// Funciona para conseguirmos pegar as informações do claim do Token JWT.
public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        // Pegamos o ID do usuário na claim do Token.
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) 
                    ?? user.FindFirst("id") 
                    ?? user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId)) return null;

        return userId;
    }

    public static string? GetUserName(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value;        
    }

    public static string? GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value;
    }

    public static Cargo? GetCargo(this ClaimsPrincipal user)
    {
        var userCargo = user.FindFirst(ClaimTypes.Role)?.Value;
        return Enum.TryParse(userCargo, out Cargo cargo) ? cargo : null;
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole(Cargo.Admin.ToString());
    }
}
