using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Infrastructure.Repositories;
using PT.Shared;
using PT.UI.Models;
using PT.UI.SignalR;

namespace PT.UI.Controllers
{
    public class ContactHomeController : Controller
    {
        
        private readonly IContactRepository _iContactRepository;
        private readonly ICustomerRepository _iCustomerRepository;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly IEmailSenderRepository _iEmailSenderRepository;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly ICountryRepository _iCountryRepository;
        private readonly IUserRepository _iUserRepository;
        private readonly IOptions<AuthorizeSettings> _authorizeSettings;
        private readonly IContactLogRepository _iContactLogRepository;

        public ContactHomeController(
            IContactRepository iContactRepository, 
            ICustomerRepository iCustomerRepository,
            IContentPageRepository iContentPageRepositor,
            IEmailSenderRepository iEmailSenderRepository,
            IOptions<BaseSettings> baseSettings ,
            IOptions<EmailSettings> emailSettings,
            ICategoryRepository iCategoryRepository,
            ICountryRepository iCountryRepository,
            IUserRepository iUserRepository,
            IOptions<AuthorizeSettings> authorizeSettings,
            IContactLogRepository iContactLogRepository
        )
        {
            _iContactRepository = iContactRepository;
            _iCustomerRepository = iCustomerRepository;
            _iContentPageRepository = iContentPageRepositor;
            _iEmailSenderRepository = iEmailSenderRepository;
            _baseSettings = baseSettings;
            _emailSettings = emailSettings;
            _iCategoryRepository = iCategoryRepository;
            _iCountryRepository = iCountryRepository;
            _iUserRepository = iUserRepository;
            _authorizeSettings = authorizeSettings;
            _iContactLogRepository = iContactLogRepository;
        }

        public async Task<IActionResult> FamousPeople(string language, string linkData)
        {
            CultureHelper.AppendLanguage(language);
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            var data = await _iContactRepository.SearchPagedListAsync(
                1,
                100,
                m => m.Status && (m.Type == Domain.Model.Contact.ContactType.FamousPeople || m.Type == Domain.Model.Contact.ContactType.FamousPeopleVideo) && !m.Delete && m.Language == language,
                x => x.OrderBy(m => m.CreatedDate));
            return View(data);
        }

        public  async Task<IActionResult> Contact(string language, string linkData)
        {
            CultureHelper.AppendLanguage(language);

            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);

            var model = new ContactHomeModel
            {
                Language = language
            };
            var data = await _iCountryRepository.SearchAsync(true, 0, 0);
            model.CountrySelectlist = new SelectList(data, "Id", "Name");
            ViewData["dataCountry"] = data;
            return View(model);
        }

