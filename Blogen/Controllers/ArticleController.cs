using Blogen.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Blogen.Controllers
{
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        {
            return RedirectToAction("list");
        }

        //
        //GET: Article/List
        public ActionResult list()
        {
            using (var database = new BlogDbContext())
            {
                var articles = database.Articles.Include(a => a.Author).ToList();

                return View(articles);
            }
               
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                var article = database.Articles.Where(a => a.Id == id).Include(a => a.Author).FirstOrDefault ();
                if (article == null)
                {
                    return HttpNotFound();
                }

                return View(article);
            }
        }

        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult Create(Article article)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    var authorId = database.Users.Where(u => u.UserName == this.User.Identity.Name).First().Id;

                    article.AuthorId = authorId;

                    database.Articles.Add(article);
                    database.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            return View(article);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDbContext())
            {
                var article = db.Articles.Where(a => a.Id == id).Include(a => a.Author).First();

                if (!IsUserAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

                }

                if (article == null)
                {
                    return HttpNotFound();
                }
                return View(article);
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDbContext())
            {
                var article = db.Articles.Where(a => a.Id == id).Include(a => a.Author).First();

                if (article == null)
                {
                    return HttpNotFound();
                }
                db.Articles.Remove(article);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            using (var db = new BlogDbContext())
            {
                var article = db.Articles.Where(a => a.Id == id).First();

                if (!IsUserAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

                }

                if (article == null)
                {
                    return HttpNotFound();
                }
                var model = new ArticleViewModel();
                model.Id = article.Id;
                model.Title = article.Title;
                model.Content = article.Content;

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult Edit(ArticleViewModel model)
        {
            //edit 
            if (ModelState.IsValid)
            {
                using (var db = new BlogDbContext())
                {
                    var article = db.Articles.FirstOrDefault(a => a.Id == model.Id);

                    article.Title = model.Title;
                    article.Content = article.Content;

                    db.Entry(article).State = EntityState.Modified;
                    db.SaveChanges();

                    return Redirect("Index");
                }
            }
            return View(model);
        }

        private bool IsUserAuthorizedToEdit(Article article)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            bool isAuthor = article.IsAuthor(this.User.Identity.Name);

            return isAdmin || isAuthor;
        }
    }
}