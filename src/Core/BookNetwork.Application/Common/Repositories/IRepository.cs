using BookNetwork.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace BookNetwork.Application.Common.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    DbSet<T> Table { get; }
}
