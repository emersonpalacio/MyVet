using MyVet.Web.Data;
using MyVet.Web.Data.Entities;
using MyVet.Web.Models;
using System.Threading.Tasks;

namespace MyVet.Web.Helper
{
    public class ConverterHelper : IConverterHelper
    {
        private readonly DataContext _dataContext;

        public ConverterHelper(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public async Task<Pet> ToPetAsync(PetViewModel model, string path, bool isNew)
        {
            var pet= new Pet
            {
                Agendas = model.Agendas,
                Born = model.Born,
                Histories = model.Histories,  
                Id= isNew ? 0 : model.Id ,
                ImageUrl = path,
                Name = model.Name,
                Owner = await _dataContext.Owners.FindAsync(model.OwnerId),
                PetType = await _dataContext.PetTypes.FindAsync(model.PetTypeId),
                Race = model.Race,
                Remarks = model.Remarks
            };        

            return pet;
        }
    }
}
