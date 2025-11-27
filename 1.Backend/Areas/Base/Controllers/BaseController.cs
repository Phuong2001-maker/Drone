using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PT.Infrastructure.Interfaces;
using PT.Domain.Model;
using PT.Base;
using PT.Infrastructure.Repositories;
using PT.Shared;

namespace PT.UI.Areas.Base.Controllers
{
    public class BaseController : Controller
    {
        
        protected  string controllerName = "";
        protected  string tableName = "";

        public async  Task AddLog(LogModel input)
        {
            var iLogRepository = (ILogRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILogRepository));
            var baseSettings = (IOptions<LogSettings>)AppHttpContext.Current.RequestServices.GetService(typeof(IOptions<LogSettings>));
            if (baseSettings.Value.Is)
            {
                var data = new Domain.Model.Log
                {
                    ObjectId = input.ObjectId,
                    Name = input.Name,
                    AcctionUser = Newtonsoft.Json.JsonConvert.SerializeObject(new { DataUserInfo.UserId, DataUserInfo.DisplayName , DataUserInfo.UserName , DataUserInfo.Email }),
                    Object = tableName,
                    ObjectType = controllerName,
                    Type = input.Type,
                    ActionTime = DateTime.Now
                };
                if (baseSettings.Value.IsUseMongo)
                {
                    // Code mongo
                }
                else
                {
                    await iLogRepository.AddAsync(data);
                    await iLogRepository.CommitAsync();
                }
            }
        }

