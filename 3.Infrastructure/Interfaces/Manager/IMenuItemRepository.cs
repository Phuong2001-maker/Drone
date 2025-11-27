using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PT.Domain.Model;

namespace PT.Infrastructure.Interfaces
{
    public interface IMenuItemRepository : IEntityBaseRepository<MenuItem>
    {
        int MaxOrder(Expression<Func<MenuItem, bool>> predicate = null);
    }
}
