using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize (Roles ="Admin")]
    public class OwnersController : Controller
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly ICombosHelper _combosHelper;

        public OwnersController(DataContext context,
                                IUserHelper userHelper,
                                ICombosHelper combosHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _combosHelper = combosHelper;
        }

        // GET: Owners
        public IActionResult Index()
        {
            return View(_context.Owners
                        .Include(o => o.User)
                        .Include(o =>o.Pets));
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

            var owner = await _context.Owners.FindAsync(id);
            if (owner == null)
            {
                return NotFound();
            }
            return View(owner);
        }

        // POST: Owners/Edit/5
  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id")] Owner owner)
        {
            if (id != owner.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(owner);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OwnerExists(owner.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(owner);
        }

        // GET: Owners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var owner = await _context.Owners
                .FirstOrDefaultAsync(m => m.Id == id);
            if (owner == null)
            {
                return NotFound();
            }

            return View(owner);
        }

        // POST: Owners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var owner = await _context.Owners.FindAsync(id);
            _context.Owners.Remove(owner);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OwnerExists(int id)
        {
            return _context.Owners.Any(e => e.Id == id);
        }


        // GET: Owners/Edit/5
        public async Task<IActionResult> AddPet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var owner = await _context.Owners.FindAsync(id);
            if (owner == null)
            {
                return NotFound();
            }
            return View(owner);
        }














        //pgina 62 big problem


        // POST:AddPet
        //public async Task<IActionResult> AddPet(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }
        //    // FindAsync = busca por la clave primaria de la tabla
        //    var owner = await _context.Owners.FindAsync(id.Value);
        //    if (owner == null)
        //    {
        //        return NotFound();
        //    }

        //    var model = new PetViewModel
        //    {
        //        Born = DateTime.Today,
        //        OwnerId = owner.Id,
        //        PetTypes = _combosHelper.GetComboPetTypes(),
        //    };

        //    return View(model);
        //}

        //[HttpPost]
        //public async Task<IActionResult> AddPet(PetViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var path = string.Empty;
        //        if (model.ImageFile != null)
        //        {
        //            var guid = Guid.NewGuid().ToString();
        //            var file = $"{guid}.jpg";

        //            path = Path.Combine(
        //                Directory.GetCurrentDirectory(),
        //                "wwwroot\\images\\Pets",
        //                 file);

        //            using (var stream = new FileStream(path, FileMode.Create))
        //            {
        //                await model.ImageFile.CopyToAsync(stream);
        //            }
        //            path = $"~/images/Pets/{file}";
        //        }
        //        var pet = await _converterHelper.ToPetAsync(model, path);

        //        _context.Add(pet);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction($"Details/{model.OwnerId}");
        //    }
        //    return View(model);
        //}





    }



}
