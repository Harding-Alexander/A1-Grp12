using Assignment1.Data;
using Assignment1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Assignment1.Controllers
{
    [Authorize(Roles = "Supervisor,Employee")]
    public class MessagesController : Controller
    {
        
        public static byte[] AESKEYS;
        public static byte[] AESIV;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public MessagesController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: Messages - Both can view
        public async Task<IActionResult> Index()
        {
            var messages = await _context.Messages
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            foreach (var msg in messages)
            {
                if (!string.IsNullOrEmpty(msg.EncryptedContent))
                {
                    msg.DecryptedContent = Decrypt(msg.EncryptedContent);
                }
            }

            return View(messages);
        }

        // GET: Messages/Create - Supervisor only
        [Authorize(Roles = "Supervisor")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Messages/Create - Supervisor only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Create([Bind("Content")] Message message)
        {
            if (ModelState.IsValid)
            {
                message.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                message.UserName = User.Identity?.Name;
                message.CreatedAt = DateTime.Now;
                message.EncryptedContent = Encrypt(message.Content);

                _context.Add(message);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(message);
        }

        private Aes AesInitialize()
        {
            Aes aes = Aes.Create();
           
            var aesSettings = new AESSettings();

           
            if (aesSettings != null)
            {
                aes.Key = AESKEYS;
                aes.IV = AESIV;
            }

            return aes;
        }

        private string Encrypt(string plainText)
        {
            Aes aes = AesInitialize();
            ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] input = Encoding.UTF8.GetBytes(plainText);
            byte[] output = encryptor.TransformFinalBlock(input, 0, input.Length);
            return Convert.ToBase64String(output);
        }

        private string Decrypt(string cipherText)
        {
            try
            {
                Aes aes = AesInitialize();
                ICryptoTransform decryptor = aes.CreateDecryptor();
                byte[] input = Convert.FromBase64String(cipherText);
                byte[] output = decryptor.TransformFinalBlock(input, 0, input.Length);
                return Encoding.UTF8.GetString(output);
            }
            catch
            {
                return cipherText;
            }
        }
    }
}