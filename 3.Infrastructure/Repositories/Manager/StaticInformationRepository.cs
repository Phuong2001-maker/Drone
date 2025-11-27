using PT.Domain.Model;
using PT.Infrastructure.Interfaces;

namespace PT.Infrastructure.Repositories
{
    public class StaticInformationRepository : EntityBaseRepository<StaticInformation>, IStaticInformationRepository
    {
        public StaticInformationRepository(ApplicationContext context) : base(context)
        {
        }
    }
}

