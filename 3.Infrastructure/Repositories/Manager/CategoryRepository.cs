using Microsoft.EntityFrameworkCore;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PT.Infrastructure.Repositories
{
    public class CategoryRepository : EntityBaseRepository<Category>, ICategoryRepository
    {
        private readonly ApplicationContext _context;
        public CategoryRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }
        
        public override async Task<BaseSearchModel<List<Category>>> SearchPagedListAsync(int page, int limit, Expression<Func<Category, bool>> predicate = null, Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null, Expression<Func<Category, Category>> select = null, params Expression<Func<Category, object>>[] includeProperties)
        {
            IQueryable<Category> query = _context.Categorys.AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }

            if (orderBy != null)
            {
                query = orderBy(query).AsQueryable();
            }
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }
            query = query
                .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.CategoryBlog || x.Type == CategoryType.CategoryService).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Delete = x.data.Delete,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    Summary = x.data.Summary,
                    Banner2 = x.data.Banner2,
                    IsHome = x.data.IsHome
                }).AsQueryable();
            var list = await query.Skip((page - 1) * limit).Take(limit).AsNoTracking().ToListAsync();
            return new BaseSearchModel<List<Category>>
            {
                Data = list,
                Limit = limit,
                Page = page,
                TotalRows = await query.CountAsync()
            };
        }


        public async Task<List<Category>> SearchAsync(
           bool asNoTracking = false,
           int skip = 0,
           int Take = 0,
           Expression<Func<Category, bool>> predicate = null,
           Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null,
           Expression<Func<Category, Category>> select = null,
           bool anyContent = false)
        {
            IQueryable<Category> query = _context.Categorys.AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }

            if (orderBy != null)
            {
                query = orderBy(query).AsQueryable();
            }
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }

            if (anyContent)
            {
                query = query.Where(x => _context.ContentPages.Any(m => m.ServiceId == x.Id && m.Type==CategoryType.FAQ));
            }

            query = query
                .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.CategoryBlog || x.Type == CategoryType.CategoryService).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Delete = x.data.Delete,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    Order = x.data.Order,
                    ParentId = x.data.ParentId,
                    Type = x.data.Type,
                    Summary = x.data.Summary,
                    Banner2 = x.data.Banner2,
                    IsHome = x.data.IsHome
                }).AsQueryable();
            if (Take > 0)
            {
                query = query.Skip(skip < 0 ? 0 : skip).Take(Take).AsQueryable();
            }
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
            return await query.ToListAsync();
        }

        public override async Task<List<Category>> SearchAsync(
            bool asNoTracking = false,
            int skip = 0, 
            int Take = 0, 
            Expression<Func<Category, bool>> predicate = null, 
            Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null,
            Expression<Func<Category, Category>> select = null, 
            params Expression<Func<Category, object>>[] includeProperties)
        {
            IQueryable<Category> query = _context.Categorys.AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }

            if (orderBy != null)
            {
                query = orderBy(query).AsQueryable();
            }
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }
            query = query
                .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.CategoryBlog || x.Type == CategoryType.CategoryService).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Delete = x.data.Delete,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    Order =x.data.Order,
                    ParentId =x.data.ParentId,
                    Type = x.data.Type,
                    Summary = x.data.Summary,
                    Banner2= x.data.Banner2,
                    IsHome =x.data.IsHome
                }).AsQueryable();
            if (Take > 0)
            {
                query = query.Skip(skip < 0 ? 0 : skip).Take(Take).AsQueryable();
            }
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
            return await query.ToListAsync();
        }
        public  async Task<List<Category>> FindByLinkReference(int skip = 0, int Take = 0, Expression<Func<Category, bool>> predicate = null, Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null, Expression<Func<Category, Category>> select = null)
        {
            IQueryable<Category> query = _context.Categorys.AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }

            if (orderBy != null)
            {
                query = orderBy(query).AsQueryable();
            }
          
            query = query
                .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.CategoryBlog || x.Type == CategoryType.CategoryService).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Delete = x.data.Delete,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    Order = x.data.Order,
                    ParentId = x.data.ParentId,
                    Type = x.data.Type,
                    Summary = x.data.Summary,
                    Banner2 = x.data.Banner2,
                    IsHome = x.data.IsHome
                }).AsQueryable();
            if (Take > 0)
            {
                query = query.Skip(skip < 0 ? 0 : skip).Take(Take).AsQueryable();
            }
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
            var list = await query.ToListAsync(); 
            var listLink = list.Select(x => x.Link?.Id);

            // List Phiên Bản
            var listReferences = await _context.LinkReferences
                .Where(x => listLink.Contains(x.LinkId1))
                .GroupJoin(_context.Links, x => x.LinkId2, y => y.Id, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new { x.data, link = y })
                .Select(x => new LinkReference
                {
                    Language = x.data.Language,
                    Id = x.data.Id,
                    LinkId2 = x.data.LinkId2,
                    LinkId1 = x.data.LinkId1,
                    Link2 = x.link
                }).ToListAsync();
            foreach (var item in list)
            {
                item.LinkReferences = listReferences.Where(x => x.LinkId1 == item.Link?.Id).ToList();
            }
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
            return list;
        }
        public int MaxOrder(Expression<Func<Category, bool>> predicate = null)
        {
            var querry = _context.Categorys.AsQueryable();
            if(querry!=null)
            {
                querry = querry.Where(predicate).AsQueryable();
            }
            if(querry.Any())
            {
                return querry.Max(x => x.Order);
            }
            else
            {
                return 0;
            }
        }

        public async override Task<Category> SingleOrDefaultAsync(bool asNoTracking = false, Expression<Func<Category, bool>> predicate = null, params Expression<Func<Category, object>>[] includeProperties)
        {
            IQueryable<Category> query = _context.Categorys.AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }

            query = query
                .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.CategoryBlog || x.Type == CategoryType.CategoryService).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Delete = x.data.Delete,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    Order = x.data.Order,
                    ParentId = x.data.ParentId,
                    Type = x.data.Type,
                    Summary = x.data.Summary,
                    Banner2 = x.data.Banner2,
                    IsHome = x.data.IsHome
                }).AsQueryable();
           
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
            return await query.FirstOrDefaultAsync();
        }
        public async Task<List<ContentPageCategory>> CurrentTreeContent(int contentPageId)
        {
            var newList = new List<ContentPageCategory>();

            var query = await _context.ContentPageCategorys.Where(x => x.ContentPageId == contentPageId)
                .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.CategoryBlog || x.Type == CategoryType.CategoryService).AsQueryable(), x => x.CategoryId, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new { x.data, link = y })
                .GroupJoin(_context.Categorys.AsQueryable(), x => x.data.CategoryId, y => y.Id, (x, y) => new { x.data, x.link, categorys = y })
                .SelectMany(x => x.categorys.DefaultIfEmpty(), (x, y) => new ContentPageCategory {
                    CategoryId = x.data.CategoryId,
                    Link = x.link,
                    Category =  y,
                    ContentPageId = x.data.ContentPageId,
                    Id = x.data.Id
                }).AsNoTracking().ToListAsync();

            query = query.Where(x => x.Category != null && x.Link != null).ToList();
            var dl = query.FirstOrDefault(x => x.Category.ParentId == 0);
            if (dl != null)
            {
                newList.Add(dl);
            }
            int currentParentId = dl?.CategoryId??0;
            // Danh mục cha 0
            foreach (var item in query.Where(x=>x.Category.ParentId!=0))
            {
                if(item.Category.ParentId==currentParentId)
                {
                    newList.Add(item);
                    currentParentId = item.CategoryId;
                }
            }
            return newList;
        }
        public async Task<List<Category>> CurrentTreeChildrent(int parentId, string language, CategoryType type)
        {
            var newList = new List<Category>();

            var query = await _context.Categorys.Where(x=>x.Language== language && x.Type==type)
                .GroupJoin(_context.Links.Where(x => x.Type == type).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Delete = x.data.Delete,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    ParentId = x.data.ParentId,
                    Type = x.data.Type,
                    Summary = x.data.Summary,
                    Banner2 = x.data.Banner2,
                    IsHome = x.data.IsHome
                }).AsNoTracking().ToListAsync();

            query = query.Where(x =>  x.Link != null).ToList();
            var dl = query.FirstOrDefault(x => x.Id == parentId);
            int currentOrder = 99;
            if (dl != null)
            {
                dl.Order = currentOrder;
                currentOrder--;
                newList.Add(dl);
            }
            int currentParentId = dl?.ParentId ?? 0;
            if(currentParentId > 0)
            {
                foreach (var item in query.Where(x => x.Id != dl?.Id))
                {
                    if (item.ParentId == currentParentId)
                    {
                        item.Order = currentOrder;
                        currentOrder--;
                        newList.Add(item);
                        currentParentId = item.Id;
                    }
                }
            }
            return newList.OrderBy(x=>x.Order).ToList();
        }
    }
}