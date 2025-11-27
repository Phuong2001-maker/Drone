using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System.Linq;
using PT.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using PT.Base;

namespace PT.UI.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class StaticInformationManagerController : Base.Controllers.BaseController
    {
        private readonly IHostingEnvironment _iHostingEnvironment;
        private readonly ILogger _logger;
        private readonly LogSettings _logSettings;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly ILogRepository _iLogRepository;
        private readonly IStaticInformationRepository _iStaticInformationRepository;

        public StaticInformationManagerController(
            ILogger<StaticInformationManagerController> logger,
            IOptions<BaseSettings> baseSettings,
            IHostingEnvironment iHostingEnvironment,
            ILogRepository iLogRepository,
            IOptions<LogSettings> logSettings,
            IOptions<EmailSettings> emailSettings,
            IStaticInformationRepository iStaticInformationRepository
        )
        {
            controllerName = "StaticInformationManager";
            tableName = "StaticInformation";
            _logger = logger;
            _baseSettings = baseSettings;
            _iHostingEnvironment = iHostingEnvironment;
            _iLogRepository = iLogRepository;
            _logSettings = logSettings.Value;
            _iStaticInformationRepository = iStaticInformationRepository;
        }

        #region [Index]
        [AuthorizePermission]
        public IActionResult Index(string language = "vi")
        {
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(int? id,int? page, int? limit, string key, string language = "vi", string ordertype = "asc", string orderby = "name")
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iStaticInformationRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                    m => (m.Name.Contains(key) || key == null) &&
                        (m.Language == language) &&
                        !m.Delete && (m.Id== id || id==null),
                OrderByExtention(ordertype, orderby));
            data.ReturnUrl = Url.Action("Index",
                new
                {
                    page,
                    limit,
                    key,
                    ordertype,
                    orderby
                });
            return View("IndexAjax", data);
        }
        private Func<IQueryable<StaticInformation>, IOrderedQueryable<StaticInformation>> OrderByExtention(string ordertype, string orderby)
        {
            Func<IQueryable<StaticInformation>, IOrderedQueryable<StaticInformation>> functionOrder = null;
            switch (orderby)
            {
                case "name":
                    functionOrder = ordertype == "asc" ? EntityExtention<StaticInformation>.OrderBy(m => m.OrderBy(x => x.Name)) : EntityExtention<StaticInformation>.OrderBy(m => m.OrderByDescending(x => x.Name));
                    break;
                default:
                    functionOrder = ordertype == "asc" ? EntityExtention<StaticInformation>.OrderBy(m => m.OrderBy(x => x.Id)) : EntityExtention<StaticInformation>.OrderBy(m => m.OrderByDescending(x => x.Id));
                    break;
            }
            return functionOrder;
        }
        #endregion

        #region [Create]
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult Create(string language = "vi", int parrentId = 0)
        {
            var dl = new StaticInformationModel
            {
                Language = language
            };
            ViewData["language"] = _baseSettings.Value.MultipleLanguage ? $"/{language}" : "";
            return View(dl);
        }
        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(StaticInformationModel use)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var data = new StaticInformation
                    {
                        Name = use.Name,
                        Content = use.Content,
                        Delete = false,
                        Status = use.Status,
                        Language = use.Language,
                    };
                    await _iStaticInformationRepository.AddAsync(data);
                    await _iStaticInformationRepository.CommitAsync();
                    CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, data.Content, ModuleType.StaticInformation, data.Id,data.Language);
                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới thông tin tĩnh \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới thông tin tĩnh thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [Edit]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> Edit(int id)
        {
            var dl = await _iStaticInformationRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null || (dl != null && dl.Delete))
            {
                return View("404");
            }
            var model = MapModel<StaticInformationModel>.Go(dl);
            return View(model);
        }
        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(StaticInformationModel use, int id, string categoryIds, string tags)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iStaticInformationRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null || (dl != null && dl.Delete))
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }

                    dl.Name = use.Name;
                    dl.Content = use.Content;
                    dl.Status = use.Status;
                    dl.Language = use.Language;

                    _iStaticInformationRepository.Update(dl);
                    await _iStaticInformationRepository.CommitAsync();

                    CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, dl.Content, ModuleType.StaticInformation, dl.Id, dl.Language);

                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật thông tin tĩnh \"{dl.Name}\".",
                        Type = LogType.Edit
                    });
                    return new ResponseModel() { Output = 1, Message = "Cập nhật thông tin tĩnh thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = -2, Message = "Bạn chưa nhập đầy đủ thông tin.", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [Delete]
        [HttpPost, ActionName("Delete")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeletePost(int id)
        {
            try
            {
                var kt = await _iStaticInformationRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null || (kt != null && kt.Delete))
                {
                    return new ResponseModel() { Output = 0, Message = "thông tin tĩnh không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                kt.Delete = true;
                await _iStaticInformationRepository.CommitAsync();

                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa thông tin tĩnh \"{kt.Name}\".",
                    Type = LogType.Delete
                });

                return new ResponseModel() { Output = 1, Message = "Xóa thông tin tĩnh thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion
    }
}