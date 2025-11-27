using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System.Linq;
using PT.Shared;
using System.Text;
using PT.Base;

namespace PT.UI.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class AdvertisingBannerManagerController : Base.Controllers.BaseController
    {
        private readonly IHostingEnvironment _iHostingEnvironment;
        private readonly ILogger _logger;
        private readonly IBannerRepository _iBannerRepository;
        private readonly IBannerItemRepository _iBannerItemRepository;

        public AdvertisingBannerManagerController(
            ILogger<AdvertisingBannerManagerController> logger,
            IHostingEnvironment iHostingEnvironment,
            IBannerRepository iBannerRepository,
            IBannerItemRepository iBannerItemRepository
        )
        {
            controllerName = "AdvertisingBannerManager";
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
        [AuthorizePermission]
        public async Task<IActionResult> IndexPost(int? id, int? page, int? limit, string key, string language = "vi", string ordertype = "asc", string orderby = "name")
        {
            page = page < 0 ? 1 : page;
            limit = (limit > 100 || limit < 10) ? 10 : limit;
            var data = await _iBannerRepository.SearchPagedListAsync(
                page ?? 1,
                limit ?? 10,
                    m =>(m.Name.Contains(key) || key == null) &&
                        (m.Language == language) && 
                        m.Type==BannerType.Advertising &&
                        !m.Delete && (m.Id ==id || id == null),
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
                        Template = use.Template?? "[Content]",
                        Type = BannerType.Advertising,
                        ClassActive = use.ClassActive
                    };
                    await _iBannerRepository.AddAsync(data);
                    await _iBannerRepository.CommitAsync();

                    CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupBanner(data), ModuleType.AdvertisingBanner, data.Id,data.Language);

                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm mới Banner \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới Banner thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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

                    CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupBanner(dl), ModuleType.AdvertisingBanner, dl.Id,dl.Language);

                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật Banner \"{dl.Name}\".",
                        Type = LogType.Edit
                    });
                    return new ResponseModel() { Output = 1, Message = "Cập nhật Banner thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = false };
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
                    return new ResponseModel() { Output = 0, Message = "Banner không tồn tại, vui lòng thử lại.", Type = ResponseTypeMessage.Warning };
                }
                kt.Delete = true;
                await _iBannerRepository.CommitAsync();
                CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupBanner(await _iBannerRepository.SingleOrDefaultAsync(true, x => x.Id == id)), ModuleType.AdvertisingBanner, id,kt.Language);

                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa Banner \"{kt.Name}\".",
                    Type = LogType.Delete
                });

                return new ResponseModel() { Output = 1, Message = "Xóa Banner thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = true };
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
                var dataParent = await _iBannerRepository.SingleOrDefaultAsync(true, x => x.Id == kt.BannerId);
                CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupBanner(dataParent), ModuleType.AdvertisingBanner, kt.BannerId, dataParent?.Language);
                await _iBannerItemRepository.CommitAsync();

                await AddLog(new LogModel
                {
                    ObjectId = kt.Id,
                    ActionTime = DateTime.Now,
                    Name = $"Xóa Banner item \"{kt.Name}\".",
                    Type = LogType.Delete
                });

                return new ResponseModel() { Output = 1, Message = "Xóa Banner item thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = false };
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
                foreach (var item in list.OrderBy(x=>x.Order))
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
        public IActionResult CreateItem(int BannerId = 0)
        {
            var dl = new BannerItemModel
            {
                BannerId = BannerId
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
                        Target = use.Target
                    
                    };
                    await _iBannerItemRepository.AddAsync(data);
                    await _iBannerItemRepository.CommitAsync();

                    var dataParent = await _iBannerRepository.SingleOrDefaultAsync(true, x => x.Id == data.BannerId);

                    CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupBanner(dataParent), ModuleType.AdvertisingBanner, data.BannerId, dataParent?.Language);

                    await AddLog(new LogModel
                    {
                        ObjectId = data.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Thêm Banner item \"{data.Name}\".",
                        Type = LogType.Create
                    });

                    return new ResponseModel() { Output = 1, Message = "Thêm mới Banner item thành công ", Type = ResponseTypeMessage.Success, IsClosePopup = false };
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
                    _iBannerItemRepository.Update(dl);
                    await _iBannerItemRepository.CommitAsync();
                    var dataParent = await _iBannerRepository.SingleOrDefaultAsync(true, x => x.Id == dl.BannerId);
                    CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupBanner(dataParent), ModuleType.AdvertisingBanner, dl.BannerId, dataParent?.Language);

                    await AddLog(new LogModel
                    {
                        ObjectId = dl.Id,
                        ActionTime = DateTime.Now,
                        Name = $"Cập nhật Banner item \"{dl.Name}\".",
                        Type = LogType.Edit
                    });
                    return new ResponseModel() { Output = 1, Message = "Cập nhật Banner thành công.", Type = ResponseTypeMessage.Success, IsClosePopup = false };
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
                    Name = $"Cập nhật thứ tự banner quảng cáo #\"{id}\".",
                    Type = LogType.Edit
                });
                var dataParent = await _iBannerRepository.SingleOrDefaultAsync(true, x => x.Id == id);
                CommonFunctions.GenModule(_iHostingEnvironment.WebRootPath, await UpdateGroupBanner(dataParent), ModuleType.AdvertisingBanner, id, dataParent?.Language);
                return new ResponseModel() { Output = 1, Message = "Cập nhật thành công.", Type = ResponseTypeMessage.Success };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        private readonly string tokenUrl = "[Url]";
        private readonly string tokenTarget = "[Target]";
        private readonly string tokenName = "[Name]";
        private readonly string tokenImg = "[Img]";
        private readonly string tokenContent = "[Content]";
        private async Task<string> UpdateGroupBanner(Banner dlGroup)
        {
            var list = await _iBannerItemRepository.SearchAsync(true, 0, 0, x => x.Status && x.BannerId== dlGroup.Id);
            int i = 0;
            var Str = new StringBuilder();
            foreach (var item in list.OrderBy(x=>x.Order))
            {
                string _TokenFor = item.Template;
                if (!string.IsNullOrEmpty(_TokenFor))
                {
                    Str.Append(_TokenFor
                     .Replace(tokenUrl, item.Href)
                     .Replace(tokenTarget, item.Target)
                     .Replace(tokenImg, item.Banner)
                     .Replace(tokenName, item.Name)
                     );
                }
                i++;
            }
            string OutPut = dlGroup.Template;
            OutPut = OutPut.Replace(tokenContent, Str.ToString());
            return OutPut;
        }
    }
}