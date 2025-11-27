using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PT.Base
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class RequireWwwAttribute : Attribute, IAuthorizationFilter, IOrderedFilter
    {

        private bool? permanent;
        public bool Permanent
        {
            get => permanent ?? true;
            set => permanent = value;
        }

        private bool? ignoreLocalhost;
        public bool IgnoreLocalhost
        {
            get => ignoreLocalhost ?? true;
            set => ignoreLocalhost = value;
        }

        public int Order { get; set; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            //var redirectLinkSetting = ((IOptions<List<RedirectLinkSetting>>)AppHttpContext.Current.RequestServices.GetService(typeof(IOptions<List<RedirectLinkSetting>>))).Value;

            //var req = context.HttpContext.Request;
            //var host = req.Host;
            //var checkRedirectLink = redirectLinkSetting.FirstOrDefault(x => x.From == req.Path);
            //if (checkRedirectLink != null)
            //{
            //    context.Result = new RedirectResult($"{checkRedirectLink.To}{req.QueryString}", true);
            //    return;
            //}

            //var isLocalHost = string.Equals(host.Host, "localhost", StringComparison.OrdinalIgnoreCase);
            //if (IgnoreLocalhost && isLocalHost)
            //{
            //    return;
            //}

            //var baseSettings = (IOptions<BaseSettings>)AppHttpContext.Current.RequestServices.GetService(typeof(IOptions<BaseSettings>));
            //bool isChangePath = false;
            //string newScheme = "https";
            //string newDomain = host.Value;
            //string path = req.Path;

            //if (host.Host.StartsWith("www", StringComparison.OrdinalIgnoreCase))
            //{
            //    newDomain = host.Value.Replace("www", "");
            //    isChangePath = true;
            //}
            //if (req.Scheme == "http")
            //{
            //    isChangePath = true;
            //}
            return;
            //if (!string.Equals(host.Host, baseSettings.Value.RootDomin, StringComparison.OrdinalIgnoreCase) && baseSettings.Value.MultiDomain)
            //{
            //    newDomain = newDomain.Replace(newDomain, baseSettings.Value.RootDomin);
            //    isChangePath = true;
            //}

            //if (isChangePath)
            //{
            //    string newPath = $"{newScheme}://{newDomain}{req.PathBase}{path}{req.QueryString}";
            //    context.Result = new RedirectResult(newPath, true);
            //    return;
            //}
            //else
            //{
            //    return;
            //}
        }
    }

    public class UrlRequestCultureProvider : RequestCultureProvider
    {
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }
            var pathSegments = httpContext.Request.Path.Value.Split('/');
            var defaultCulture = CultureHelper.GetDefaultCulture.Id;
            if (pathSegments.Length >= 2)
            {
                string checkSegment = pathSegments[1].ToLower();
                if (string.IsNullOrEmpty(checkSegment))
                {
                    return Task.FromResult(new ProviderCultureResult(defaultCulture));
                }
                else if (ListData.ListLanguage.Any(x => x.Id == checkSegment))
                {
                    return Task.FromResult(new ProviderCultureResult(checkSegment));
                }
                else if (checkSegment == "content" || checkSegment == "data" || checkSegment == "lib" || checkSegment == "module")
                {
                    return Task.FromResult(new ProviderCultureResult(Thread.CurrentThread.CurrentCulture.Name));
                }
                else if (checkSegment == "admin")
                {
                    return Task.FromResult(new ProviderCultureResult("vi"));
                }

            }
            return Task.FromResult(new ProviderCultureResult(defaultCulture));
        }
    }

    public class CustomRouter : IRouter
    {
        private readonly IRouter _defaultRouter;
        public CustomRouter(IRouter defaultRouteHandler)
        {
            _defaultRouter = defaultRouteHandler;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return _defaultRouter.GetVirtualPath(context);
        }

        public async Task RouteAsync(RouteContext context)
        {

            string path = context.HttpContext.Request.Path.Value;
            string language = CultureHelper.GetDefaultCulture.Id;

            if (path.ToLower().StartsWith("/admin") || path.ToLower().StartsWith("/data") || path.ToLower().StartsWith("/content") || path.ToLower().StartsWith("/lib") || path.ToLower().StartsWith("/module"))
            {
                await _defaultRouter.RouteAsync(context);
            }
            else if (path.ToLower().StartsWith("/imageresize"))
            {
                context.RouteData.Values["controller"] = "Home";
                context.RouteData.Values["action"] = "ImageResize";
                await _defaultRouter.RouteAsync(context);
            }
            else if (path.ToLower().StartsWith("/websitehub"))
            {
                await _defaultRouter.RouteAsync(context);
            }
            else
            {
                var baseSettings = (IOptions<BaseSettings>)AppHttpContext.Current.RequestServices.GetService(typeof(IOptions<BaseSettings>));
                var _iLinkRepository = (ILinkRepository)AppHttpContext.Current.RequestServices.GetService(typeof(ILinkRepository));
                string slug = path.EndsWith(".html") ? path.Substring(0, path.Length - 5) : path;
                if (slug.Length == 3)
                {
                    language = baseSettings.Value.MultipleLanguage ? slug.Substring(1, 2) : CultureHelper.GetDefaultCulture.Id;
                    slug = "";
                }
                else if (slug.Length > 3)
                {
                    language = baseSettings.Value.MultipleLanguage ? slug.Substring(1, 2) : CultureHelper.GetDefaultCulture.Id;
                    slug = baseSettings.Value.MultipleLanguage ? slug.Replace($"/{language}/", "") : slug.Substring(1);
                }
                else if (slug == "/")
                {
                    slug = "";
                }

                var kt = await _iLinkRepository.FindObject(slug, language);
                if (kt != null)
                {
                    context.RouteData.Values["controller"] = kt.Controller;
                    context.RouteData.Values["action"] = kt.Acction;
                    context.RouteData.Values["language"] = kt.Link?.Language;
                    context.RouteData.Values["id"] = kt.Link?.ObjectId;
                    context.RouteData.Values["linkData"] = Newtonsoft.Json.JsonConvert.SerializeObject(kt.Link);
                }
                else if (path.ToLower().StartsWith("/data/"))
                {
                   
                }
                else if (path.ToLower().StartsWith("/home/changelanguage"))
                {
                 
                }
                else
                {
                    if(!ListData.ListLanguage.Any(x=>x.Id==language))
                    {
                        language = CultureHelper.GetDefaultCulture.Id;
                    }
                    var dataLink = await _iLinkRepository.FindObject404(language);
                    context.RouteData.Values["controller"] = "Home";
                    context.RouteData.Values["action"] = "Page404";
                    context.RouteData.Values["language"] = dataLink.Language;
                    context.RouteData.Values["id"] = dataLink.ObjectId;
                    context.RouteData.Values["linkData"] = Newtonsoft.Json.JsonConvert.SerializeObject(dataLink);
                }
            }
       
            await _defaultRouter.RouteAsync(context);
        }
    }
}
