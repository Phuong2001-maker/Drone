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
using System.Threading;
using System.Text;
using PT.Base;

namespace PT.UI.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class PhotoSlideManagerController : Base.Controllers.BaseController
    {
        private readonly IHostingEnvironment _iHostingEnvironment;
        private readonly ILogger _logger;
        private readonly IBannerRepository _iBannerRepository;
        private readonly IBannerItemRepository _iBannerItemRepository;

        public PhotoSlideManagerController(
            ILogger<PhotoSlideManagerController> logger,
            IHostingEnvironment iHostingEnvironment,
            IBannerRepository iBannerRepository,
            IBannerItemRepository iBannerItemRepository
        )
        {
            controllerName = "PhotoSlideManager";
            tableName = "Banner";
            _logger = logger;
            _iHostingEnvironment = iHostingEnvironment;
            _iBannerRepository = iBannerRepository;
            _iBannerItemRepository = iBannerItemRepository;
        }

        #region [Index]
        [AuthorizePermission]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost, ActionName("Index")]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> IndexPost(int? id, int? page, int? limit, string key, string language = "vi", string ordertype = "asc", string orderby = "name")
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iBannerRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                    m => (m.Name.Contains(key) || key == null) &&
                        (m.Language == language) &&
                        m.Type == BannerType.Slide &&
                        !m.Delete
                        && (m.Id==id || id==null),
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
        private Func<IQueryable<Banner>, IOrderedQueryable<Banner>> OrderByExtention(string ordertype, string orderby)
        {
            Func<IQueryable<Banner>, IOrderedQueryable<Banner>> functionOrder = null;
            switch (orderby)
            {
                case "name":
                    functionOrder = ordertype == "asc" ? EntityExtention<Banner>.OrderBy(m => m.OrderBy(x => x.Name)) : EntityExtention<Banner>.OrderBy(m => m.OrderByDescending(x => x.Name));
                    break;
                default:
                    functionOrder = ordertype == "asc" ? EntityExtention<Banner>.OrderBy(m => m.OrderBy(x => x.Id)) : EntityExtention<Banner>.OrderBy(m => m.OrderByDescending(x => x.Id));
                    break;
            }
            return functionOrder;
        }
        #endregion

        #region [Create]
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult Create(string language = "vi")
        {
            var dl = new BannerModel
            {
                Language = language
            };
            return View(dl);
        }
        [HttpPost, ActionName("Create")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreatePost(BannerModel use)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var data = new Banner
                    {
                        Name = use.Name,
                        Delete = false,
                        Status = use.Status,
                        Language = use.Language,
                        Template = use.Template,
                        Type = BannerType.Slide,
                        ClassActive = use.ClassActive
                        
                    };
                    await _iBannerRepository.AddAsync(data);
                    await _iBannerRepository.CommitAsync();

                    CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupBanner(data), ModuleType.PhotoSlide, data.Id,data.Language);

                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới slide ảnh \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới slide ảnh thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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
            var dl = await _iBannerRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null || (dl != null && dl.Delete))
            {
                return View("404");
            }
            var model = MapModel<BannerModel>.Go(dl);
            return View(model);
        }
        [HttpPost, ActionName("Edit")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditPost(BannerModel use, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iBannerRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null || (dl != null && dl.Delete))
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }
                    dl.Name = use.Name;
                    dl.Status = use.Status;
                    dl.Language = use.Language;
                    dl.Template = use.Template;
                    dl.ClassActive = use.ClassActive;

                    _iBannerRepository.Update(dl);
                    await _iBannerRepository.CommitAsync();

                    CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupBanner(dl), ModuleType.PhotoSlide, dl.Id,dl.Language);

                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật slide ảnh \"{dl.Name}\".",
                        Type = LogType.Edit
                    });
                    return new ResponseModel() { Output = 1, Message = "Cập nhật slide ảnh thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = false };
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
                var kt = await _iBannerRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null || (kt != null && kt.Delete))
                {
                    return new ResponseModel() { Output = 0, Message = "Slide ảnh không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                kt.Delete = true;

                await UpdateGroupBanner(kt);

                await _iBannerRepository.CommitAsync();

                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa slide ảnh \"{kt.Name}\".",
                    Type = LogType.Delete
                });

                return new ResponseModel() { Output = 1, Message = "Xóa slide ảnh thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [DeleteItem]
        [HttpPost, ActionName("DeleteItem")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> DeleteItemPost(int id)
        {
            try
            {
                var kt = await _iBannerItemRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                if (kt == null)
                {
                    return new ResponseModel() { Output = 0, Message = "Banner không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                _iBannerItemRepository.Delete(kt);
                var dataParrent = await _iBannerRepository.SingleOrDefaultAsync(true, x => x.Id == kt.BannerId);
                CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupBanner(dataParrent), ModuleType.PhotoSlide, kt.BannerId, dataParrent?.Language);
                await _iBannerItemRepository.CommitAsync();

                await UpdateGroupBanner(await _iBannerRepository.SingleOrDefaultAsync(true,x=>x.Id==kt.BannerId));

                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa ảnh slide \"{kt.Name}\".",
                    Type = LogType.Delete
                });

                return new ResponseModel() { Output = 1, Message = "Xóa ảnh thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [ABC]
        private List<DataSortModel> ConverData(List<DataSortModel> list, int parentId = 0)
        {
            int order = 1;
            var newList = new List<DataSortModel>();
            foreach (var item in list)
            {
                item.ParentId = parentId;
                item.Order = order;
                newList.Add(item);
                if (item.Children.Count() > 0)
                {
                    newList.AddRange(ConverData(item.Children, item.Id));
                }
                order++;
            }
            return newList;
        }

        private string ShowTree(List<BannerItem> list, int parrentId)
        {
            if (list.Count() == 0)
            {
                return "<span>Hiện tại chưa có dữ liệu</span>";
            }
            var listCulture = Shared.ListData.ListLanguage;
            StringBuilder str = new System.Text.StringBuilder();
            if (list.Count() > 0)
            {
                if (parrentId == 0)
                {
                    str.Append($"<ol class=\"dd-list\">");
                }
                else
                {
                    str.Append($"<ol class=\"dd-list\">");
                }
                foreach (var item in list.OrderBy(x => x.Order))
                {
                    if (item.Id == parrentId)
                    {
                        return "";
                    }
                    str.Append($"<li class=\"dd-item {(item.Status ? "treeTrue" : "")} dd3-item\" data-id=\"{item.Id}\">");
                    str.Append($"<div class=\"dd-handle dd3-handle\"></div>");
                    str.Append($"<div class=\"dd3-content\">");
                    str.Append($"{item.Name}");
                    str.Append($"<span class=\"button-icon\"><a button-static href='{Url.Action("EditItem", new { id = item.Id })}'  title=\"Cập nhật\"><i class=\"material-icons iconcontrol text-primary\">edit</i></a><span>");
                    str.Append("</div>");
                    str.Append(ShowTree(list, item.Id));
                    str.Append("</li>");
                }
                str.Append($"</ol>");
            }
            return str.ToString();
        }

        [HttpGet]
        public async Task<string> Items(int id)
        {
            var list = await _iBannerItemRepository.SearchAsync(true, 0, 0, x => x.BannerId == id);
            return ShowTree(list, 0);
        }
        #endregion

        #region [CreateItem]
        [HttpGet]
        [AuthorizePermission("Index")]
        public IActionResult CreateItem(int bannerId = 0)
        {
            var dl = new BannerItemModel
            {
                BannerId = bannerId
            };
            return View(dl);
        }
        [HttpPost, ActionName("CreateItem")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> CreateItemPost(BannerItemModel use)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var maxOrder = _iBannerItemRepository.MaxOrder(x => x.BannerId == use.BannerId);
                    var data = new BannerItem
                    {
                        Name = use.Name,
                        Status = use.Status,
                        Order = maxOrder + 1,
                        Href = use.Href,
                        BannerId = use.BannerId,
                        Banner = use.Banner,
                        Template = use.Template,
                        Target = use.Target,
                        Content = use.Content

                    };
                    await _iBannerItemRepository.AddAsync(data);
                    await _iBannerItemRepository.CommitAsync();

                    var dataParrent = await _iBannerRepository.SingleOrDefaultAsync(true, x => x.Id == data.BannerId);

                    CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupBanner(dataParrent), ModuleType.PhotoSlide, data.BannerId, dataParrent?.Language);

                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm ảnh slide \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới ảnh thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = false };
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

        #region [EditItem]
        [HttpGet]
        [AuthorizePermission("Index")]
        public async Task<IActionResult> EditItem(int id)
        {
            var dl = await _iBannerItemRepository.SingleOrDefaultAsync(true, m => m.Id == id);
            if (dl == null)
            {
                return View("404");
            }
            var model = MapModel<BannerItemModel>.Go(dl);
            return View(model);
        }
        [HttpPost, ActionName("EditItem")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> EditItemPost(BannerItemModel use, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _iBannerItemRepository.SingleOrDefaultAsync(false, m => m.Id == id);
                    if (dl == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Dữ liệu không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                    }
                    dl.Name = use.Name;
                    dl.Status = use.Status;
                    dl.Href = use.Href;
                    dl.Template = use.Template;
                    dl.Target = use.Target;
                    dl.Banner = use.Banner;
                    dl.Content = use.Content;
                    _iBannerItemRepository.Update(dl);
                    await _iBannerItemRepository.CommitAsync();

                    var dataParrent = await _iBannerRepository.SingleOrDefaultAsync(true, x => x.Id == dl.BannerId);
                    CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupBanner(dataParrent), ModuleType.PhotoSlide, dl.BannerId, dataParrent?.Language);

                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật ảnh slide \"{dl.Name}\".",
                        Type = LogType.Edit
                    });
                    return new ResponseModel() { Output = 1, Message = "Cập nhật ảnh thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = false };
                }
                return new ResponseModel() { Output = -2, Message = "Bạn chưa nhập đầy đủ thông tin.", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
        }

        [HttpPost, ActionName("UpdateOrder")]
        [AuthorizePermission("Index")]
        public async Task<ResponseModel> UpdateOrderPost([FromBody]string data, int id)
        {
            try
            {
                var listItem = ConverData(Newtonsoft.Json.JsonConvert.DeserializeObject<List<DataSortModel>>(data));
                var listItemIds = listItem.Select(x => x.Id).ToList();
                var listCa = await _iBannerItemRepository.SearchAsync(false, 0, 0, x => listItemIds.Contains(x.Id));
                foreach (var item in listCa)
                {
                    var objIn = listItem.FirstOrDefault(x => x.Id == item.Id);
                    item.Order = objIn.Order;
                    _iBannerItemRepository.Update(item);
                }
                await _iBannerItemRepository.CommitAsync();
                await AddLog(new LogModel
                {
                    ObjectId = id,
                    ActionTime = DateTime.Now,
                    Name = $"Cập nhật thứ tự trình diễn ảnh #\"{id}\".",
                    Type = LogType.Edit
                });
                var dataParrent = await _iBannerRepository.SingleOrDefaultAsync(true, x => x.Id == id);
                CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupBanner(dataParrent), ModuleType.PhotoSlide, id,dataParrent?.Language);
                return new ResponseModel() { Output = 1, Message = "Cập nhật thành công.", Type = ResponseTypeMessage.Success };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        private readonly string TokenFor = "For";
        private readonly string TokenForPage = "ForPage";
        private readonly string TokenI = "[I]";
        private readonly string TokenUrl = "[Url]";
        private readonly string TokenTarget = "[Target]";
        private readonly string TokenName = "[Name]";
        private readonly string TokenImg = "[Img]";
        private readonly string TokenNote = "[Note]";
        public string TokenClassActive = "[ClassActive]";

        private async Task<string> UpdateGroupBanner(Banner dlGroup)
        {
            if (dlGroup == null || dlGroup.Delete) return "";
            if (dlGroup.Template == null) return "";
            var list = (await _iBannerItemRepository.SearchAsync(true, 0, 0, m => m.BannerId == dlGroup.Id && m.Status == true)).OrderBy(m => m.Order).ToList();
            return LopUpdateGroupBanner(list,dlGroup);
        }
        private string LopUpdateGroupBanner(List<BannerItem> list, Banner dlGroup)
        {
            string _TokenFor = Functions.TrimToken(dlGroup.Template, TokenFor);
            string _TokenForPage = Functions.TrimToken(dlGroup.Template, TokenForPage);
            int i = 0;
            var StrFor = new StringBuilder();
            var StrForPage = new StringBuilder();
            foreach (var item in list)
            {
                if (!string.IsNullOrEmpty(_TokenFor))
                {
                    if (i == 0)
                    {
                        StrFor.Append(_TokenFor
                         .Replace(TokenUrl, item.Href)
                         .Replace(TokenTarget, item.Target).Replace(TokenNote, item.Content)
                         .Replace(TokenImg, item.Banner)
                         .Replace(TokenName, item.Name)
                         .Replace(TokenClassActive, dlGroup.ClassActive)
                         .Replace(TokenI, i.ToString())
                         );
                    }
                    else
                    {
                        StrFor.Append(_TokenFor
                         .Replace(TokenUrl, item.Href)
                         .Replace(TokenTarget, item.Target).Replace(TokenNote, item.Content)
                         .Replace(TokenImg, item.Banner)
                         .Replace(TokenName, item.Name)
                         .Replace(TokenI, i.ToString())
                         );
                    }
                }
                if (!string.IsNullOrEmpty(_TokenForPage))
                {
                    if (i == 0)
                    {
                        StrForPage.Append(_TokenForPage
                         .Replace(TokenUrl, item.Href)
                         .Replace(TokenTarget, item.Target)
                         .Replace(TokenImg, item.Banner)
                         .Replace(TokenName, item.Name)
                          .Replace(TokenNote, item.Content)
                         .Replace(TokenClassActive, dlGroup.ClassActive).Replace(TokenI, i.ToString())
                         );
                    }
                    else
                    {
                        StrForPage.Append(_TokenForPage
                         .Replace(TokenUrl, item.Href)
                         .Replace(TokenTarget, item.Target)
                         .Replace(TokenImg, item.Banner)
                         .Replace(TokenName, item.Name)
                          .Replace(TokenNote, item.Content)
                         .Replace(TokenI, i.ToString())
                         );
                    }
                }
                i++;
            }
            string OutPut = dlGroup.Template;
            if (!string.IsNullOrEmpty(_TokenFor))
            {
                OutPut = OutPut.Replace(_TokenFor, StrFor.ToString()).Replace("[" + TokenFor + "]", "").Replace("[/" + TokenFor + "]", "");
            }
            if (!string.IsNullOrEmpty(_TokenForPage))
            {
                OutPut = OutPut.Replace(_TokenForPage, StrForPage.ToString()).Replace("[" + TokenForPage + "]", "").Replace("[/" + TokenForPage + "]", "");
            }
            return OutPut;
        }
    }
}