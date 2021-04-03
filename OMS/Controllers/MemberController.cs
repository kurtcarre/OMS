using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Models;
using OMS.Data;

namespace OMS.Controllers
{
    public class MemberController : Controller
    {
        private readonly DBContext Context;

        public MemberController(DBContext _context)
        {
            Context = _context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await Context.Members.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Member newMember)
        {
            if(ModelState.IsValid)
            {
                Context.Add(newMember);
                await Context.SaveChangesAsync();
                if (newMember.Under18)
                    return RedirectToAction("Create", "ChildMember", new { memberNo = newMember.MemberNo });
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id == null)
                return NotFound();

            Member member = await Context.Members.FirstOrDefaultAsync(m => m.MemberNo == Id);
            if (member == null)
                return NotFound();

            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? Id, Member member)
        {
            if (Id == null)
                return NotFound();

            try
            {
                Context.Update(member);
                await Context.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception e)
            {
                ViewData["ErrorMessage"] = e.Message;
            }
            return View(member);
        }
    }
}