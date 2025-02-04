using Cars.AppDbContext;
using Cars.Models;
using Cars.Models.ViewModels;
using cloudscribe.Pagination.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Cars.Views.Car;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Cars.Controllers
{
    public class CarController : Controller
    {
        private readonly VroomDbContext vroomDbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;


        [BindProperty]
        public CarViewModel CarVM { get; set; }

        public CarController(VroomDbContext vroomDbContext, IWebHostEnvironment hostingEnvironment)
        {
            this.vroomDbContext = vroomDbContext;
            CarVM = new CarViewModel()
            {
                Makes = vroomDbContext.Makes.ToList(),
                Models = vroomDbContext.Models.ToList(),
                Car = new Car()
            };
            _hostingEnvironment = hostingEnvironment;
        }

        // Action for both Guests and Executives to view all cars
        public IActionResult Index(string searchString, string sortOrder, int pageNumber = 1, int pageSize = 6)
        {
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentFilter = searchString;
            ViewBag.PriceSortParam = string.IsNullOrEmpty(sortOrder) ? "Price_desc" : "";

            int excludeRecords = (pageSize * pageNumber) - pageSize;

            var cars = vroomDbContext.Cars.Include(m => m.Make).Include(m => m.Model).AsQueryable();

            // Search functionality
            if (!string.IsNullOrEmpty(searchString))
            {
                cars = cars.Where(b => b.Make.Name.Contains(searchString));
            }

            // Sorting functionality
            cars = sortOrder == "Price_desc"
                ? cars.OrderByDescending(b => b.Price)
                : cars.OrderBy(b => b.Price);

            // Pagination
            var pagedResult = new PagedResult<Car>
            {
                Data = cars.Skip(excludeRecords).Take(pageSize).AsNoTracking().ToList(),
                TotalItems = cars.Count(),
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            ViewBag.TotalPages = (int)Math.Ceiling((double)pagedResult.TotalItems / pageSize);

            // Check if the user is an Admin
            if (User.IsInRole("Admin"))
            {
                return View("Index", pagedResult);  // Admins see the "AdminView"
            }

            // Check if the user is an Executive
            if (User.IsInRole("Executive"))
            {
                return View("GuestView", pagedResult);  // Executives see the "ExecutiveView"
            }

            // Default view for guests (and others)
            return View("GuestView", pagedResult);  // Non-admin, non-executive users see the "GuestView"
        }


        // Action for Admin to view all cars (Admin-specific Index)
        [Authorize(Roles = "Admin")]
        public IActionResult AdminIndex(string searchString, string sortOrder, int pageNumber = 1, int pageSize = 6)
        {
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentFilter = searchString;
            ViewBag.PriceSortParam = string.IsNullOrEmpty(sortOrder) ? "Price_desc" : "";

            int excludeRecords = (pageSize * pageNumber) - pageSize;

            var cars = vroomDbContext.Cars.Include(m => m.Make).Include(m => m.Model).AsQueryable();

            // Search functionality
            if (!string.IsNullOrEmpty(searchString))
            {
                cars = cars.Where(b => b.Make.Name.Contains(searchString));
            }

            // Sorting functionality
            cars = sortOrder == "Price_desc"
                ? cars.OrderByDescending(b => b.Price)
                : cars.OrderBy(b => b.Price);

            // Pagination
            var pagedResult = new PagedResult<Car>
            {
                Data = cars.Skip(excludeRecords).Take(pageSize).AsNoTracking().ToList(),
                TotalItems = cars.Count(),
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            ViewBag.TotalPages = (int)Math.Ceiling((double)pagedResult.TotalItems / pageSize);

            // Return the "Index" view for Admin, showing all cars
            return View("Index", pagedResult); // You can reuse the existing "Index" view here.
        }


        // Action for Executives to view only their cars (MyCars)
        [Authorize(Roles = "Executive")]
        public IActionResult MyCars(string searchString, string sortOrder, int pageNumber = 1, int pageSize = 6)
        {
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentFilter = searchString;
            ViewBag.PriceSortParam = string.IsNullOrEmpty(sortOrder) ? "Price_desc" : "";

            int excludeRecords = (pageSize * pageNumber) - pageSize;

            // Get only cars owned by the current executive user (SellerEmail = User.Identity.Name)
            var cars = vroomDbContext.Cars
                .Where(c => c.SellerName == User.Identity.Name) // Filter by current user's email
                .Include(m => m.Make)
                .Include(m => m.Model)
                .AsQueryable();

            // Search functionality for My Cars
            if (!string.IsNullOrEmpty(searchString))
            {
                cars = cars.Where(b => b.Make.Name.Contains(searchString)); // Filter by Make name
            }

            // Sorting functionality for My Cars
            cars = sortOrder == "Price_desc"
                ? cars.OrderByDescending(b => b.Price) // Sort by descending price
                : cars.OrderBy(b => b.Price); // Sort by ascending price

            // Pagination
            var pagedResult = new PagedResult<Car>
            {
                Data = cars.Skip(excludeRecords).Take(pageSize).AsNoTracking().ToList(),
                TotalItems = cars.Count(),
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            ViewBag.TotalPages = (int)Math.Ceiling((double)pagedResult.TotalItems / pageSize);

            // Return the same "Index" view for executives, but filtered to show only their own cars
            return View("MyCars", pagedResult); // Reusing the "Index" view for the "MyCars" page
        }


        // Action for Executives to Create a New Car
        [Authorize(Roles = "Executive,Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(CarVM);
        }

        // Action for Executives to Save a New Car
        [Authorize(Roles = "Executive,Admin")]
        [HttpPost, ActionName("Create")]
        [HttpPost]
        public async Task<IActionResult> CreatePost(CarViewModel CarVM, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // Define folder to store images
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");

                // Ensure the directory exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Save path in model
                CarVM.Car.ImagePath = "/images/" + uniqueFileName;
            }

            // Assign the current user's info
            CarVM.Car.SellerName = User.Identity.Name;
            CarVM.Car.SellerEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            // Add car to database
            vroomDbContext.Add(CarVM.Car);
            await vroomDbContext.SaveChangesAsync();

            // Redirect based on role
            return User.IsInRole("Admin") ? RedirectToAction(nameof(AdminIndex)) : RedirectToAction(nameof(ExecutiveView));
        }



        // Action for Executives to Edit an Existing Car
        [Authorize(Roles = "Executive,Admin")]
        [HttpGet("Car/Edit/{id}")]
        public IActionResult Edit(int id)
        {
            CarVM.Car = vroomDbContext.Cars.Find(id);
            if (CarVM.Car == null)
            {
                return NotFound();
            }
            CarVM.Makes = vroomDbContext.Makes.ToList();
            CarVM.Models = vroomDbContext.Models.ToList();
            return View(CarVM);
        }

        // Action for Executives to Save Edited Car Details
        [Authorize(Roles = "Executive,Admin")]
        [HttpPost, ActionName("Edit")]
        public async Task<IActionResult> EditPost(IFormFile file)
        {
            if (file != null)
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                CarVM.Car.ImagePath = "/images/" + uniqueFileName;
            }
            CarVM.Car.SellerName = User.Identity.Name ?? "Default Seller Name";  // Set a default if it's null
            CarVM.Car.SellerEmail = User.FindFirst(ClaimTypes.Email)?.Value;  // Use Email claim



            vroomDbContext.Update(this.CarVM.Car);
            await vroomDbContext.SaveChangesAsync();

            // Redirect based on role
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(AdminIndex));  // Admin redirects to the general Index page
            }
            else
            {
                return RedirectToAction(nameof(MyCars));  // Executive redirects to MyCars page
            }
        }


        [Authorize(Roles = "Executive,Admin")]
        [HttpGet("Car/Delete/{id}")]
        public IActionResult Delete(int id)
        {
            var car = vroomDbContext.Cars.Find(id);
            if (car == null)
            {
                return NotFound();
            }
            vroomDbContext.Cars.Remove(car);
            vroomDbContext.SaveChanges();

            // Redirect based on role
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(AdminIndex));  // Admin redirects to the general Index page
            }
            else
            {
                return RedirectToAction(nameof(MyCars));  // Executive redirects to MyCars page
            }
        }


        [Authorize(Roles = "Executive")]
        public IActionResult ExecutiveView(string searchString, string sortOrder, int pageNumber = 1, int pageSize = 6)
        {
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentFilter = searchString;
            ViewBag.PriceSortParam = string.IsNullOrEmpty(sortOrder) ? "Price_desc" : "";

            int excludeRecords = (pageSize * pageNumber) - pageSize;

            var cars = vroomDbContext.Cars.Include(m => m.Make).Include(m => m.Model).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                cars = cars.Where(b => b.Make.Name.Contains(searchString));
            }

            cars = sortOrder == "Price_desc"
                ? cars.OrderByDescending(b => b.Price)
                : cars.OrderBy(b => b.Price);

            var pagedResult = new PagedResult<Car>
            {
                Data = cars.Skip(excludeRecords).Take(pageSize).AsNoTracking().ToList(),
                TotalItems = cars.Count(),
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            ViewBag.TotalPages = (int)Math.Ceiling((double)pagedResult.TotalItems / pageSize);

            return View("Index", pagedResult); // Reusing the "Index" view for Executive users
        }
    }
}
