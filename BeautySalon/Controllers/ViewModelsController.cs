using Microsoft.AspNetCore.Mvc;
using BeautySalon.Data.Models;
using Microsoft.EntityFrameworkCore;

using Controller = Microsoft.AspNetCore.Mvc.Controller;
using ModelState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateEntry;
using SelectList = Microsoft.AspNetCore.Mvc.Rendering.SelectList;

namespace BeautySalon.Controllers;

public class ViewModelsController : Controller
{
    private readonly BeautysalonContext _context;

    
    public ViewModelsController(BeautysalonContext context)
    {
        _context = context;
    }
    
    public IActionResult Create()
    {
        ViewData["Pos"] = new SelectList(_context.Positions, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Employee", "EmployeesOnPosition")] ViewModel model)
    {
        _context.Add(model.Employee);
        await _context.SaveChangesAsync();
        model.EmployeesOnPosition.Empid = model.Employee.Id;
        _context.Add(model.EmployeesOnPosition);
        await _context.SaveChangesAsync();
        return Redirect("/Employees");
    }

    public async Task<IActionResult> AddPos(long id)
    {
        var employees = _context.Employees.Include("EmployeesOnPositions.Pos").Where(emp=>emp.Id==id);
        var emp = await employees.FirstAsync();

        var takenPos = _context.EmployeesOnPositions.Where(emponpos=>emponpos.Empid==id);
        IEnumerable<Position> poses = _context.Positions;
        foreach (var pos in takenPos)
        {
            if (emp.EmployeesOnPositions.Contains(pos))
            {
                poses = poses.Where(p => p.Id != pos.Posid);
            }
        }
        ViewData["Pos"] = new SelectList(poses, "Id", "Name"); // await _context.Positions.Where(p=> _context.EmployeesOnPositions.FindAsync(p.Id) = false)
        var viewmodel = new ViewModel
        {
            Employee = emp
        };
        return View(viewmodel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPos(long id, [Bind("Employee, EmployeesOnPosition")] ViewModel model)
    {
        model.EmployeesOnPosition.Empid = id;
        _context.Add(model.EmployeesOnPosition);
        await _context.SaveChangesAsync();
        return Redirect("/Employees/Edit/"+id);
    }
    
    [Route ("ViewModels/DelPos/{empid?}/{posid}")]
    public async Task<IActionResult> DelPos(long empid, long posid)
    {
        var employees = _context.Employees.Include("EmployeesOnPositions.Pos").Where(emp=>emp.Id==empid);
        var emp = await employees.FirstAsync();
        ViewData["Pos"] = new SelectList(_context.Positions, "Id", "Name");
        var viewmodel = new ViewModel
        {
            Employee = emp,
            EmployeesOnPosition = new EmployeesOnPosition {Empid = emp.Id, Posid = posid, Pos = await _context.Positions.FindAsync(posid)}
        };
        return View(viewmodel);
    }
    
    [HttpPost, ActionName("DelPos")]
    [ValidateAntiForgeryToken]
    [Route ("ViewModels/DelPos/{empid?}/{posid}")]
    public async Task<IActionResult> DelPosConfirmed(long empid, long posid)
    {
        var emponpos = await _context.EmployeesOnPositions.FindAsync(empid, posid);
        _context.EmployeesOnPositions.Remove(emponpos);
        await _context.SaveChangesAsync();
        return Redirect($"/ViewModels/AddPos/{empid}");
    }

    [Route ("ViewModels/ViewSchedule/{empid}")]
    public async Task<IActionResult> ViewSchedule(long empid, DateTime? date)
    {
        var viewmodel = new ViewModel
        {
            Employee = await _context.Employees
                .Include("Schedules.Serviceprovision.Cli")
                .Include("Schedules.Serviceprovision.Ser")
                .Where(emp => emp.Id == empid)
                .FirstOrDefaultAsync()
        };
        if (date.HasValue)
        {
            DateTime startDate = new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 0, 0, 0);
            DateTime endDate = new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 23, 59, 59);
            viewmodel.Employee.Schedules = viewmodel.Employee.Schedules.Where(s => s.Date > startDate && s.Date < endDate).ToList();
        }
        viewmodel.Employee.Schedules = viewmodel.Employee.Schedules.OrderBy(sch => sch.Date).ToList();
        return View(viewmodel);
    }
    
    [Route("ViewModels/ClientServices/{cliid?}")]
    public async Task<IActionResult> ClientServices(long cliid)
    {
        var viewmodel = new ViewModel
        {
            Client = await _context.Clients
                .Include("Serviceprovisions.Sch.Emp")
                .Include("Serviceprovisions.Ser")
                .Include("Serviceprovisions")
                .Where(cli => cli.Id == cliid)
                .FirstOrDefaultAsync()
        };
        return View(viewmodel);
    }

    [Route("ViewModels/SelectService/{cliid?}")]
    public async Task<IActionResult> SelectService(long cliid)
    {
        var services = await _context.Services.ToListAsync();
        var model = new ViewModel {Client = await _context.Clients.FindAsync(cliid)};
        ViewData["serid"] = new SelectList(services, "Id", "Name");
        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("ViewModels/SelectService/{cliid?}")]
    public async Task<IActionResult> SelectService(long cliid, [Bind("Serviceprovision")] ViewModel model)
    {
        return Redirect($"/ViewModels/SelectTime/{cliid}/{model.Serviceprovision.Serid}");
    }

    [Route("ViewModels/SelectTime/{cliid?}/{serid?}")]
    public async Task<IActionResult> SelectTime(long cliid, long serid)
    {
        var service = await _context.Services.FindAsync(serid);
        // var position = await _context.Positions.FindAsync(service.Posid);
        var emponpos = await _context.EmployeesOnPositions.Where(ep=>ep.Posid == service.Posid).ToListAsync();
        var employees = new List<Employee>();
        foreach (var item in emponpos)
        {
            var emp = await _context.Employees.Include("EmployeesOnPositions")
                .Where(e => e.EmployeesOnPositions.Contains(item)).FirstOrDefaultAsync();
            employees.Add(emp);
        }
        // var employees = await _context.Employees.Where(e =>
            // e.EmployeesOnPositions.Contains()).ToListAsync();
    
    /*new KeyValuePair<string, long> {Key = "Empid", } <"Empid", e.Id> KeyValuePair<"Posid", service.Posid>*/
        // var schedules = await _context.Schedules.Where(sch=>sch.Emp)
        var schedules = await _context.Schedules.Include("Emp").Where(sch => employees.Contains(sch.Emp) && sch.Status=='-').ToListAsync();
        ViewData["schid"] = new SelectList(schedules, "Id", "Date");
        return View(new ViewModel {Client = await _context.Clients.FindAsync(cliid), Service = await _context.Services.FindAsync(serid)});
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("ViewModels/SelectTime/{cliid?}/{serid?}")]
    public async Task<IActionResult> SelectTime(long cliid, long serid, [Bind("Serviceprovision")] ViewModel model)
    {
        model.Serviceprovision.Cliid = cliid;
        model.Serviceprovision.Serid = serid;
        model.Schedule = await _context.Schedules.FindAsync(model.Serviceprovision.Schid);
        model.Schedule.Status = '+';
        _context.Update(model.Schedule);


        _context.Add(model.Serviceprovision);
        _context.SaveChanges();
        return Redirect($"/ViewModels/ClientServices/{cliid}");
    }
    // [Route("ViewModels/AddSerProv/{cliid?}/{serid}")]
    // 
    // public async Task<IActionResult> AddSerProv(long cliid, [Bind("")])
    // {
    //     
    //     return View();
    // }
    //
    // [HttpPost, ActionName("AddSerProv")]
    // [Route("ViewModels/AddSerProv/{cliid?}/{serid}")]
    // public async Task<IActionResult> AddSerProvCli(long cliid, long serid)
    // {
    //     var viewModels = new List<ViewModel>();
    //     var clients = await _context.Clients.ToListAsync();
    //     foreach (var client in clients)
    //     {
    //         viewModels.Add(new ViewModel {Client = client});
    //     }
    //
    //     return View(viewModels);
    // }
    
}