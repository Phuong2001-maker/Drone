using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace PT.Infrastructure.Repositories
{
    public class ContentPageReferenceRepository : EntityBaseRepository<ContentPageReference>, IContentPageReferenceRepository
    {
        public ContentPageReferenceRepository(ApplicationContext context) : base(context)
        {
        }
    }
}

