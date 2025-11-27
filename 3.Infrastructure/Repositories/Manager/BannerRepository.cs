using PT.Domain.Model;
using PT.Infrastructure.Interfaces;

namespace PT.Infrastructure.Repositories
{
    public class BannerRepository : EntityBaseRepository<Banner>, IBannerRepository
    {
        public BannerRepository(ApplicationContext context) : base(context)
        {
        }
    }
}

