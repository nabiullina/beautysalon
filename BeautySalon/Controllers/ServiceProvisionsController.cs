using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeautySalon.Data.Models;
using SQLitePCL;

namespace BeautySalon.Controllers
{
    public class ServiceProvisionsController : Controller
    {
        private readonly BeautysalonContext _context;

        public ServiceProvisionsController(BeautysalonContext context)
        {
            _context = context;
        }

        // GET: ServiceProvisions
        public async Task<IActionResult> Index()
        {
            var beautysalonContext = _context.Serviceprovisions.Include(s => s.Cli).Include(s => s.Sch).Include(s => s.Ser);
            return View(await beautysalonContext.ToListAsync());
        }

        // GET: ServiceProvisions/Create
        [Route ("ServiceProvisions/Create/{cliid}")]
        public async Task<IActionResult> Create(long? cliid)
        {
            var client = await _context.Clients.FindAsync(cliid);
            ViewData["Serid"] = new SelectList(_context.Services, "Id", "Name");
            ViewData["Schid"] = new SelectList(_context.Schedules.Where(sch=>sch.Status=='-'), "Id", "Date");
            return View(new Serviceprovision {Cli = client});
        }

        // POST: ServiceProvisions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route ("ServiceProvisions/Create/{cliid}")]
        public async Task<IActionResult> Create(long cliid, [Bind("Serid, Schid")] Serviceprovision serviceprovision)
        {
            serviceprovision.Cliid = cliid;
            serviceprovision.Sch = await _context.Schedules.FindAsync(serviceprovision.Schid);
            serviceprovision.Sch.Status = '+';
            
            // if (ModelState.IsValid)
            // {
            _context.Update(serviceprovision.Sch);
                _context.Add(serviceprovision);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            // }
            // return View(serviceprovision);
        }

        // GET: ServiceProvisions/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.Serviceprovisions == null)
            {
                return NotFound();
            }

            var serviceprovision = await _context.Serviceprovisions.FindAsync(id);
            if (serviceprovision == null)
            {
                return NotFound();
            }
            ViewData["Cliid"] = new SelectList(_context.Clients, "Id", "Id", serviceprovision.Cliid);
            ViewData["Schid"] = new SelectList(_context.Schedules, "Id", "Id", serviceprovision.Schid);
            ViewData["Serid"] = new SelectList(_context.Services, "Id", "Id", serviceprovision.Serid);
            return View(serviceprovision);
        }

        // POST: ServiceProvisions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Cliid,Serid,Schid")] Serviceprovision serviceprovision)
        {
            if (id != serviceprovision.Cliid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(serviceprovision);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceprovisionExists(serviceprovision.Cliid))
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
            ViewData["Cliid"] = new SelectList(_context.Clients, "Id", "Id", serviceprovision.Cliid);
            ViewData["Schid"] = new SelectList(_context.Schedules, "Id", "Id", serviceprovision.Schid);
            ViewData["Serid"] = new SelectList(_context.Services, "Id", "Id", serviceprovision.Serid);
            return View(serviceprovision);
        }

        // GET: ServiceProvisions/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.Serviceprovisions == null)
            {
                return NotFound();
            }

            var serviceprovision = await _context.Serviceprovisions
                .Include(s => s.Cli)
                .Include(s => s.Sch)
                .Include(s => s.Ser)
                .FirstOrDefaultAsync(m => m.Cliid == id);
            if (serviceprovision == null)
            {
                return NotFound();
            }

            return View(serviceprovision);
        }

        // POST: ServiceProvisions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.Serviceprovisions == null)
            {
                return Problem("Entity set 'BeautysalonContext.Serviceprovisions'  is null.");
            }
            var serviceprovision = await _context.Serviceprovisions.FindAsync(id);
            if (serviceprovision != null)
            {
                _context.Serviceprovisions.Remove(serviceprovision);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceprovisionExists(long id)
        {
          return (_context.Serviceprovisions?.Any(e => e.Cliid == id)).GetValueOrDefault();
        }
        
        
    }
}
