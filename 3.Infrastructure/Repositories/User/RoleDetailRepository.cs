using PT.Infrastructure.Interfaces;
using PT.Domain.Model;

namespace PT.Infrastructure.Repositories
{
    public class RoleDetailRepository : EntityBaseRepository<RoleDetail>, IRoleDetailRepository
    {
        public RoleDetailRepository(ApplicationContext context) : base(context)
        {
        }

    }
}
