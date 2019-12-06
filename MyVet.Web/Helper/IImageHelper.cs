using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MyVet.Web.Helper
{
    public interface IImageHelper
    {
        Task<string> UploadImageAsync(IFormFile imageFile);
    }
}