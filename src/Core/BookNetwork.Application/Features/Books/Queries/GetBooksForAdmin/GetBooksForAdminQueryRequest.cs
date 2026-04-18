using MediatR;

namespace BookNetwork.Application.Features.Books.Queries.GetBooksForAdmin;

public sealed record GetBooksForAdminQueryRequest:IRequest<GetBooksForAdminQueryResponse>;