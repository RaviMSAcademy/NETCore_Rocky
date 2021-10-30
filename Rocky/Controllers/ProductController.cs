using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Rocky.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(ApplicationDBContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            IEnumerable<Product> objList = _db.Product.Include(p => p.Category).Include(p => p.ApplicationType);
            //foreach (var obj in objList)
            //{
            //    obj.Category = _db.Category.FirstOrDefault(i => i.Id == obj.CategoryId);
            //    obj.ApplicationType = _db.ApplicationType.FirstOrDefault(i => i.Id == obj.ApplicationId);
            //}
            return View(objList);
        }

        //GET - Upsert
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _db.Category.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                ApplicationTypeSelectList = _db.ApplicationType.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            //New entry
            if (id == null)
            {

                return View(productVM);
            }
            //Edit
            else
            {
                productVM.Product = _db.Product.Find(id);
                if (productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var webRootPath = _webHostEnvironment.WebRootPath;
                string uploadDir = webRootPath + WC.ImagePath;

                if (productVM.Product.Id == 0)
                {
                    var files = HttpContext.Request.Form.Files;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(files[0].FileName);

                    using (var fileStream = new FileStream(Path.Combine(uploadDir, fileName), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    productVM.Product.Image = fileName;

                    _db.Product.Add(productVM.Product);
                }
                else
                {
                    var files = HttpContext.Request.Form.Files;
                    var objProductFromDB = _db.Product.AsNoTracking().FirstOrDefault(p => p.Id == productVM.Product.Id);

                    if (files.Count() > 0)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(files[0].FileName);

                        string oldFile = Path.Combine(uploadDir, objProductFromDB.Image);

                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }

                        using (var fileStream = new FileStream(Path.Combine(uploadDir, fileName), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }
                        productVM.Product.Image = fileName;
                    }
                    else
                    {
                        productVM.Product.Image = objProductFromDB.Image;
                    }

                    _db.Product.Update(productVM.Product);
                }
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            productVM.CategorySelectList = _db.Category.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            productVM.ApplicationTypeSelectList = _db.ApplicationType.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            return View(productVM);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Product product = _db.Product.Include(u => u.Category).Include(a=>a.ApplicationType).FirstOrDefault(u => u.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.Product.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            var webRootPath = _webHostEnvironment.WebRootPath;
            string uploadDir = webRootPath + WC.ImagePath;
            string oldFile = Path.Combine(uploadDir, obj.Image);
          
            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }
            _db.Product.Remove(obj);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