        public async Task CheckValidateSlug(string slug, CategoryType type, int id, string language)
        {
            var _iLinkRepository = (ILinkRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkRepository));
            if (id > 0)
            {
                var ktLug = await _iLinkRepository.AnyAsync(x => x.Slug == slug && x.ObjectId != id && x.Type== type && x.Language== language);
                if(ktLug)
                {
                    ModelState.AddModelError("Slug", "{0} đã tồn tại, vui lòng thay đổi hoặc thêm một số ký tự khác bao gồm (a-z), (0-9), (-,/)");
                }
            }
            else
            {
                var ktLug = await _iLinkRepository.AnyAsync(x => x.Slug == slug && x.Language == language);
                if (ktLug)
                {
                    ModelState.AddModelError("Slug", "{0} đã tồn tại, vui lòng thay đổi hoặc thêm một số ký tự khác bao gồm (a-z), (0-9), (-,/)");
                }
            }
        }

        public async Task CheckValidateSlug2(string slug, int id, string language)
        {
            var _iLinkRepository = (ILinkRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkRepository));
            var ktLug = await _iLinkRepository.AnyAsync(x => x.Slug == slug && x.Id != id  && x.Language == language);
            if (ktLug)
            {
                ModelState.AddModelError("Slug", "{0} đã tồn tại, vui lòng thay đổi hoặc thêm một số ký tự khác bao gồm (a-z), (0-9), (-,/)");
            }
        }

        public async Task<object> AddTagLink(string name,string language)
        {
            var _iLinkRepository = (ILinkRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkRepository));
            var _iTagRepository = (ITagRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ITagRepository));

            if (name == null || language == null)
            {
                return null;
            }
            var use = new TagModel
            {
                Type = CategoryType.Tag,
                Slug = Functions.ToUrlSlug(name),
                Language = language,
                Name = name
            };
            var check = await _iTagRepository.SingleOrDefaultAsync(true, x => x.Name.ToLower() == use.Name.ToLower() && x.Language == use.Language);
            if (check != null)
            {
                return new { id = check.Id, name = check.Name };
            }

            var ktLug = await _iLinkRepository.AnyAsync(x => x.Slug == use.Slug && x.Language == language);
            if (ktLug)
            {
                for (int i = 1; i <= 30; i++)
                {
                    use.Slug = $"{use.Slug}-{i}";
                    var ktLug2 = await _iLinkRepository.AnyAsync(x => x.Slug == use.Slug && x.Language == language);
                    if (!ktLug2)
                    {
                        break;
                    }
                    if (i == 30)
                    {
                        return null;
                    }
                }
            }

            var data = new Tag
            {
                Name = use.Name,
                Delete = false,
                Status = true,
                Language = use.Language
            };
            await _iTagRepository.AddAsync(data);
            await _iTagRepository.CommitAsync();
            var seoModel = MapModel<SeoModel>.Go(use);
            await AddSeoLink(CategoryType.Tag, data.Language, data.Id, seoModel, name);

        

            return new { id = data.Id, name = data.Name };
        }

        public async Task<int> AddSeoLink(CategoryType type, string language, int id, SeoModel model,string name)
        {
            var _iLinkRepository = (ILinkRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkRepository));
            var _iLinkReferenceRepository = (ILinkReferenceRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkReferenceRepository));
            var baseSettings = (IOptions<BaseSettings>)AppHttpContext.Current.RequestServices.GetService(typeof(IOptions<BaseSettings>));

            var dlLug = new Link
            {
                Slug = model.Slug,
                Name = name,
                Type = type,
                ObjectId = id,
                Language = language,
                IsStatic = false,
                Changefreq = model.Changefreq,
                Lastmod = model.Lastmod??DateTime.Now,
                Priority = model.Priority.ConvertToDouble(),
                Delete = model.Delete,
                Description= model.Description,
                FacebookBanner = model.FacebookBanner,
                FacebookDescription = model.FacebookDescription,
                FocusKeywords=model.FocusKeywords,
                GooglePlusDescription = model.GooglePlusDescription,
                IncludeSitemap = model.IncludeSitemap,
                Keywords = model.Keywords,
                MetaRobotsAdvance = model.MetaRobotsAdvance,
                MetaRobotsFollow = model.MetaRobotsFollow,
                MetaRobotsIndex = model.MetaRobotsIndex,
                Redirect301 = model.Redirect301,
                Title= model.Title,
                Status = model.Status
            };
            await _iLinkRepository.AddAsync(dlLug);
            await _iLinkRepository.CommitAsync();

            // Update ánh xạ
            if(baseSettings.Value.MultipleLanguage)
            {
                model.Language = language;
                model.Type = dlLug.Type;
                model.LinkId = dlLug.Id;
                await _iLinkReferenceRepository.ReferenceUpdate(model);
            }
            return dlLug.Id;
        }
        public async Task UpdateSeoLink(bool changeSlug, CategoryType type, int id, string language, SeoModel model, string name)
        {
            var _iLinkRepository = (ILinkRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkRepository));
            var _iLinkReferenceRepository = (ILinkReferenceRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkReferenceRepository));
            var baseSettings = (IOptions<BaseSettings>)AppHttpContext.Current.RequestServices.GetService(typeof(IOptions<BaseSettings>));

            var ktLink = await _iLinkRepository.SingleOrDefaultAsync(false, x => x.ObjectId == id && x.Type == type);
            if (ktLink == null)
            {
                ktLink = new Link
                {
                    Slug = model.Slug,
                    Type = type,
                    Name = name,
                    ObjectId = id,
                    Language = language,
                    IsStatic = false,
                    Changefreq = model.Changefreq,
                    Lastmod = model.Lastmod ?? DateTime.Now,
                    Priority = model.Priority.ConvertToDouble(),
                    Delete = model.Delete,
                    Description = model.Description,
                    FacebookBanner = model.FacebookBanner,
                    FacebookDescription = model.FacebookDescription,
                    FocusKeywords = model.FocusKeywords,
                    GooglePlusDescription = model.GooglePlusDescription,
                    IncludeSitemap = model.IncludeSitemap,
                    Keywords = model.Keywords,
                    MetaRobotsAdvance = model.MetaRobotsAdvance,
                    MetaRobotsFollow = model.MetaRobotsFollow,
                    MetaRobotsIndex = model.MetaRobotsIndex,
                    Redirect301 = model.Redirect301,
                    Title = model.Title,
                    Status = model.Status
                };
                await _iLinkRepository.AddAsync(ktLink);
                await _iLinkRepository.CommitAsync();
            }
            else if (changeSlug)
            {
                ktLink.Slug = model.Slug;
            }
            ktLink.Changefreq = model.Changefreq;
            ktLink.Changefreq = model.Changefreq;
            ktLink.Lastmod = model.Lastmod ?? DateTime.Now;
            ktLink.Priority = model.Priority.ConvertToDouble();
            ktLink.Description = model.Description;
            ktLink.FacebookBanner = model.FacebookBanner;
            ktLink.FacebookDescription = model.FacebookDescription;
            ktLink.FocusKeywords = model.FocusKeywords;
            ktLink.GooglePlusDescription = model.GooglePlusDescription;
            ktLink.IncludeSitemap = model.IncludeSitemap;
            ktLink.Keywords = model.Keywords;
            ktLink.MetaRobotsAdvance = model.MetaRobotsAdvance;
            ktLink.MetaRobotsFollow = model.MetaRobotsFollow;
            ktLink.MetaRobotsIndex = model.MetaRobotsIndex;
            ktLink.Redirect301 = model.Redirect301;
            ktLink.Title = model.Title;
            ktLink.Status = model.Status;
            ktLink.Name = name;
            await _iLinkRepository.CommitAsync();
            if (baseSettings.Value.MultipleLanguage)
            {
                model.Language = language;
                // Update ánh xạ
                await _iLinkReferenceRepository.ReferenceUpdate(model);
            }
        }
        public async Task DeleteSeoLink(CategoryType type, int id)
        {
            var _iLinkRepository = (ILinkRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkRepository));
            var ktLink = await _iLinkRepository.SingleOrDefaultAsync(false, x => x.ObjectId == id && x.Type == type);
            if (ktLink != null)
            {
                ktLink.Delete = true;
                await _iLinkRepository.CommitAsync();
            }
        }
    }
}