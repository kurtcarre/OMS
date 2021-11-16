using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Models;
using OMS.Data;
using OMS.AuthZ;
using OMS.AuthZ.Models;

namespace OMS.Controllers
{
    [PermissionType(PermissionType.ChildMembers)]
    public class ChildMemberController : Controller
    {
        private readonly DBContext Context;

        public ChildMemberController(DBContext _context)
        {
            Context = _context;
        }

        [RequirePermission(Permission.Read)]
        public async Task<IActionResult> Index()
        {
            IQueryable<ChildMember> children = Context.ChildMembers.Include(m => m.Member).AsNoTracking();
            return View(await children.ToListAsync());
        }

        [RequirePermission(Permission.Create)]
        public async Task<IActionResult> Create(int memberNo)
        {
            ChildMember template = new ChildMember
            {
                MemberNo = memberNo,
                Member = await Context.Members.FirstOrDefaultAsync(m => m.MemberNo == memberNo)
            };
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.Create)]
        public async Task<IActionResult> Create(ChildMember childMember)
        {
            if(ModelState.IsValid)
            {
                Context.ChildMembers.Add(childMember);
                await Context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        [RequirePermission(Permission.Write)]
        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id == null)
                return NotFound();

            ChildMember child = await Context.ChildMembers.FirstOrDefaultAsync(mem => mem.MemberNo == Id);
            child.Member = await Context.Members.FirstOrDefaultAsync(mem => mem.MemberNo == Id);
            if (child == null)
                return NotFound();

            return View(child);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.Write)]
        public async Task<IActionResult> Edit(int? Id, ChildMember childMember)
        {
            if (Id == null)
                return NotFound();

            if(ModelState.IsValid)
            {
                Context.ChildMembers.Update(childMember);
                await Context.SaveChangesAsync();
            }

            childMember.Member = await Context.Members.FirstOrDefaultAsync(mem => mem.MemberNo == Id);
            return View(childMember);
        }
    }
}