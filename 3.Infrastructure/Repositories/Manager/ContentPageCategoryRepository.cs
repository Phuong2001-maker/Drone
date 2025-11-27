using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace PT.Infrastructure.Repositories
{
    public class ContentPageCategoryRepository : EntityBaseRepository<ContentPageCategory>, IContentPageCategoryRepository
    {
        public ContentPageCategoryRepository(ApplicationContext context) : base(context)
        {
        }
    }
}

