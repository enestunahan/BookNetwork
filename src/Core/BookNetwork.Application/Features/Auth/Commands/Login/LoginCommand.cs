using BookNetwork.Application.Common.Security;
using MediatR;

namespace BookNetwork.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string UserNameOrEmail, string Password) : IRequest<TokenDto>;
