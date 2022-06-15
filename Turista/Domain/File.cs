
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using Turista.Data;
using Turista.Resources;

namespace Domain
{
    public static class File
    {
        //private static readonly;
        public static string Get(ApplicationDbContext _context, Guid id)
        {
            var file = _context.Files.Find(id);
            if (file != null)
            {
                return "data:" + file.Type + ";base64, " + file.FileContent;// Convert.ToBase64String();

            }
            return null;
        }
        public static string GetImage(ApplicationDbContext _context, Guid? id)
        {
            var path = Path.Combine("images", "noimg.jpg");
            if (id==null)
                return path;
            if (id==Guid.Empty)
                return path;
            var file = _context.Files.Find(id);
            return file != null ? file.FileContent : path;
        }

        public static string GetThumpImage(ApplicationDbContext _context, Guid? id)
        {
            var path = Path.Combine("images", "noimg.jpg");
            if (id == null)
                return path; 
            if (id == Guid.Empty)
                return path;
            var file = _context.Files.Find(id);
            return file != null ? file.FileContentMin : path;
        }
        public static string GetExtension(string name)
        {
            return Path.GetExtension(name).Replace(".", "");
        }
        public static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            string[] SizeSuffixes = { lang.Byte, lang.KiloByte, lang.MegaByte, lang.GigaByte, "TB", "PB", "EB", "ZB", "YB" };

            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
        public static Guid Upload(ApplicationDbContext _context, IFormFile formfile, bool secure = false)
        {
            try
            {
                if (formfile.Length > 0)
                {
                    var file = new Turista.Data.General.File();
                    file.Name = formfile.FileName;
                    using (var ms = new MemoryStream())
                    {
                        formfile.CopyTo(ms);
                        file.FileContent = Convert.ToBase64String(ms.ToArray());
                    }
                    file.Size = formfile.Length;
                    file.Type = formfile.ContentType;
                    var result = _context.Files.Add(file);
                    return file.Id;
                }
            }
            catch (Exception ex)
            {
                return new Guid();
            }

            return new Guid();
        }
        public static List<Guid> Upload(ApplicationDbContext _context, IEnumerable<IFormFile> formfiles, bool secure = false)
        {
            List<Guid> guids = new List<Guid>();

            foreach (FormFile ff in formfiles)
            {
                try
                {
                    var file = new Turista.Data.General.File();
                    file.Name = ff.FileName;
                    using (var ms = new MemoryStream())
                    {
                        ff.CopyTo(ms);
                        file.FileContent = Convert.ToBase64String(ms.ToArray());
                    }
                    file.Size = ff.Length;
                    file.Type = ff.ContentType;
                    var result = _context.Files.Add(file);
                    guids.Add(file.Id);
                }
                catch
                {

                }

            }
            return guids;
        }
        public static List<Guid> Upload(string directory,ApplicationDbContext _context, IEnumerable<IFormFile> formfiles)
        {
            List<Guid> guids = new List<Guid>();

            foreach (FormFile ff in formfiles)
            {
                try
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(ff.FileName);
                    var file = new Turista.Data.General.File();
                    file.Name = fileName;
                    var path = Path.Combine(
                           Directory.GetCurrentDirectory(), "wwwroot", "_uploads", directory,
                           fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        ff.CopyTo(stream);
                    }
                    file.FileContent = Path.Combine("_uploads", directory, fileName);
                    file.Size = ff.Length;
                    file.Type = ff.ContentType;
                    

                    //Resize Images
                    if (directory== "ResortsImages" || directory== "PropertiesImages")
                    {
                        
                        var filePath = Path.Combine(
                           Directory.GetCurrentDirectory(), "wwwroot", "_uploads", directory, "Thumb",
                           fileName);
                        using (var image = SixLabors.ImageSharp.Image.Load(ff.OpenReadStream()))
                        {
                            var newSize = ImageResize(image, 400, 260);
                            image.Mutate(c => c.Resize(newSize.Width, newSize.Height));
                            image.Save(filePath);
                        }
                        file.FileContentMin= Path.Combine("_uploads", directory, "Thumb",
                          fileName);
                    }
                    var result = _context.Files.Add(file);
                    guids.Add(file.Id);
                }
                catch(Exception ex)
                {

                }

            }
            return guids;
        }

