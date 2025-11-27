using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;

namespace PT.UI.Controllers
{
    public class CategoryHomeController : Controller
    {
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ITagRepository _iTagRepository;
        private readonly IContentPageTagRepository _iContentPageTagRepository;
        private readonly ICategoryRepository _iCategoryRepository;


        public CategoryHomeController(IContentPageRepository iContentPageRepository, ITagRepository iTagRepository, IContentPageTagRepository iContentPageTagRepository, ICategoryRepository iCategoryRepository)
        {
            _iContentPageRepository = iContentPageRepository;
            _iTagRepository = iTagRepository;
            _iContentPageTagRepository = iContentPageTagRepository;
            _iCategoryRepository = iCategoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, string language, int? page, string key, string linkData)
        {
            CultureHelper.AppendLanguage(language);

            var objectLink = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            objectLink.Title = string.IsNullOrEmpty(objectLink.Title) ? objectLink.Name : objectLink.Title;
         

            string viewName = "_404";
            var dl = await _iCategoryRepository.SingleOrDefaultAsync(true, x => x.Id == id && x.Status && !x.Delete);
            if (dl == null)
            {
                return View("_Home404");
            }

            switch (dl.Type)
            {
                case CategoryType.CategoryBlog:
                    viewName = "Blogs";
                    dl.PageBlog = await _iContentPageRepository.SearchPagedListAsync(
                      page ?? 1,
                      10,
                      id,
                      null,
                      m => (m.Name.Contains(key) || key == null || m.Content.Contains(key) || m.Summary.Contains(key))
                          && m.Type == CategoryType.Blog
                          && (m.Language == language)
                          && m.Status
                          && !m.Delete, x=> x.OrderByDescending(mbox=>mbox.DatePosted),x=> new ContentPage {
                              Category = x.Category,
                              Id = x.Id,
                              Author = x.Author,
                              Banner = x.Banner,
                              DatePosted = x.DatePosted,
                              Delete = x.Delete,
                              Name = x.Name,
                              Language = x.Language,
                              Status = x.Status,
                              Summary = x.Summary,
                              Tags = x.Tags,
                              Type = x.Type,
                              Link = x.Link
                          });

                   
                    objectLink.Title = $"{ objectLink.Title }{ ((page == null) ? "" : (language == "vi" ? $" - trang {page}" : $" - page {page}"))}";
                    ViewData["linkData"] = objectLink;
                    int totalPage = (dl.PageBlog.TotalRows % dl.PageBlog.Limit > 0) ? (dl.PageBlog.TotalRows / dl.PageBlog.Limit + 1) : (dl.PageBlog.TotalRows / dl.PageBlog.Limit);
                    if (totalPage >= 2)
                    {
                        page = page ?? 1;
                        if (page < totalPage)
                        {
                            ViewData["linkNext"] = $"{Request.Path}?page={page + 1}";
                        }
                        if (page >= totalPage)
                        {
                            ViewData["linkPrev"] = $"{Request.Path}?page={page - 1}";
                        }
                    }

                    break;
                case CategoryType.CategoryService:
                    if (dl.ParentId == 0)
                    {
                        ViewData["linkData"] = objectLink;
                        viewName = "Services";
                        dl.ChildrentCategorys = await _iCategoryRepository.SearchAsync(true, 0, 0, x => x.Status && !x.Delete && x.ParentId == id && x.Type == CategoryType.CategoryService);
                        foreach (var item in dl.ChildrentCategorys)
                        {
                            item.ContentPageCategory = await _iContentPageRepository.SearchAdvanceAsync(CategoryType.Service, 0, 0, item.Id, null, x => x.Status && !x.Delete && x.Type == CategoryType.Service,x=>x.OrderByDescending(m=>m.DatePosted), x => new ContentPage
                            {
                                Category = x.Category,
                                Id = x.Id,
                                Author = x.Author,
                                Banner = x.Banner,
                                DatePosted = x.DatePosted,
                                Delete = x.Delete,
                                Name = x.Name,
                                Language = x.Language,
                                Price = x.Price,
                                Serice = x.Serice,
                                ServiceId = x.ServiceId,
                                Status = x.Status,
                                Summary = x.Summary,
                                Tags = x.Tags,
                                Type = x.Type,
                                Link = x.Link,
                                IsHome = x.IsHome
                            });
                        }

                    }
                    else
                    {
                        viewName = "ChildrentServices";
                        ViewData["linkData"] = objectLink;
                        dl.PageBlog = await _iContentPageRepository.SearchPagedListAsync(
                                  page ?? 1,
                                  99999,
                                  id,
                                  null,
                                  m => (m.Name.Contains(key) || key == null || m.Content.Contains(key) || m.Summary.Contains(key))
                                      && m.Type == CategoryType.Service
                                      && (m.Language == language)
                                      && m.Status
                                      && !m.Delete, x => x.OrderByDescending(m => m.DatePosted), x => new ContentPage
                                      {
                                          Category = x.Category,
                                          Id = x.Id,
                                          Author = x.Author,
                                          Banner = x.Banner,
                                          DatePosted = x.DatePosted,
                                          Delete = x.Delete,
                                          Name = x.Name,
                                          Language = x.Language,
                                          Price = x.Price,
                                          Serice = x.Serice,
                                          ServiceId = x.ServiceId,
                                          Status = x.Status,
                                          Summary = x.Summary,
                                          Tags = x.Tags,
                                          Type = x.Type,
                                          Link = x.Link,
                                          IsHome = x.IsHome
                                      });
                                    }



                    break;
            }
           
            return View(viewName, dl);
        }
    }
}