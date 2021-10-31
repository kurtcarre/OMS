using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Models;
using OMS.Data;
using OMS.AuthZ;
using OMS.AuthZ.Models;

namespace OMS.Controllers
{
    [PermissionType(PermissionType.MailingListEntries)]
    public class MailingListController : Controller
    {
        private readonly DBContext Context;

        public MailingListController(DBContext _context)
        {
            Context = _context;
        }

        [RequirePermission(Permission.Read)]
        public async Task<IActionResult> Index()
        {
            return View(await Context.MailingListEntries.ToListAsync());
        }

        [RequirePermission(Permission.Create)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.Create)]
        public async Task<IActionResult> Create(MailingListEntry entry)
        {
            if (ModelState.IsValid)
            {
                await Context.AddAsync(entry);
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

            MailingListEntry entry = await Context.FindAsync<MailingListEntry>(Id);
            if (entry == null)
                return NotFound();

            return View(entry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.Write)]
        public async Task<IActionResult> Edit(int? Id, MailingListEntry entry)
        {
            if (Id == null)
                return NotFound();

            Context.Update(entry);
            await Context.SaveChangesAsync();
            return View(entry);
        }

        [RequirePermission(Permission.Full)]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null)
                return NotFound();

            MailingListEntry entry = await Context.FindAsync<MailingListEntry>(Id);
            Context.Remove(entry);
            await Context.SaveChangesAsync();
            return RedirectToAction("Index", "MailingList");
        }
    }
}