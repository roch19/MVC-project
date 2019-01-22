using Projekt.Models.Data;
using Projekt.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PagedList;

namespace Projekt.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {

            //Declare list of models
            List<CategoryVM> categoryVMList;
            using(Db db = new Db())
            {
                //Init the list
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }

            //return view with list
            return View(categoryVMList);
        }
        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //Declare id
            string id;
            using (Db db = new Db())
            {
                //check that the category name is unique 
                if(db.Categories.Any(x => x.Name == catName))
                {
                    return "titletaken";
                }
                //init DTO
                CategoryDTO dto = new CategoryDTO();

                //Add DTO
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                //Save DTO
                db.Categories.Add(dto);
                db.SaveChanges();
                //Get the id
                id = dto.Id.ToString();
            }
            //Return Id
            return id;
        }

        // POST: Admin/Shop/ReorderCategories/id
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //set initial count
                int count = 1;
                //Declare PageDTO
                CategoryDTO dto;
                //Set sorting for each category
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }

        }

        // GET: Admin/Shop/DeleteCategory/id
        public ActionResult DeleteCategory(int id)
        {

            using (Db db = new Db())
            {
                //Get the category
                CategoryDTO dto = db.Categories.Find(id);
                //Remove Page
                db.Categories.Remove(dto);
                //Save 
                db.SaveChanges();
            }
            //Redirect
            return RedirectToAction("Categories");
        }

        // POST: Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                // Check category name is unique
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";
                // Get DTO
                CategoryDTO dto = db.Categories.Find(id);


                // Edit DTO 
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                //Save
                db.SaveChanges();
            }
            return "ok";
        }

        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            // Init model 
            ProductVM model = new ProductVM();
            //AdditionalMetadataAttribute selectet list of categories to model 

            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

            }
            // Return view with model
            return View(model);


        }

        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {

            //check model state
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }
            // make sure product name unique
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "Ta nazwa jest już wykorzystywana!");
                    return View(model);
                }
            }
            //declare product id
            int id;
            // init and save productDTO
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Slug;
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();


                // get id
                id = product.Id;
            }

            // set TempData message
            TempData["SM"] = "Dodałeś nowy produkt!";
            #region Upload Image

            //Create necessary direct
            var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));


             
            var pathString1 = Path.Combine(orginalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            //Check if a file was uploaded
            if (file != null && file.ContentLength >0 )
            {
                //Get file extension
                string ext = file.ContentType.ToLower();
                // Verify extension
                if (ext != "image/jpg" && 
                    ext != "image/jpeg" && 
                    ext != "image/pjpeg" 
                    && ext != "image/gif" 
                    && ext != "image/x-png" 
                    && ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                       
                            model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                            ModelState.AddModelError("", "Zdjęcie nie zostało załadowane!");
                            return View(model);
                    }
                }
                //Init image name
                string imageName = file.FileName;

                // Save image name to DTO
                using (Db db= new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }
                // Set orginal and thumb image paths
                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);
                // Save orginal image
                file.SaveAs(path);
                //Create and save thumb
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }
            #endregion

            // Redirect
            return RedirectToAction("AddProduct");
        }

        // GET: Admin/Shop/Products
        public ActionResult Products(int? page, int? catId)
        {
            //Declare a list of ProductVM
            List<ProductVM> listOfProductVM;

            //Set page number
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                //Init the list
                listOfProductVM = db.Products.ToArray()
                                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                    .Select(x => new ProductVM(x))
                                    .ToList();

                //Populate categories select list 
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");


                //Set selected category
                ViewBag.SelectedCat = catId.ToString();

            }



            //Set pagination
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 4);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            //Return view with list

            return View(listOfProductVM);
        }

        // GET: Admin/Shop/EditProduct/id
        public ActionResult EditProduct(int id)
        {    //Declare productVM
            ProductVM model;

            using (Db db = new Db())
            {
                //Get the product
                ProductDTO dto = db.Products.Find(id);
                
                //Make sure product exists
                if (dto == null)
                {
                    return Content("Ten produkt nie istnieje.");
                }
                //init model
                model = new ProductVM(dto);

                // make a select list
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //get all gallery images
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                               .Select(fn => Path.GetFileName(fn));
            }
          
            //return view with model

            return View(model);
        }


        // POST: Admin/Shop/EditProduct/id
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {

            //Get product id
            int id = model.Id;
            //Populate categories select list and gallery images 

            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                              .Select(fn => Path.GetFileName(fn));
            //CHeck modle state
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            //Make sure product name is unique 

            using (Db db = new Db())
            {
                if(db.Products.Where(x => x.Id != id).Any(x=> x.Name == model.Name))
                {
                    ModelState.AddModelError("", "Ta nazwa produktu jest już wykorzystywana!");
                    return View(model);
                }
            }
            //Update product
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Slug;
                dto.Price = model.Price;
                dto.Description = model.Description;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }
            //Set TempData message
            TempData["SM"] = "Produkt został zedytowany!";
                #region Image Upload
                    //Check for file upload
                    if(file != null && file.ContentLength > 0){

                //Get extension
                string ext = file.ContentType.ToLower();

                //Verify extension

                if (ext != "image/jpg" &&
               ext != "image/jpeg" &&
               ext != "image/pjpeg"
               && ext != "image/gif"
               && ext != "image/x-png"
               && ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "Zdjęcie nie zostało załadowane!");
                        return View(model);
                    }
                }
                //Set upload directory paths

                var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                var pathString1 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs") ;

                //Delete files from directories

                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (FileInfo file2 in di1.GetFiles())
                    file2.Delete();

                foreach (FileInfo file3 in di2.GetFiles())
                    file3.Delete();

                //Save image name

                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }
                //Save orginal and thumb images

                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }


            #endregion
            return RedirectToAction("EditProduct");
        }

        // GET : Admin/Shop/DeleteProduct/id
        public ActionResult DeleteProduct(int id)
        {
            //Delete product from DB
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }
            //Delete product folder
            var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            string pathString = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);

                return RedirectToAction("Products");
        }

        // POST: Admin/Shop/SaveGalleryImages
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            // Loop through files
            foreach (string fileName in Request.Files)
            {
                // Init the file
                HttpPostedFileBase file = Request.Files[fileName];

                //Check it's not null
                if (file.ContentLength > 0)
                {
                    //Set directory paths
                    var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");
                    //Set image paths
                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);


                    //Save orginal and thumb
                    file.SaveAs(path);
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);

                }
                else {; }

            }
        }
    }
}