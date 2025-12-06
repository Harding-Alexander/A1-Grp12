using Assignment1.Data;
using Assignment1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Assignment1.Controllers
{

    [Authorize(Roles = "Supervisor,Employee")]
    public class EmployeesController : Controller
    {
        public static byte[] AESKEYS;
        public static byte[] AESIV;

        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public EmployeesController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: Employees - Both can view
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employee.ToListAsync();
            foreach (var emp in employees)
            {
                if (!string.IsNullOrEmpty(emp.EncryptedSalary))
                {
                    emp.DecryptedSalary = Decrypt(emp.EncryptedSalary);
                }
            }
            return View(employees);
        }

        // GET: Employees/Details/5 - Both can view
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(employee.EncryptedSalary))
            {
                employee.DecryptedSalary = Decrypt(employee.EncryptedSalary);
            }

            return View(employee);
        }

        // GET: Employees/Create - Supervisor only
        [Authorize(Roles = "Supervisor")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create - Supervisor only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Create([Bind("EmployeeId,FirstName,LastName,Position,Salary,HireDate,CompanyName")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                employee.EncryptedSalary = Encrypt(employee.Salary.ToString());
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: Employees/Edit/5 - Supervisor only
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(employee.EncryptedSalary))
            {
                employee.DecryptedSalary = Decrypt(employee.EncryptedSalary);
                if (decimal.TryParse(employee.DecryptedSalary, out decimal salary))
                {
                    employee.Salary = salary;
                }
            }

            return View(employee);
        }

        // POST: Employees/Edit/5 - Supervisor only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeId,FirstName,LastName,Position,Salary,HireDate,CompanyName")] Employee employee)
        {
            if (id != employee.EmployeeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    employee.EncryptedSalary = Encrypt(employee.Salary.ToString());
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: Employees/Delete/5 - Supervisor only
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5 - Supervisor only
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employee.FindAsync(id);
            if (employee != null)
            {
                _context.Employee.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employee.Any(e => e.EmployeeId == id);
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