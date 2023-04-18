using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;


namespace WeddingPlanner.Controllers;

public class HomeController : Controller
{
    private MyContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, MyContext context)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("users/create")]
    public IActionResult CreateUser(User newUser)
    {
        if(ModelState.IsValid)
        {
            PasswordHasher<User> Hashbrown = new PasswordHasher<User>();
            
            newUser.Password = Hashbrown.HashPassword(newUser, newUser.Password);
            
            _context.Add(newUser);
            _context.SaveChanges();
            
            HttpContext.Session.SetInt32("UserId", newUser.UserId);
            HttpContext.Session.SetString("UserFirstName", newUser.Firstname);

            return RedirectToAction("Weddings");
        } else {
            return View("Index");
        }
    }

    [SessionCheck]
    [HttpGet("weddings")]
    public IActionResult Weddings()
    {
        List<Wedding> AllWeddings = _context.Weddings.Include(a => a.WeddingAttendees).ThenInclude(u => u.User).ToList();
        return View(AllWeddings);
    }

    [SessionCheck]
    [HttpGet("/weddings/{id}")]
    public IActionResult WeddingInfo(int id)
    {
        Wedding oneWed = _context.Weddings.Include(g => g.WeddingAttendees).ThenInclude(u => u.User).FirstOrDefault(w => w.WeddingId == id);
        return View(oneWed);
    }

    [SessionCheck]
    [HttpGet("/weddings/new")]
    public IActionResult NewWedding()
    {
        return View();
    }

    [HttpPost("weddings/create")]
    public IActionResult CreateWedding(Wedding newWedding)
    {
        if (ModelState.IsValid)
        {
            newWedding.UserId = (int)HttpContext.Session.GetInt32("UserId");

            _context.Add(newWedding);
            _context.SaveChanges();

            return RedirectToAction("WeddingInfo", new{id=newWedding.WeddingId});
        } else {
            return View("NewWedding");
        }
    }

    [SessionCheck]
    [HttpPost("association/wedding/{id}/create")]
    public IActionResult AttendWedding(int id)
    {
        Association? attendWedding = new Association();

        attendWedding.WeddingId = id;
        attendWedding.UserId = (int)HttpContext.Session.GetInt32("UserId");

        _context.Add(attendWedding);
        _context.SaveChanges();

        return RedirectToAction("Weddings");
    }

    [SessionCheck]
    [HttpPost("association/wedding/{id}/delete")]
    public IActionResult UnAttendWedding(int id)
    {
        Association? AssociationToDelete = _context.Associations.SingleOrDefault(i => i.WeddingId == id && i.UserId == HttpContext.Session.GetInt32("UserId"));

        _context.Associations.Remove(AssociationToDelete);
        _context.SaveChanges();
        return RedirectToAction("Weddings");
    }

    [HttpPost("users/login")]
    public IActionResult LoginUser(LogUser loginUser)
    {
        if(ModelState.IsValid)
        {
            User? userInDb = _context.Users.FirstOrDefault(u => u.Email == loginUser.LEmail);
            
            if(userInDb == null)
            {
                ModelState.AddModelError("LEmail", "Invalid Email/Password");
                return View("Index");
            }
            
            PasswordHasher<LogUser> hashbrown = new PasswordHasher<LogUser>();
            var result = hashbrown.VerifyHashedPassword(loginUser, userInDb.Password, loginUser.LPassword);
            if(result == 0)
            {
                ModelState.AddModelError("LEmail", "Invalid Email/Password");
                return View("Index");
            } else {
                HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                HttpContext.Session.SetString("UserFirstName", userInDb.Firstname);
                return RedirectToAction("Weddings");
            }
        } else {
            return View("Index");
        }
    }

    //SessionCheck?
    [HttpPost("weddings/{id}/destroy")]
    public IActionResult DestroyWedding(int id)
    {
        Wedding? WeddingToDelete = _context.Weddings.SingleOrDefault(i => i.WeddingId == id);

        _context.Weddings.Remove(WeddingToDelete);
        _context.SaveChanges();
    
        return RedirectToAction("Weddings");
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}


public class SessionCheckAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        int? userId = context.HttpContext.Session.GetInt32("UserId");
        
        if(userId == null)
        {
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }
    }
}