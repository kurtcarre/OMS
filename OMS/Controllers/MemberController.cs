using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Models;
using OMS.Data;
using OMS.AuthZ;
using OMS.AuthZ.Models;

namespace OMS.Controllers
{
    [PermissionType(PermissionType.Members)]
    public class MemberController : Controller
    {
        private readonly DBContext Context;

        public MemberController(DBContext _context)
        {
            Context = _context;
        }

        [RequirePermission(Permission.Read)]
        public async Task<IActionResult> Index()
        {
            return View(await Context.Members.ToListAsync());
        }

        [RequirePermission(Permission.Create)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.Create)]
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

        [RequirePermission(Permission.Write)]
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
        [RequirePermission(Permission.Write)]
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

        [RequirePermission(Permission.Full)]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null)
                return NotFound();

            Member member = await Context.Members.FirstOrDefaultAsync(m => m.MemberNo == Id);
            Context.Remove(member);

            await Context.SaveChangesAsync();

            return RedirectToAction("Index", "Member");
        }
    }
}