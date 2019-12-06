using MyVet.Web.Data.Entities;
using MyVet.Web.Models;
using System.Threading.Tasks;

namespace MyVet.Web.Helper
{
    public interface IConverterHelper
    {
        Task<Pet> ToPetAsync(PetViewModel model, string path, bool isNew);

        PetViewModel ToPetViewModelAsync(Pet pet);

        //nos convierte de HistoryViewModel a History
        Task<History> ToHistoryAsync(HistoryViewModel model, bool isNew);

        //nos convierte de History a HistoryViewModel
        HistoryViewModel ToHistoryViewAsync(History history);

    }
}