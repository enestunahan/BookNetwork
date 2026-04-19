using BookNetwork.Application.Common.Security;
using MediatR;

namespace BookNetwork.Application.Features.Auth.Commands.RefreshTokenLogin;

public sealed record RefreshTokenLoginCommand(string RefreshToken) : IRequest<TokenDto>;
