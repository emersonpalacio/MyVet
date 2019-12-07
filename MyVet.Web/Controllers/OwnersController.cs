using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyVet.Web.Data;
using MyVet.Web.Data.Entities;
using MyVet.Web.Helper;
using MyVet.Web.Models;

namespace MyVet.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    
    public class OwnersController : Controller
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly IImageHelper _imageHelper;

        public OwnersController(DataContext context,
                                IUserHelper userHelper,
                                ICombosHelper combosHelper,
                                IConverterHelper converterHelper,
                                IImageHelper imageHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
        }

        // GET: Owners
        public IActionResult Index()
        {
            return View(_context.Owners
                        .Include(o => o.User)
                        .Include(o => o.Pets));
        }


        // GET: Owners/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var owner = await _context.Owners
                .Include(o => o.User)
                .Include(o => o.Pets)
                .ThenInclude(p => p.PetType)
                .Include(o => o.Pets)
                .ThenInclude(p => p.Histories)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (owner == null)
            {
                return NotFound();
            }

            return View(owner);
        }


        // GET: Owners/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Owners/Create     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await AddUser(model);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "This email is already used.");
                    return View(model);
                }
                var owner = new Owner
                {
                    Pets = new List<Pet>(),
                    User = user,
                };
                _context.Owners.Add(owner);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
        //implementación de método en el mismo owner pero después de se debe pasar a un helpers.
        private async Task<User> AddUser(AddUserViewModel model)
        {
            var user = new User
            {
                Address = model.Address,
                Document = model.Document,
                Email = model.Username,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                UserName = model.Username
            };

            var result = await _userHelper.AddUserAsync(user, model.Password);
            if (result != IdentityResult.Success)
            {
                return null;
            }

            var newUser = await _userHelper.GetUserByEmailAsync(model.Username);
            await _userHelper.AddUserToRoleAsync(newUser, "Customer");
            return newUser;
        }



        // GET: Owners/Edit/5

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var owner = await _context.Owners
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id.Value);
            if (owner == null)
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                Address = owner.User.Address,
                Document = owner.User.Document,
                FirstName = owner.User.FirstName,
                Id = owner.Id,
                LastName = owner.User.LastName,
                PhoneNumber = owner.User.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var owner = await _context.Owners
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == model.Id);

                owner.User.Document = model.Document;
                owner.User.FirstName = model.FirstName;
                owner.User.LastName = model.LastName;
                owner.User.Address = model.Address;
                owner.User.PhoneNumber = model.PhoneNumber;

                await _userHelper.UpdateUserAsync(owner.User);
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }


        // GET: Owners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var owner = await _context.Owners
                .Include(o => o.Pets)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (owner == null)
            {
                return NotFound();
            }
            if (owner.Pets.Count > 0)
            {
                ModelState.AddModelError(string.Empty, "Tiene registro relacionados, no se peude borrado.");
                return RedirectToAction(nameof(Index));
            }
            await _userHelper.DeleteUserAsync(owner.User.Email);
            _context.Owners.Remove(owner);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    

        private bool OwnerExists(int id)
        {
            return _context.Owners.Any(e => e.Id == id);
        }


        [HttpGet]
        public async Task<IActionResult> AddPet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            // FindAsync = busca por la clave primaria de la tabla
            var owner = await _context.Owners.FindAsync(id.Value);
            if (owner == null)
            {
                return NotFound();
            }

            var model = new PetViewModel
            {
                Born = DateTime.Today,
                OwnerId = owner.Id,
                PetTypes = _combosHelper.GetComboPetTypes(),
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddPet(PetViewModel model)
        {
            if (ModelState.IsValid)
            {
                var path = string.Empty;
                if (model.ImageFile != null)
                {
                 
                    path = await _imageHelper.UploadImageAsync(model.ImageFile);
                }
                var pet = await _converterHelper.ToPetAsync(model, path, true);

                _context.Add(pet);
                await _context.SaveChangesAsync();
                return RedirectToAction($"Details/{model.OwnerId}");
            }

            //Se le tiene que mandar el origen de los combos
            model.PetTypes = _combosHelper.GetComboPetTypes();
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> EditPet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            // FindAsync = busca por la clave primaria de la tabla
            var pets = await _context.Pets
                                     .Include(p => p.Owner)
                                     .Include(p => p.PetType)
                                     .FirstOrDefaultAsync(p => p.Id == id);
            if (pets == null)
            {
                return NotFound();
            }
            return View(_converterHelper.ToPetViewModelAsync(pets));
        }

        [HttpPost]
        public async Task<IActionResult> EditPet(PetViewModel model)
        {
            if (ModelState.IsValid)
            {
                var path = model.ImageUrl;
                if (model.ImageFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(model.ImageFile);
                }

                var pet = await _converterHelper.ToPetAsync(model, path, false);
                _context.Pets.Update(pet);
                await _context.SaveChangesAsync();
                return RedirectToAction($"Details/{model.OwnerId}");
            }
            model.PetTypes = _combosHelper.GetComboPetTypes();
            return View(model);
        }


        public async Task<IActionResult> DetailsPet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var pet = await _context.Pets
                .Include(p => p.Owner)
                .ThenInclude(o => o.User)
                .Include(p => p.Histories)
                .ThenInclude(h => h.ServiceType)
                .FirstOrDefaultAsync(o => o.Id == id.Value);
            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }
    








        public async Task<IActionResult> AddHistory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _context.Pets.FindAsync(id.Value);
            if (pet == null)
            {
                return NotFound();
            }

            var model = new HistoryViewModel
            {
                Date = DateTime.Now,
                PetId = pet.Id,
                ServiceTypes = GetComboServiceTypes(),
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddHistory(HistoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var history = new History
                {
                    Date = model.Date,
                    Description = model.Description,
                    Pet = await _context.Pets.FindAsync(model.PetId),
                    Remarks = model.Remarks,
                    ServiceType = await _context.ServiceTypes.FindAsync(model.ServiceTypeId)
                };

                _context.Histories.Add(history);
                await _context.SaveChangesAsync();
                return RedirectToAction($"{nameof(DetailsPet)}/{model.PetId}");
            }
            model.ServiceTypes = _combosHelper.GetComboServicePetTypes();
            return View(model);
        }



        private IEnumerable<SelectListItem> GetComboServiceTypes()
        {
            var list = _context.ServiceTypes.Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.Id.ToString()
            }).OrderBy(p => p.Text).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "(Select a service type...)",
                Value = "0"
            });

            return list;
        }


        public async Task<IActionResult> EditHistory(int? id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var history = await _context.Histories
                        .Include(h => h.Pet)
                        .Include(h => h.ServiceType)
                        .FirstOrDefaultAsync(p => p.Id == id.Value);
            if (history == null)
            {
                return NotFound();
            }

            return View(_converterHelper.ToHistoryViewAsync(history));
        }

        [HttpPost]

        public async Task<IActionResult> EditHistory(HistoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var history = await _converterHelper.ToHistoryAsync(model, false);
                _context.Histories.Update(history);
                await _context.SaveChangesAsync();
                return RedirectToAction($"{nameof(DetailsPet)}/{model.PetId }");
            }
            model.ServiceTypes = _combosHelper.GetComboServicePetTypes();
            return View (model);
        }

        public async Task<IActionResult> DeleteHistory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var history = await _context.Histories
                .Include(h => h.Pet)
                .FirstOrDefaultAsync(h => h.Id == id.Value);
            if (history == null)
            {
                return NotFound();
            }

            _context.Histories.Remove(history);
            await _context.SaveChangesAsync();
            return RedirectToAction($"{nameof(DetailsPet)}/{history.Pet.Id}");
        }


        public async Task<IActionResult> DeletePet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _context.Pets
                .Include(p => p.Owner)
                .Include(p =>p.Histories )
                .FirstOrDefaultAsync(p => p.Id == id.Value);
            if (pet == null)
            {
                return NotFound();
            }
            if (pet.Histories.Count > 0)
            {
                ModelState.AddModelError(string.Empty,"posee registros relacionados.");
                return RedirectToAction($"{nameof(Details)}/{pet.Owner.Id}");
            }

            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();
            return RedirectToAction($"{nameof(Details)}/{pet.Owner.Id}");
        }



    }
}
