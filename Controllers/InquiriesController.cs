using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebServer.Data;
using WebServer.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Xrm.Sdk.Query;

namespace WebServer.Controllers
{
    [Authorize]
    public class InquiriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrganizationService _service;
        private readonly UserManager<ApplicationUser> _userManager;

        public InquiriesController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
            _service = CRM.CrmService.GetServiceProvider();
        }

        // GET: Inquiries
        public async Task<IActionResult> Index()       
        {
            var inquiries = _context.Inquiry.Where(u => u.UserId == _userManager.GetUserId(User)).ToList();
            foreach (var inquiry in inquiries)
            {
                var crmInquiry = _service.Retrieve("new_inquiry", inquiry.InquiryId, new ColumnSet("new_response")); 
               
                inquiry.Response = crmInquiry.GetAttributeValue<string>("new_response");
            }

            return View(inquiries); 
        }

        // GET: Inquiries/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inquiry = await _context.Inquiry
                .SingleOrDefaultAsync(m => m.InquiryId == id);
            var crmInquiry = _service.Retrieve("new_inquiry", inquiry.InquiryId, new ColumnSet("new_response"));

            inquiry.Response = crmInquiry.GetAttributeValue<string>("new_response");

            if (inquiry == null)
            {
                return NotFound();
            }

            return View(inquiry);
        }

        // GET: Inquiries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Inquiries/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InquiryId,Question,Response,UserId")] Inquiry inquiry)
        {
            if (ModelState.IsValid)
            {
                inquiry.InquiryId = Guid.NewGuid();
                inquiry.UserId = _userManager.GetUserId(User);
                _context.Add(inquiry);
                await _context.SaveChangesAsync();

                Entity CrmInquiry = new Entity("new_inquiry");
                CrmInquiry["new_name"] = _userManager.GetUserName(User);
                CrmInquiry.Id = inquiry.InquiryId;
                CrmInquiry["new_question"] = inquiry.Question;
                CrmInquiry["new_contact"] = new EntityReference("contact", Guid.Parse(_userManager.GetUserId(User)));
                _service.Create(CrmInquiry);

                return RedirectToAction("Index");
            }
            return View(inquiry);
        }

        // GET: Inquiries/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inquiry = await _context.Inquiry.SingleOrDefaultAsync(m => m.InquiryId == id);
            if (inquiry == null)
            {
                return NotFound();
            }
            return View(inquiry);
        }

        // POST: Inquiries/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("InquiryId,Question,Response,UserId")] Inquiry inquiry)
        {
            if (id != inquiry.InquiryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inquiry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InquiryExists(inquiry.InquiryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(inquiry);
        }

        // GET: Inquiries/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inquiry = await _context.Inquiry
                .SingleOrDefaultAsync(m => m.InquiryId == id);
            if (inquiry == null)
            {
                return NotFound();
            }

            return View(inquiry);
        }

        // POST: Inquiries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var inquiry = await _context.Inquiry.SingleOrDefaultAsync(m => m.InquiryId == id);
            _context.Inquiry.Remove(inquiry);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool InquiryExists(Guid id)
        {
            return _context.Inquiry.Any(e => e.InquiryId == id);
        }
    }
}
