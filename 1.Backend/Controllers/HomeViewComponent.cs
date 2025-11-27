using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static PT.Domain.Model.Contact;

namespace PT.Component
{
    [ViewComponent(Name = "ViewModule")]
    public class ViewModuleComponent : ViewComponent
    {
        private readonly IHostingEnvironment _iHostingEnvironment;

        public ViewModuleComponent(IHostingEnvironment iHostingEnvironment)
        {
            _iHostingEnvironment = iHostingEnvironment;
        }
        public async Task<HtmlString> InvokeAsync(ModuleType type, int id)
        {
            try
            {
                var file = $"{_iHostingEnvironment.WebRootPath}/Module/{type.ToString()}_{id}.html";
                if (System.IO.File.Exists(file))
                {
                    return new HtmlString(await System.IO.File.ReadAllTextAsync(file));
                }
                else
                {
                    return new HtmlString("");
                }
            }
            catch (Exception)
            {
                return new HtmlString("");
            }
        }
    }
}

namespace PT.Component
{
    [ViewComponent(Name = "ContentPage")]
    public class ContentPageComponent : ViewComponent
    {
        private readonly IContentPageRepository _iContentPageRepository;
        public ContentPageComponent(IContentPageRepository iContentPageRepository)
        {
            _iContentPageRepository = iContentPageRepository;
        }
        public async Task<IViewComponentResult> InvokeAsync(string language, int? contentPageid, int? categoryId, int? tagId, int take, string view, string title, string href, CategoryType type)
        {
            try
            {
                var listNew = new List<ContentPage>();

                listNew = await _iContentPageRepository
                   .SearchAdvanceAsync(type, 0, take, categoryId, tagId, m =>
                       m.Status && !m.Delete
                       && m.DatePosted <= DateTime.Now
                       && m.Language == language
                       && (m.Type == type)
                       && (m.Id != contentPageid || contentPageid == null)
                       , m => m.OrderByDescending(x => x.DatePosted),
                   select: a => new ContentPage
                   {
                       Banner = a.Banner,
                       DatePosted = a.DatePosted,
                       Id = a.Id,
                       Name = a.Name,
                       Summary = a.Summary,
                       Language = a.Language,
                       Author = a.Author,
                       CategoryId = a.CategoryId,
                       Link = a.Link
                   });

                return View(view, new ComponentModel<ContentPage> {
                    Items = listNew,
                    CategoryId = categoryId,
                    Language = language,
                    Take = take,
                    Title = title,
                    Href = href,
                    View = view
                });
            }
            catch
            {
                return View();
            }
        }
    }
}

namespace PT.Component
{
    [ViewComponent(Name = "ContentPageRelated")]
    public class ContentPageRelatedComponent : ViewComponent
    {
        private readonly IContentPageRelatedRepository _iContentPageRelatedRepository;
        public ContentPageRelatedComponent(IContentPageRelatedRepository iContentPageRelatedRepository)
        {
            _iContentPageRelatedRepository = iContentPageRelatedRepository;
        }
        public async Task<IViewComponentResult> InvokeAsync(int contentPageId,string language,  int take, string view, string title)
        {
            try
            {
                var listNew = await _iContentPageRelatedRepository.GetContentPageAsync(contentPageId,0, take, m => m.Status && !m.Delete && m.DatePosted <= DateTime.Now && m.Language == language, m => m.OrderByDescending(x => x.DatePosted),
                    select: a => new ContentPage
                    {
                        Banner = a.Banner,
                        DatePosted = a.DatePosted,
                        Id = a.Id,
                        Name = a.Name,
                        Summary = a.Summary,
                        Language = a.Language,
                        Author = a.Author,
                        Link = a.Link
                    });
                return View(view, new ComponentModel<ContentPage> { Items = listNew,  Language = language, Take = take, Title = title, View = view });
            }
            catch
            {
                return View();
            }
        }
    }
}

namespace PT.Component
{
    [ViewComponent(Name = "Contact")]
    public class ContactComponent : ViewComponent
    {
        private readonly IContactRepository _iContactRepository;
        public ContactComponent(IContactRepository iContactRepository)
        {
            _iContactRepository = iContactRepository;
        }
        public async Task<IViewComponentResult> InvokeAsync(string language, int take, string view, string title, ContactType type)
        {
            try
            {

               if(type==ContactType.Testimonial)
                {
                    var listNew = await _iContactRepository.SearchAsync(true, 0, take, m => !m.Delete && m.Type == type && m.Language == language && m.IsHome, m => m.OrderByDescending(x => x.CreatedDate));
                    return View(view, new ComponentModel<Contact> { Items = listNew, Language = language, Take = take, Title = title, View = view });
                }
               else
               {
                    var listNew = await _iContactRepository.SearchAsync(true, 0, take, m => !m.Delete && m.Type == type && m.Language == language, m => m.OrderByDescending(x => x.CreatedDate));
                    return View(view, new ComponentModel<Contact> { Items = listNew, Language = language, Take = take, Title = title, View = view });
               }
            }
            catch
            {
                return View();
            }
        }
    }
}

namespace PT.Component
{
    [ViewComponent(Name = "Category")]
    public class CategoryComponent : ViewComponent
    {
        private readonly ICategoryRepository _iCategoryRepository;
        public CategoryComponent(ICategoryRepository iCategoryRepository)
        {
            _iCategoryRepository = iCategoryRepository;
        }
        public async Task<IViewComponentResult> InvokeAsync(
            string language, 
            int take, 
            string view,
            string title,
            Expression<Func<Category, bool>> predicate = null,
            Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null, 
            Expression<Func<Category, Category>> select = null
            )
        {
            try
            {
                var listNew = await _iCategoryRepository.SearchAsync(true, 0, take, predicate, orderBy, select);
                return View(view, new ComponentModel<Category> { Items = listNew, Language = language, Take = take, Title = title, View = view });
            }
            catch
            {
                return View();
            }
        }
    }
}

namespace PT.Component
{
    [ViewComponent(Name = "ContentPageAdvance")]
    public class ContentPageAdvanceComponent : ViewComponent
    {
        private readonly IContentPageRepository _iContentPageRepository;
        public ContentPageAdvanceComponent(IContentPageRepository iContentPageRepository)
        {
            _iContentPageRepository = iContentPageRepository;
        }
        public async Task<IViewComponentResult> InvokeAsync(
            int take,
            string view,
            string title,
            int? categoryId, 
            int? tagId,
            CategoryType type,
            Expression<Func<ContentPage, bool>> predicate = null,
            Func<IQueryable<ContentPage>, IOrderedQueryable<ContentPage>> orderBy = null,
            Expression<Func<ContentPage, ContentPage>> select = null
            )
        {
            try
            {
                var listNew = await _iContentPageRepository.SearchAdvanceAsync(type, 0, take , categoryId,  tagId, predicate, orderBy, select);
                return View(view, new ComponentModel<ContentPage> { Items = listNew,  Take = take, Title = title, View = view });
            }
            catch
            {
                return View();
            }
        }
    }
}