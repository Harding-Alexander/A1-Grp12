using Assignment1.Data;
using Assignment1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Assignment1.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Everyone can view the board
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var messages = await _context.Messages
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
            return View(messages);
        }

        // Only logged-in users can post
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // Create message: fills username & date automatically
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Content")] Message message)
        {
            if (ModelState.IsValid)
            {
                message.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                message.UserName = User.Identity?.Name;
                message.CreatedAt = DateTime.Now;

                _context.Add(message);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine($"ModelState Valid: {ModelState.IsValid}");
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"Error in {state.Key}: {error.ErrorMessage}");
                }
            }

            // If invalid, re-render form so you can see validation errors
            return View(message);
        }
    }
}
