using Cars.AppDbContext;
using Cars.Helpers;
using Cars.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cars.Controllers
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Executive)]
    public class MakeController : Controller
    {
        private readonly VroomDbContext vroomDbContext;

        public MakeController(VroomDbContext vroomDbContext)
        {
            this.vroomDbContext = vroomDbContext;
        }

        public IActionResult Index()
        {
            return View(vroomDbContext.Makes.ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Make make)
        {
            if (ModelState.IsValid)
            {
                vroomDbContext.Add(make);
                vroomDbContext.SaveChanges();
                return (RedirectToAction(nameof(Index)));
            }
            return View(make);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var make = vroomDbContext.Makes.Find(id);
            if (make == null)
            {
                return NotFound();
            }
            vroomDbContext.Makes.Remove(make);
            vroomDbContext.SaveChanges();
            return (RedirectToAction(nameof(Index)));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var make = vroomDbContext.Makes.Find(id);
            if (make == null)
            {
                return NotFound();
            }
            return View(make);
        }


        [HttpPost]
        public IActionResult Edit(Make make)
        {
            if (ModelState.IsValid)
            {
                vroomDbContext.Update(make);
                vroomDbContext.SaveChanges();
                return (RedirectToAction(nameof(Index)));
            }
            return View(make);
        }
    }

}