        public static Guid CopyImage(ApplicationDbContext _context, Turista.Data.General.File file)
        {
            
            var fileId = Guid.NewGuid();
            var newFile = file;
            try
            {
                var fileContentPath = Path.Combine(
                           Directory.GetCurrentDirectory(), "wwwroot", newFile.FileContent);
                var fileName = Path.GetFileName(fileContentPath);
                var fileExt = Path.GetExtension(fileContentPath);

                

                var desFileContentPath = Path.Combine(
                           Directory.GetCurrentDirectory(), "wwwroot","_uploads", "PropertiesImages", fileId + fileExt);
                System.IO.File.Copy(fileContentPath, desFileContentPath);

                newFile.FileContent = Path.Combine("_uploads", "PropertiesImages",
                          fileId + fileExt);

                //For Temp File
                var fileContentMinPath = Path.Combine(
                           Directory.GetCurrentDirectory(), "wwwroot", newFile.FileContentMin);

                if (System.IO.File.Exists(fileContentMinPath))
                {
                    var fileNameMin = Path.GetFileName(fileContentMinPath);
                    var fileNameExMin = Path.GetExtension(fileContentMinPath);

                    var desFileContentMinPath = Path.Combine(
                           Directory.GetCurrentDirectory(), "wwwroot", "_uploads", "PropertiesImages","Thumb", fileId + fileNameExMin);

                    newFile.FileContentMin = Path.Combine("_uploads", "PropertiesImages", "Thumb",
                          fileId + fileNameExMin);
                    System.IO.File.Copy(fileContentMinPath, desFileContentMinPath);
                }

                newFile.Id = fileId;
                newFile.CreatedDate = DateTime.Now;
                _context.Files.Add(newFile);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
            return fileId;
        }

        public class ImageRatio
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }
        public static ImageRatio ImageResize(SixLabors.ImageSharp.Image image,int maxWidth,int maxHeight)
        {
            ImageRatio imageRatio = new()
            {
                Width = image.Width,
                Height = image.Height
            };
            try
            {
                if (image.Width>maxWidth||image.Height>maxHeight)
                {
                    double widthRatio = (double)image.Width / (double)maxWidth;
                    double heightRatio = (double)image.Height / (double)maxHeight;
                    double ratio = Math.Max(widthRatio, heightRatio);
                    imageRatio.Width = (int)(image.Width / ratio);
                    imageRatio.Height = (int)(image.Height / ratio);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return imageRatio;
        }



        public static string Upload(IFormFile formfile, string directory, string name)
        {
            if (formfile.Length > 0)
            {

                var path = Path.Combine(
                           Directory.GetCurrentDirectory(), "wwwroot", "_uploads",
                           formfile.FileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    formfile.CopyToAsync(stream);
                }

                return path;
            }

            return null;
        }
        public static bool Remove(string path)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                return true;
            }

            return false;
        }
        public static bool Remove(ApplicationDbContext _context, Guid id)
        {
            var file = _context.Files.Find(id);
            var filePath = Path.Combine(
                           Directory.GetCurrentDirectory(), "wwwroot", file.FileContent);
            
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch
                {
                }
            }

            if (!string.IsNullOrEmpty(file.FileContentMin))
            {
                var filePathMin = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot", file.FileContentMin);
                if (System.IO.File.Exists(filePathMin))
                {
                    try
                    {
                        System.IO.File.Delete(filePathMin);
                    }
                    catch
                    {
                    }
                }
            }
            _context.Files.Remove(file);
            return true;
        }
        public static string ConvertImageURLToBase64(String url)
        {
            StringBuilder _sb = new StringBuilder();
            Byte[] _byte = GetImage(url);
            _sb.Append(Convert.ToBase64String(_byte, 0, _byte.Length));
            return _sb.ToString();
        }

        private static byte[] GetImage(string url)
        {
            Stream stream = null;
            byte[] buf;
            try
            {
                WebProxy myProxy = new WebProxy();
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                stream = response.GetResponseStream();
                using (BinaryReader br = new BinaryReader(stream))
                {
                    int len = (int)(response.ContentLength);
                    buf = br.ReadBytes(len);
                    br.Close();
                }
                stream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                buf = null;
            }
            return (buf);
        }


    }
}
