using PT.Domain.Model;
using PT.Infrastructure;
using System.Threading.Tasks;
using PT.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace PT.Infrastructure.Repositories
{
    public class ContactLogRepository : EntityBaseRepository<ContactLog>, IContactLogRepository
    {
        public ContactLogRepository(ApplicationContext context) : base(context)
        {
        }
    }
}

