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
    public class OtherHomeController : Controller
    {
        private readonly IEmployeeRepository _iEmployeeRepository;
        public OtherHomeController(IEmployeeRepository iEmployeeRepository)
        {
            _iEmployeeRepository = iEmployeeRepository;
        }
 
        public async Task<IActionResult> Feature(string language, string linkData)
        {
            CultureHelper.AppendLanguage(language);
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
         
            return View();
        }
        public async Task<IActionResult> Price(string language, string linkData)
        {
            CultureHelper.AppendLanguage(language);
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
           
            return View();
        }
        public async Task<IActionResult> Cooperate(string language, string linkData)
        {
            CultureHelper.AppendLanguage(language);
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
           
            return View();
        }
        public async Task<IActionResult> Document(string language, string linkData)
        {
            CultureHelper.AppendLanguage(language);
            ViewData["linkData"] = Newtonsoft.Json.JsonConvert.DeserializeObject<Link>(linkData);
           
            return View();
        }
    }
}
