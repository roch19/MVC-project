using Projekt.Models.Data;
using Projekt.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Projekt.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //Dekleracja storny PagesViewModels
            List<PageVM> pagesList;

            

            using (Db db = new Db())
            {    //Inicjalizacja listy
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }
            // Zwraca widok listy
            return View(pagesList);
        }
        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //check model state
            
            if(! ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {
                //declare slug
                string slug;

                //init pageDTO
                PageDTO dto = new PageDTO();

                //DTO title
                dto.Title = model.Title;

                //Chceck for and set slug if need be
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                //Make sure title and slug are unique
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug) )
                {
                    ModelState.AddModelError("", "That title or slug alredy exist.");
                    return View(model);
                }


                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                //Save DTO
                db.Pages.Add(dto);
                db.SaveChanges();
            }
            //Set TempData message 
            TempData["SM"] = "Tou have added a new page!";

            //Redirect 
            return RedirectToAction("AddPage");
            
        }
        
        // GET: Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //Declare pageVM
            PageVM model;

            using (Db db = new Db())
            {

                //Get the page
                PageDTO dto = db.Pages.Find(id);

                //Confirm page exists 
                if (dto == null)
                {
                    return Content("Strona nie istnieje.");
                }

                //Init pageVM
                model = new PageVM(dto);
            }

            //Return vwiew with model
            return View(model);
        }

        // POST: Admin/Pages/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                //Get page ID
                int id = model.Id;

                //init slug
                string slug= "home";

                //Get the page
                PageDTO dto = db.Pages.Find(id);

                //DTO the title 
                dto.Title = model.Title;

                //Check for slug and set it if need be
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }

                }


                //Make sure title and slug are unique 
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "Istnieje dany tutuł lub tag");
                    return View(model);
                }
                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;


                //Save the DTO
                db.SaveChanges();
            }
            //Set TempData message
            TempData["SM"] = "Strona została edytowana!";

            return RedirectToAction("EditPage");
        }

        // GET: Admin/Pages/PageDetails/id
        public ActionResult PageDetails(int id)
        {   // Declare PageVm
            PageVM model;

            using (Db db = new Db())
            {
                // Get The page
                PageDTO dto = db.Pages.Find(id);

                //Confirm Page exist
                if (dto == null)
                {
                    return Content("Dana strona nie istnieje.");
                }
                //Init PageVM
                model = new PageVM(dto);

            }


            //Return view with model
            return View(model);
        }
      
        // GET: Admin/Pages/DeletePage/id
        public ActionResult DeletePage(int id)
        {
            
            using (Db db = new Db())
            {
                //Get page
                PageDTO dto = db.Pages.Find(id);
                //Remove Page
                db.Pages.Remove(dto);
                //Save 
                db.SaveChanges();
            }
            //Redirect
            return RedirectToAction("Index");
        }

        // POST: Admin/Pages/ReorderPages/id
        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                //set initial count
                int count = 1;
                //Declare PageDTO
                PageDTO dto;
                //Set sorting for each page
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;
                    db.SaveChanges();

                    count++;
                }
            }

        }

        // GET: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //Declare model
            SidebarVM model;
            using (Db db = new Db())
            {
                //Get DTO
                SidebarDTO dto = db.Sidebar.Find(1);

                //Init model
                model = new SidebarVM(dto);
            }
            //Return view with model
            return View(model);
        }


        // POST: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {

            using (Db db = new Db())
            {
                //Get DTO
                SidebarDTO dto = db.Sidebar.Find(1);
                //DTO the body                    
                dto.Body = model.Body;

                //Save
                db.SaveChanges();
            }
            //Set TempData message
            TempData["SM"] = "Pasek nawigacyjny został edytowany.";

            //Redierect
            return RedirectToAction("EditSidebar");
        }
    }
}