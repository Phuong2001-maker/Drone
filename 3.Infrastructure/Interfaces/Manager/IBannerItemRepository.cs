using System;
using System.Linq.Expressions;
using PT.Domain.Model;

namespace PT.Infrastructure.Interfaces
{
    public interface IBannerItemRepository : IEntityBaseRepository<BannerItem>
    {
        int MaxOrder(Expression<Func<BannerItem, bool>> predicate = null);
    }
}
