using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieDB.Data;
using MovieDB.Models;
using MovieDB.Services;
using MovieDB.Utility;
using System;
using System.Threading.Tasks;

namespace MovieDB.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.Admin)]
    [Area("Admin")]
    public class Dashboard : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly MovieContext context;

        public Dashboard(ApplicationDbContext db, MovieContext context)
        {
            _db = db;
            this.context = context;
        }

        public MoviesDB MovieDB { get; set; }

        public async Task<IActionResult> Index(MoviesDB result)
        {
            var movies = await context.GetMovies();
            return View(movies);
        }

        //create button action
        public IActionResult Add()
        {
            return View();
        }

        //saving in db
        [HttpPost, ActionName("Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Added(MovieModel model)
        {
            if (ModelState.IsValid)
            {
                await context.AddOrUpdate(model);
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        //detail view
        public async Task<IActionResult> Movie(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await context.GetMovie(id);
            movie.Comments = await context.GetComments(movie.Id);
            return View(movie);
        }

        //edit page
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await context.GetMovie(id);
            return View(movie);
        }

        //edit and save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MovieModel model)
        {
            if (model.Id == null)
            {
                return NotFound();
            }

            await context.AddOrUpdate(model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            await context.RemoveMovie(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Download(string id)
        {
            var movie = await context.GetMovieBinary(id);
            return File(movie.MovieBinary, "video/mp4", movie.Name + ".mp4");
        }

        [HttpGet]
        public async Task<IActionResult> PostComment(string movieId, string comment, int rating, string commentId)
        {
            if (!string.IsNullOrWhiteSpace(commentId))
            {
                var cmt = await context.GetComment(commentId);

                if (cmt.Date.AddDays(1) < DateTime.Now)
                {
                    return BadRequest();
                }
            }

            var model = new CommentModel() { Comment = comment, MovieId = movieId, Rating = rating, UserId = User.Identity.Name, Id = commentId };
            await context.AddOrUpdateComment(model);
            return Ok();
        }
    }
}