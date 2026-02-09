using MinimalAPI.Enums;

namespace MinimalAPI.Domain.DTOs;

public record TokenResponseDTO
(
    string Token,
    string TokenType,
    DateTime ExpiresAt,
    TokenUsuarioDTO Usuario 
);

public record TokenUsuarioDTO
(
    int Id,
    string Name,
    string Email,
    Cargo Cargo
);
