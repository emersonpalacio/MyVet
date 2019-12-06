using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyVet.Web.Helper
{
    public interface ICombosHelper
    {
        IEnumerable<SelectListItem> GetComboPetTypes();

        IEnumerable<SelectListItem> GetComboServicePetTypes();


    }
}