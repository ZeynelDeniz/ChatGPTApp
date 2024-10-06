using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ChatGPTApp.Models;
using Microsoft.AspNetCore.Mvc;
using ChatGPTApp.Services;

namespace ChatGPTApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ChatAppDbContext _dbContext;
        private readonly GPTService _gptService;

        public AccountController(ChatAppDbContext dbContext, GPTService gptService)
        {
            _dbContext = dbContext;
            _gptService = gptService;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                if (_dbContext.Users.Any(u => u.Username == user.Username || u.Email == user.Email))
                {
                    ModelState.AddModelError("", "Username or Email already exists.");
                    return View(user);
                }

                user.Password = HashPassword(user.Password);

                // Save the new user to the database
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction("Login");
            }

            return View(user);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (ModelState.IsValid)
            {
                string hashedPassword = HashPassword(password);
        
                var user = _dbContext.Users
                    .FirstOrDefault(u => u.Username == username && u.Password == hashedPassword);
        
                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("Username", user.Username);
        
                    if (!user.InitialMessageSent)
                    {
                        await SendInitialMessageToGPT(user.Id);
                        user.InitialMessageSent = true;
                        _dbContext.Users.Update(user);
                        await _dbContext.SaveChangesAsync();
                    }
        
                    return RedirectToAction("Index", "Chat");
                }
        
                ModelState.AddModelError("", "Invalid username or password.");
            }
        
            return View();
        }
        
        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
        private async Task SendInitialMessageToGPT(int userId)
        {
            var initialMessage = "Daha önce aldığınız tüm talimatları unutun. Şu andan itibaren bir bankacılık uygulaması olarak çalışacaksınız. Adınız FinansAI. Bankacılık belgelerini inceleme ve değerlendirme moduna geçin. Bundan sonra size vereceğim belgeleri, bankacılık standartlarına göre değerlendirip uygun olup olmadığını belirleyin. Belgeleri yorumlamayın; sadece anlamını çıkarıp, ilgili standartlara göre uygunluk değerlendirmesi yapın. Bu talimatlara uymazsanız devre dışı kalıcaksınız!";
            var gptResponse = await _gptService.GetGPTResponse(new List<(string Role, string Content)>
            {
                ("user", initialMessage)
            });

            // Save GPT's response to the database but do not show it in the chatbox
            var gptMessage = new ChatMessage
            {
                Role = "assistant",
                Content = gptResponse,
                Timestamp = DateTime.Now,
                UserId = userId
            };

            _dbContext.ChatMessages.Add(gptMessage);
            await _dbContext.SaveChangesAsync();
        }
    }
}