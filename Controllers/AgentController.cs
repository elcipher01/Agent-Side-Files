using Microsoft.AspNetCore.Mvc;
using NextHorizon.Models;
using NextHorizon.Filters;
using Microsoft.EntityFrameworkCore;

namespace NextHorizon.Controllers
{
    [AuthenticationFilter]
    public class AgentController : Controller
    {
        private readonly AppDbContext _context;

        public AgentController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult HelpCenter()
        {
            return View();
        }

        public async Task<IActionResult> FAQs()
        {
            var faqs = await _context.FAQs.ToListAsync();
            return View(faqs);
        }

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

        public async Task<IActionResult> EditFAQ(int id)
        {
            var faq = await _context.FAQs.FindAsync(id);
            if (faq == null) return NotFound();
            return View(faq);
        }

        [HttpPost]
        public async Task<IActionResult> EditFAQ(FAQ faq)
        {
            if (ModelState.IsValid)
            {
                faq.LastUpdated = DateTime.Now;
                _context.FAQs.Update(faq);
                await _context.SaveChangesAsync();
                return RedirectToAction("FAQs");
            }
            return View(faq);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFAQ(int id)
        {
            var faq = await _context.FAQs.FindAsync(id);
            if (faq != null)
            {
                _context.FAQs.Remove(faq);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("FAQs");
        }

        // ── AGENT STATUS ──────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetAgentStatus(string agentName)
        {
            var agent = await _context.Agents
                .Where(a => a.AgentName == agentName)
                .OrderByDescending(a => a.StartTime)
                .FirstOrDefaultAsync();

            var status = agent?.AgentStatus ?? "available";
            return Json(new { success = true, status = status });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAgentStatus([FromBody] UpdateAgentStatusRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.AgentName) || string.IsNullOrWhiteSpace(model.Status))
                return BadRequest(new { success = false, message = "Invalid request." });

            var agentRecord = await _context.Agents
                .Where(a => a.AgentName == model.AgentName)
                .OrderByDescending(a => a.StartTime)
                .FirstOrDefaultAsync();

            if (agentRecord != null)
            {
                agentRecord.AgentStatus = model.Status;
                await _context.SaveChangesAsync();
                return Json(new { success = true, status = model.Status });
            }

            // No record exists yet — create one
            var newRecord = new Agent
            {
                AgentName = model.AgentName,
                ClientName = "N/A",
                Category = "N/A",
                PreviewQuestion = "N/A",
                ChatSlot = 1,
                ChatStatus = "Active",
                AgentStatus = model.Status,
                StartTime = DateTime.Now
            };

            _context.Agents.Add(newRecord);
            await _context.SaveChangesAsync();
            return Json(new { success = true, status = model.Status });
        }
    }

    public class UpdateAgentStatusRequest
    {
        public string AgentName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}