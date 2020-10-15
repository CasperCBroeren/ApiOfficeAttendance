using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ApiOfficeAttendance.Repository;
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
        private readonly IAvailabilityRepository _availabilityRepository;

        public OfficeAttendanceController(ILogger<OfficeAttendanceController> logger,
            IAvailabilityRepository availabilityRepository)
        {
            _logger = logger;
            _availabilityRepository = availabilityRepository;
        }

        [HttpGet]
        [Authorize(PolicyPermissions.ReadAttendance)]
        public async Task<AppData> Get()
        { 
            var savedItems = await GetInOfficeAvailables();
            var items = new List<InOfficeAvailable>();

            for (var i = 0; i < 14; i++)
            {
                var currentDate = DateTime.Now.Date.AddDays(i);
                var inOfficeDay = new InOfficeAvailable(currentDate);
                foreach (var personInOffice in savedItems.FindAll(x=> x.Date == currentDate))
                {
                    inOfficeDay.Persons.Add(personInOffice.Person);
                }
                items.Add(inOfficeDay);
                
            }

            return new AppData()
            {
                CurrentDayIndex = 0,
                OfficeAvailability = items
            };
        }

        [Route("set")]
        [HttpPost]
        [Authorize(PolicyPermissions.WriteOwnAttendance)]
        public async Task Set([FromBody]OfficeAvailableMutation mutation)
        {
            var nameOfPerson = User.Claims.First(x => x.Type == "http://officeattendan.ce/name").Value;
            var theDay = await GetInOfficeAvailableOrCreate(mutation.DateAsString, nameOfPerson);  

            await _availabilityRepository.Add("test", theDay);
        }

        [Route("remove")]
        [HttpPost]
        [Authorize(PolicyPermissions.WriteOwnAttendance)]
        public async Task Remove([FromBody] OfficeAvailableMutation mutation)
        {
            var nameOfPerson = User.Claims.First(x => x.Type == "http://officeattendan.ce/name").Value;
            var theDay = await GetInOfficeAvailableOrCreate(mutation.DateAsString, nameOfPerson); 

            await _availabilityRepository.Remove("test",theDay);
        }

        private async Task<PersonInOffice> GetInOfficeAvailableOrCreate(string dateAsString, string person)
        {
            var selectedDate = DateTime.ParseExact(dateAsString, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            var items = await GetInOfficeAvailables();
            var theDay = items.FirstOrDefault(x => x.Date == selectedDate.Date && x.Person == person);
            if (theDay == null)
            {
                theDay = new PersonInOffice(selectedDate,person);
                theDay.ETag = "*";
            }

            return theDay;
        }

        private async Task<List<PersonInOffice>> GetInOfficeAvailables()
        {
            var items = new List<PersonInOffice>();
            await foreach (var item in _availabilityRepository.Find(DateTime.Now, "test", 14))
            {
                items.Add(item);
            }

            return items;
        }
    }
}
