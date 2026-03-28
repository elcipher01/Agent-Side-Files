using Microsoft.AspNetCore.Mvc;
using NextHorizon.Models;
using NextHorizon.Filters;
using Microsoft.EntityFrameworkCore;

namespace NextHorizon.Controllers
{
    [AuthenticationFilter]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // Admin Dashboard
        public IActionResult Dashboard()
        {
            // Get admin name from session
            var adminName = HttpContext.Session.GetString("FullName") ?? "Admin";
            var userType = HttpContext.Session.GetString("UserType") ?? "Admin";
            
            ViewData["Title"] = "Admin Dashboard";
            ViewData["AdminName"] = adminName;
            ViewData["UserType"] = userType;
            
            return View();
        }

        // Admin can view all FAQs
        public async Task<IActionResult> FAQs()
        {
            var faqs = await _context.FAQs.ToListAsync();
            return View(faqs);
        }

        // Admin can create FAQs
        public IActionResult CreateFAQ()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateFAQ(FAQ faq)
        {
            if (ModelState.IsValid)
            {
                faq.DateAdded = DateTime.Now;
                faq.LastUpdated = DateTime.Now;
                _context.FAQs.Add(faq);
                await _context.SaveChangesAsync();
                return RedirectToAction("FAQs");
            }
            return View(faq);
        }

        // Admin can edit FAQs
        public async Task<IActionResult> EditFAQ(int id)
        {
            var faq = await _context.FAQs.FindAsync(id);
            if (faq == null) return NotFound();
            return View(faq);
        }

        [HttpPost]
        public async Task<IActionResult> EditFAQ(int id, FAQ faq)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    faq.LastUpdated = DateTime.Now;
                    _context.Update(faq);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("FAQs");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.FAQs.Any(e => e.FaqID == faq.FaqID))
                        return NotFound();
                    throw;
                }
            }
            return View(faq);
        }

        // Admin can delete FAQs
        [HttpPost]
        public async Task<IActionResult> DeleteFAQ(int id)
        {
            var faq = await _context.FAQs.FindAsync(id);
            if (faq == null) return NotFound();

            _context.FAQs.Remove(faq);
            await _context.SaveChangesAsync();
            return RedirectToAction("FAQs");
        }
    }
}
