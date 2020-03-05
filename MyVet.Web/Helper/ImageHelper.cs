using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MyVet.Web.Helper
{
    public class ImageHelper : IImageHelper
    {

        public async Task<string> UploadImageAsync(IFormFile model)
        {
            var guid = Guid.NewGuid().ToString();
            var file = $"{guid}.jpg";

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot\\images\\Pets",
                 file);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await model.CopyToAsync(stream);//upload  
            }
            return $"~/images/Pets/{file}";
        }
    }
}
