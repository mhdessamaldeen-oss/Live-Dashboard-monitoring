using Application.Common;
using Application.DTOs.Auth;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string Token, string RefreshToken) : IRequest<Result<AuthResponse>>;
