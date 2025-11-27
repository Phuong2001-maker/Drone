using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using PT.Base;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;
using PT.UI.Models;

namespace PT.UI.Controllers
{
    public class AccountHomeController : Controller
    {
        private readonly IEmployeeRepository _iEmployeeRepository;
        public AccountHomeController(IEmployeeRepository iEmployeeRepository)
        {
            _iEmployeeRepository = iEmployeeRepository;
        }
 
        public async Task<IActionResult> Register(string language, string linkData)
        {
            CultureHelper.AppendLanguage(language);
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            var model = new AccountHomeModel
            {
                Language = language
            };
            return View(model);
        }
        public async Task<IActionResult> Signin(string language, string linkData)
        {
            CultureHelper.AppendLanguage(language);
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
            var model = new AccountHomeModel
            {
                Language = language
            };
            return View(model);
        }
    }
}
