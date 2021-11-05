using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MovieDB.Data;
using MovieDB.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MovieDB.Controllers
{
    [Area("Users")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly MovieContext context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, MovieContext context)
        {
            _logger = logger;
            _db = db;
            this.context = context;
        }

        public async Task<IActionResult> Index()
        {
            var movies = await context.GetDisplayMovies();
            return View(movies);
        }

        [HttpGet]
        public async Task<IActionResult> Index(string MovieSearch)
        {
            ViewData["GetMovies"] = MovieSearch;
            var movies = await context.GetDisplayMovies();
            if (!String.IsNullOrEmpty(MovieSearch))
            {
                movies = movies.Where(x => x.Name.ToLower().Contains(MovieSearch?.ToLower())).ToList();
            }
            return View(movies);
        }

        [Authorize]
        public async Task<IActionResult> Detail(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await context.GetMovie(id);
            movie.Comments = await context.GetComments(movie.Id);
            return View(movie);
        }

        public async Task<IActionResult> Download(string id)
        {
            var movie = await context.GetMovieBinary(id);
            return File(movie.MovieBinary, "video/mp4", movie.Name + ".mp4");
        }
    }
}