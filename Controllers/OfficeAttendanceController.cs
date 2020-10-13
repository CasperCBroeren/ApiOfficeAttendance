using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ApiOfficeAttendance.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OfficeAttendanceController : ControllerBase
    {
        
        private readonly ILogger<OfficeAttendanceController> _logger;

        public OfficeAttendanceController(ILogger<OfficeAttendanceController> logger)
        {
            _logger = logger;
        }

        [HttpGet] 
        [Authorize(PolicyPermissions.ReadAttendance)]
        public AppData Get()
        { 
            return new AppData()
            {
                CurrentDayIndex = 0,
                OfficeAvailability = new List<InOfficeAvailable>()
                {
                    new InOfficeAvailable()
                    {
                        Date = DateTime.Now,
                        Persons = new List<string> {"Casper Broeren", "Operah Winfrey", "Matt le Blanc", "Mohamed Ali"}
                    },
                    new InOfficeAvailable()
                    {
                        Date = DateTime.Now.AddDays(1),
                        Persons = new List<string> {  "Matt le Blanc", "Mohamed Ali"}
                    },
                    new InOfficeAvailable()
                    {
                        Date = DateTime.Now.AddDays(2),
                        Persons = new List<string> {"Casper Broeren", "Operah Winfrey", "Matt le Blanc", "Mohamed Ali"}
                    },
                    new InOfficeAvailable()
                    {
                        Date = DateTime.Now.AddDays(3),
                        Persons = new List<string> {"Casper Broeren", "Operah Winfrey", "Matt le Blanc", "Mohamed Ali"}
                    }

                }
            };
        }
    }
}
