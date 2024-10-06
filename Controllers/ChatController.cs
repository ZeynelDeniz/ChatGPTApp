using System.Linq;
using System.Threading.Tasks;
using ChatGPTApp.Models;
using ChatGPTApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;

namespace ChatGPTApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly GPTService _gptService;
        private readonly ChatAppDbContext _dbContext;

        public ChatController(GPTService gptService, ChatAppDbContext dbContext)
        {
            _gptService = gptService;
            _dbContext = dbContext;
        }

        // GET: /Chat/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var username = HttpContext.Session.GetString("Username");
        
            if (userId == null || username == null)
            {
                return RedirectToAction("Login", "Account");
            }
        
            var conversationHistory = await _dbContext.ChatMessages
                .Where(c => c.UserId == userId.Value)
                .OrderBy(c => c.Timestamp)
                .ToListAsync();
        
            ViewBag.Username = username; 
        
            return View(conversationHistory ?? new List<ChatMessage>());
        }

        [HttpPost]
        public async Task<IActionResult> Index(ChatMessage chatMessage)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var username = HttpContext.Session.GetString("Username");

            if (userId == null || username == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(chatMessage.Content))
            {
                ModelState.AddModelError("Content", "Message content cannot be empty.");
                return PartialView("_ChatMessages", await GetConversationHistory(userId.Value));
            }

            chatMessage.Role = "user";
            chatMessage.Timestamp = DateTime.Now;
            chatMessage.UserId = userId.Value;
            _dbContext.ChatMessages.Add(chatMessage);
            await _dbContext.SaveChangesAsync();

            try
            {
                // Prepare the conversation history for the GPT API
                var apiConversationHistory = await _dbContext.ChatMessages
                    .Where(c => c.UserId == userId.Value)
                    .OrderBy(c => c.Timestamp)
                    .Select(c => new { Role = c.Role, Content = c.Content })
                    .ToListAsync();

                var tupleConversationHistory = apiConversationHistory
                    .Select(c => (c.Role, c.Content))
                    .ToList();

                // Get the response from the GPT API
                var gptResponse = await _gptService.GetGPTResponse(tupleConversationHistory);

                // Save GPT's response to the database
                var gptMessage = new ChatMessage
                {
                    Role = "assistant",
                    Content = gptResponse,
                    Timestamp = DateTime.Now,
                    UserId = userId.Value
                };

                _dbContext.ChatMessages.Add(gptMessage);
                await _dbContext.SaveChangesAsync();

                var conversationHistory = await _dbContext.ChatMessages
                .Where(c => c.UserId == userId.Value && c.ShowInChatbox) // Filter messages
                .OrderBy(c => c.Timestamp)
                .ToListAsync();

                return PartialView("_ChatMessages", conversationHistory);

            }
            catch (Exception ex)
            {
                // Log the error details
                Console.WriteLine($"Error: {ex.Message}\nStack Trace: {ex.StackTrace}");

                // Save a placeholder response to the database
                var errorResponse = new ChatMessage
                {
                    Role = "assistant",
                    Content = "An error occurred while processing your request. Please try again.",
                    Timestamp = DateTime.Now,
                    UserId = userId.Value
                };
                _dbContext.ChatMessages.Add(errorResponse);
                await _dbContext.SaveChangesAsync();
            }

            var updatedConversationHistory = await GetConversationHistory(userId.Value);

            return PartialView("_ChatMessages", updatedConversationHistory);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }

        private async Task<List<ChatMessage>> GetConversationHistory(int userId)
        {
            return await _dbContext.ChatMessages
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Timestamp)
                .ToListAsync();
        }
    }
}