        [HttpPost, ActionName("Contact"), ValidateAntiForgeryToken]
        public async Task<ResponseModel> ContactPost(ContactHomeModel use)
        {
            try
            {
                var output = await _iUserRepository.VeryfyCapcha(_authorizeSettings.Value.CapchaVerifyUrl, _authorizeSettings.Value.CapChaSecret, use.Capcha);
                var capchaOke = output.Success;
                if (!capchaOke)
                {
                    return new ResponseModel() { Output = 69, Message = "System waits too long, please try again", Type = ResponseTypeMessage.Warning };
                }

                if (await IsCheckRequest(ContactLogType.Contact, HttpContext.Connection.RemoteIpAddress.ToString()))
                {
                }
                else
                {
                    return new ResponseModel() { Output = -1, Message = $"Time out {_baseSettings.Value.TimeOutSendRequest} s", Type = ResponseTypeMessage.Warning };
                }

                if (ModelState.IsValid)
                {
                    await _iContactRepository.AddAsync(new Contact
                    {
                        FullName = Functions.SContent(use.FullName),
                        Content = Functions.SContent(use.Content),
                        Delete = false,
                        Status = false,
                        Email = use.Email,
                        Phone = use.Phone,
                        CreatedDate = DateTime.Now,
                        CountryId  = use.CountryId??0,
                        PhoneCode = use.PhoneCode
                    });
                    await _iContactRepository.CommitAsync();

                    if(use.FullName.ToLower().Contains("sex") || use.FullName.ToLower().Contains("girl") || use.FullName.ToLower().Contains("human"))
                    {
                        return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
                    }

                    if ((use.Content ?? "").ToLower().Contains("sex") || (use.Content ?? "").ToLower().Contains("girl") || (use.Content ?? "").ToLower().Contains("human"))
                    {
                        return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
                    }

                    var dlCountry = await _iCountryRepository.SingleOrDefaultAsync(true, x => x.Id == use.CountryId);

                    if(!string.IsNullOrEmpty(_baseSettings.Value.ToEmail))
                    {
                        var strB = new StringBuilder();
                        strB.Append($"Họ và tên: {Functions.SContent(use.FullName)}<br>");
                        strB.Append($"Email: {use.Email}<br>");
                        strB.Append($"Phone: ({dlCountry?.PhoneCode}){use.Phone}<br>");
                        strB.Append($"Quốc gia: {dlCountry?.Name}<br>");
                        strB.Append($"Nội dung: {Functions.SContent(use.Content)}<br>");

                        //await Task.Run(() => SendEmail(_emailSettings.Value, _baseSettings.Value.ToEmail,  $"Có liên hệ mới từ {use.FullName} địa chỉ email là {use.Email}", strB.ToString())).ConfigureAwait(false);
                    }
                    await SetRequest(ContactLogType.Contact, HttpContext.Connection.RemoteIpAddress.ToString());
                    return new ResponseModel() { Output = 1, Message = "Liên hệ thành công", Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
            }
            catch
            {
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }

        private async void SendEmail(EmailSettings emailSettings,string toEmail,string title, string content)
        {
           await _iEmailSenderRepository.SendEmailAsync(emailSettings, null, title, content, toEmail);
        }

        public IActionResult Testimonials(string language, string linkData)
        {
            CultureHelper.AppendLanguage(language);
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            return View(null);
        }

        [HttpGet]
        [Route("{language}/ContactHome/TestimonialsAjax")]
        public async Task<IActionResult> TestimonialsAjax(string language, int? page, Domain.Model.Contact.ContactType type)
        {
            var data = await _iContactRepository.SearchPagedListAsync(
                page ?? 1,
                6,
                m => m.Status && m.Type == type && !m.Delete && m.Language == language,
                x => x.OrderByDescending(m => m.CreatedDate));
            return View("TestimonialsAjax", data);
        }

        [Route("{language}/ContactHome/FormTestimonials")]
        public IActionResult FormTestimonials(string language)
        {
            CultureHelper.AppendLanguage(language);
            return View();
        }

        [HttpPost]
        [Route("{language}/ContactHome/FormTestimonials")]
        public async Task<ResponseModel> FormTestimonialsPost(string language, TestimonialHomeModel use)
        {
            try
            {

                var output = await _iUserRepository.VeryfyCapcha(_authorizeSettings.Value.CapchaVerifyUrl, _authorizeSettings.Value.CapChaSecret, use.Capcha);
                var capchaOke = output.Success;
                if (!capchaOke)
                {
                    return new ResponseModel() { Output = 69, Message = "System waits too long, please try again", Type = ResponseTypeMessage.Warning };
                }

                if (await IsCheckRequest(ContactLogType.FormTestimonials, HttpContext.Connection.RemoteIpAddress.ToString()))
                {
                }
                else
                {
                    return new ResponseModel() { Output = -1, Message = $"Time out {_baseSettings.Value.TimeOutSendRequest} s", Type = ResponseTypeMessage.Warning };
                }

                if (ModelState.IsValid)
                {
                    var dlAdd = new Contact()
                    {
                        Type = Domain.Model.Contact.ContactType.Testimonial,
                        FullName = Functions.SContent(use.FullName),
                        Email = use.Email,
                        Content = Functions.SContent(use.Content),
                        Rating = use.Rating ?? 5,
                        CreatedDate = DateTime.Now,
                        Language = language
                    };
                    await _iContactRepository.AddAsync(dlAdd);
                    await _iContactRepository.CommitAsync();
                }

                await SetRequest(ContactLogType.FormTestimonials, HttpContext.Connection.RemoteIpAddress.ToString());

                return new ResponseModel() { Output = 1, Message = "Liên hệ thành công", Type = ResponseTypeMessage.Success, IsClosePopup = true };
            }
            catch
            {
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }

        [Route("{language}/ContactHome/Booking")]
        public async Task<IActionResult> Booking(int type=0, string language="vi")
        {
            CultureHelper.AppendLanguage(language);

            var model = new HomeContactAppointmentModel
            {
                ServiceSelectList = new SelectList(await _iCategoryRepository.SearchAsync(true, 0, 0, x => !x.Delete && x.Status && x.Type == CategoryType.CategoryService && x.Language == language), "Id", "Name")
            };
            var data = await _iCountryRepository.SearchAsync(true, 0, 0);
            model.CountrySelectlist = new SelectList(data, "Id", "Name");
            ViewData["dataCountry"] = data;
            if (type==2)
            {
                return View("_Booking", model);
            }
       
            return View(model);
        }

        [Route("{language}/ContactHome/Booking"), ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ResponseModel> BookingPost(HomeContactAppointmentModel use)
        {
            try
            {

                //var output = await _iUserRepository.VeryfyCapcha(_authorizeSettings.Value.CapchaVerifyUrl, _authorizeSettings.Value.CapChaSecret, use.Capcha);
                //var capchaOke = output.Success;
                //if (!capchaOke)
                //{
                //    return new ResponseModel() { Output = 69, Message = "System waits too long, please try again", Type = ResponseTypeMessage.Warning };
                //}

                if (await IsCheckRequest(ContactLogType.Booking, HttpContext.Connection.RemoteIpAddress.ToString()))
                {
                }
                else
                {
                    return new ResponseModel() { Output = -1, Message = $"Time out {_baseSettings.Value.TimeOutSendRequest} s", Type = ResponseTypeMessage.Warning };
                }

                var dlAdd = new Contact();
                if (ModelState.IsValid)
                {
                    try
                    {
                        dlAdd = new Contact()
                        {
                            Type = Domain.Model.Contact.ContactType.BookAnAppointment,
                            FullName = Functions.SContent(use.FullName),
                            Email = use.Email,
                            Content = Functions.SContent(use.Content),
                            Age = use.Age,
                            ServiceId = use.ServiceId ?? 0,
                            CreatedDate = DateTime.Now,
                            CountryId = use.CountryId ?? 0,
                            PhoneCode = use.PhoneCode,
                            AppointmentDate = use.AppointmentDate==null ? DateTime.Now: Convert.ToDateTime($"{use.AppointmentDate?.ToString("yyyy/MM/dd")} {use.AppointmentTime??"00:00"}")
                        };
                        await _iContactRepository.AddAsync(dlAdd);
                        await _iContactRepository.CommitAsync();
                    }
                    catch
                    {
                    }

                    if (use.FullName.ToLower().Contains("sex") || use.FullName.ToLower().Contains("girl") || use.FullName.ToLower().Contains("human"))
                    {
                        return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
                    }

                    if ((use.Content??"").ToLower().Contains("sex") || (use.Content ?? "").ToLower().Contains("girl") || (use.Content ?? "").ToLower().Contains("human"))
                    {
                        return new ResponseModel() { Output = 0, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
                    }

                    var dlCountry = await _iCountryRepository.SingleOrDefaultAsync(true, x => x.Id == use.CountryId);

                    if (!string.IsNullOrEmpty(_baseSettings.Value.ToEmail))
                    {
                        var dlService = await _iCategoryRepository.SingleOrDefaultAsync(true,x => x.Id == use.ServiceId);
                        var strB = new StringBuilder();
                        strB.Append($"Họ và tên: {use.FullName}<br>");
                        strB.Append($"Email: {use.Email}<br>");
                        strB.Append($"Phone: ({dlCountry?.PhoneCode}){use.Phone}<br>");
                        strB.Append($"Hẹn ngày khám: ({dlAdd.AppointmentDate?.ToString("dd/MM/yyyy")})<br>");
                        strB.Append($"Quốc gia: {dlCountry?.Name}<br>");
                        strB.Append($"Dịch vụ: {dlService?.Name}<br>");
                        strB.Append($"Nội dung: {use.Content}<br>");
                        //await Task.Run(() => SendEmail(_emailSettings.Value, _baseSettings.Value.ToEmail, $"Có lịch hẹn mới từ {use.FullName} địa chỉ email là {use.Email}", strB.ToString())).ConfigureAwait(false);
                    }

                    await SetRequest(ContactLogType.Booking, HttpContext.Connection.RemoteIpAddress.ToString());
                }
                return new ResponseModel() { Output = 1, Message = "Liên hệ thành công", Type = ResponseTypeMessage.Success, IsClosePopup = true };
            }
            catch
            {
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }

        private async Task SetRequest(ContactLogType type, string ip)
        {
            var kt = await _iContactLogRepository.SingleOrDefaultAsync(false, x => x.IP == ip && x.Type == type);
            if (kt == null)
            {
                await _iContactLogRepository.AddAsync(new ContactLog
                {
                    IP = ip,
                    Count = 1,
                    IsBanlist = false,
                    LastConnection = DateTime.Now,
                    Type = type
                });
                await _iContactLogRepository.CommitAsync();
            }
            else
            {
                kt.LastConnection = DateTime.Now;
                kt.Count += 1;
            }
            await _iContactLogRepository.CommitAsync();
        }

        private async Task<bool> IsCheckRequest(ContactLogType type, string ip)
        {
            var kt = await _iContactLogRepository.SingleOrDefaultAsync(true, x => x.IP == ip && x.Type == type);
            if(kt == null)
            {
                await _iContactLogRepository.AddAsync(new ContactLog
                {
                    IP = ip,
                    Count = 0,
                    IsBanlist =false,
                    LastConnection = DateTime.Now,
                    Type = type
                });
                await _iContactLogRepository.CommitAsync();
                return true;
            }  
            else
            {
                if(kt.IsBanlist)
                {
                    return false;
                }

                var nextTime = kt.LastConnection.AddSeconds(_baseSettings.Value.TimeOutSendRequest);

                if (nextTime < DateTime.Now)
                {
                    return true;
                }    
                else
                {
                    return false;
                }    
            }    
        }
    }
}

