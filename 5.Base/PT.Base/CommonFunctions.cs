using Newtonsoft.Json;
using PT.Domain.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PT.Shared;
namespace PT.Base
{
    public class CommonFunctions
    {
        public static LogUserModel GetLogUser(string data)
        {
            try
            {
                return JsonConvert.DeserializeObject<LogUserModel>(data);
            }
            catch
            {
                return null;
            }
        }
        public static string GetStringModule(ModuleType type, int id)
        {
            return $"<vc:view-module id=\"{id}\" type=\"{type.ToString()}\"></vc:view-module>";
        }
        public static void GenModule(string map, string data, ModuleType type, int id, string language)
        {
            string url = "";
            string NewKyTu = "";
            map = $"{map}/Module";
            if (!Directory.Exists(map))
            {
                Directory.CreateDirectory(map);
            }
            if (type == ModuleType.Menu)
            {
                url = $"/Admin/Manager/MenuManager?id={id}&language={language}#openPopup";
            }
            else if (type == ModuleType.PhotoSlide)
            {
                url = $"/Admin/Manager/PhotoSlideManager?id={id}&language={language}#openPopup";
            }
            else if (type == ModuleType.StaticInformation)
            {
                url = $"/Admin/Manager/StaticInformationManager?id={id}&language={language}#openPopup";
            }
            else if (type == ModuleType.AdvertisingBanner)
            {
                url = $"/Admin/Manager/AdvertisingBannerManager?id={id}&language={language}#openPopup";
            }
            map = $"{map}/{type.ToString()}_{id}.html";

            if (!File.Exists(map))
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(map);
                using (System.IO.FileStream fs = fi.Create())
                {

                    Byte[] txt = new System.Text.UTF8Encoding(true).GetBytes("New file.");
                    fs.Write(txt, 0, txt.Length);
                    Byte[] author = new System.Text.UTF8Encoding(true).GetBytes("Author Mahesh Chand");
                    fs.Write(author, 0, author.Length);
                }
            }

            if (url != null && url != "")
            {
                NewKyTu += "<div class='formadmin'>";
                NewKyTu += "<span data-href='" + url + "' class='bindata'></span>";
                NewKyTu += data;
                NewKyTu += "</div>";
                NewKyTu = Functions.ZipStringHTML(NewKyTu);
            }
            else
            {
                NewKyTu = Functions.ZipStringHTML(data);
            }
            File.WriteAllText(map, NewKyTu);
        }
    }
}
