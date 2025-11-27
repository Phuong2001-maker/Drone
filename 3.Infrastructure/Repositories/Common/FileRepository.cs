using Microsoft.AspNetCore.Http;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using PT.Infrastructure.Interfaces;
using PT.Domain.Model;
using System.Net.Http.Headers;
using System.Linq;

namespace PT.Infrastructure.Repositories
{

    public class FileRepository : IFileRepository
    {
        //public virtual async Task ResizeImageAsync(IFormFile file, string pathFile, ImageFormat imgType, int width, int height = 0)
        //{

        //    int newWidth;
        //    int newHeight;
        //    Image image = Image.FromStream(file.OpenReadStream());
        //    int BaseWidth = image.Width;
        //    int BaseHeight = image.Height;
        //    if (BaseWidth > width && width > 0)
        //    {
        //        var typeSave = imgType;
        //        string FileType = Path.GetExtension(pathFile).ToLower();

        //        double dblCoef = (double)width / (double)BaseWidth;
        //        if (height > 0)
        //        {
        //            newWidth = width;
        //            newHeight = height;
        //        }
        //        else
        //        {
        //            newWidth = Convert.ToInt32(dblCoef * BaseWidth);
        //            newHeight = Convert.ToInt32(dblCoef * BaseHeight);
        //        }

        //        Image ReducedImage;
        //        Image.GetThumbnailImageAbort callb = new Image.GetThumbnailImageAbort(ThumbnailCallback);
        //        ReducedImage = image.GetThumbnailImage(newWidth, newHeight, callb, IntPtr.Zero);
        //        ReducedImage.Save(pathFile, typeSave);
        //    }
        //    else
        //    {
        //        using (FileStream fs = File.Create(pathFile))
        //        {
        //            await file.CopyToAsync(fs);
        //            fs.Flush();
        //        }
        //    }
        //}
        public bool ThumbnailCallback()
        {
            return false;
        }
        public void SettingsUpdate(string map, object a)
        {
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(a, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(map, output);
        }
        public bool DeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ResponseModel> UploadFile(IFormFileCollection files, BaseSettings _baseSettings, string webPath, string folder)
        {
            try
            {
                string[] allowedExtensions = _baseSettings.ImagesType.Split(',');
                string path = $"{webPath}{folder}";
                string pathServer = folder;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                foreach (var file in files)
                {
                    if (!allowedExtensions.Contains(Path.GetExtension(file.FileName)))
                    {
                        return new ResponseModel() { Output = 2, Message = "Hình ảnh tải lên không đúng định dạng.", Type = ResponseTypeMessage.Warning, Data = "" };
                    }
                    else if (_baseSettings.ImagesMaxSize < file.Length)
                    {
                        return new ResponseModel() { Output = 3, Message = "Hình ảnh tải lên vượt quá kích thước cho phép.", Type = ResponseTypeMessage.Warning, Data = "" };
                    }
                    else
                    {
                        var newFilename = Path.GetFileName(file.FileName);
                        if (System.IO.File.Exists(path + file.Name))
                        {
                            newFilename = DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + Path.GetFileName(file.FileName);
                        }

                        string pathFile = ContentDispositionHeaderValue
                        .Parse(file.ContentDisposition)
                        .FileName
                        .Trim('"');

                        pathFile = $"{path}{newFilename}";
                        pathServer = $"{pathServer}{newFilename}";
                        using (var stream = new FileStream(pathFile, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                }
                return new ResponseModel() { Output = 1, Message = "Tải ảnh lên thành công.", Type = ResponseTypeMessage.Success, Data = pathServer, IsClosePopup = false };
            }
            catch (Exception)
            {
                return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.", Type = ResponseTypeMessage.Danger, Status = false };
            }
        }
    }
}