using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ApiOfficeAttendance.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace ApiOfficeAttendance.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OfficeAttendanceController : ControllerBase
    { 
        private readonly IAvailabilityRepository _availabilityRepository;

        private readonly Gauge _attendance = Metrics.CreateGauge("officeattendance_attendance_total", "The amount of people attending an office", "organization");
        
        public OfficeAttendanceController(IAvailabilityRepository availabilityRepository)
        { 
            _availabilityRepository = availabilityRepository; 
        }

        [HttpGet]
        [Authorize(PolicyPermissions.ReadAttendance)]
        public async Task<AppData> Get()
        {
            var nameOfOrganisation = "test";
            var savedItems = await GetInOfficeAvailability(nameOfOrganisation);
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
            var nameOfOrganisation = "test";
            var nameOfPerson = User.Claims.First(x => x.Type == "http://officeattendan.ce/name").Value;
            var theDay = await GetInOfficeAvailableOrCreate(mutation.DateAsString, nameOfOrganisation, nameOfPerson);  

            await _availabilityRepository.Add(nameOfOrganisation, theDay);
            var savedItems = await GetInOfficeAvailability(nameOfOrganisation);
            _attendance.Labels(nameOfOrganisation).Set(savedItems.Count());
        }

        [Route("remove")]
        [HttpPost]
        [Authorize(PolicyPermissions.WriteOwnAttendance)]
        public async Task Remove([FromBody] OfficeAvailableMutation mutation)
        {
            var nameOfOrganisation = "test";
            var nameOfPerson = User.Claims.First(x => x.Type == "http://officeattendan.ce/name").Value;
            var theDay = await GetInOfficeAvailableOrCreate(mutation.DateAsString, nameOfOrganisation, nameOfPerson); 

            await _availabilityRepository.Remove("test",theDay);
            var savedItems = await GetInOfficeAvailability(nameOfOrganisation);
            _attendance.Set(savedItems.Count());
            _attendance.Labels(nameOfOrganisation).Set(savedItems.Count());
        }

        private async Task<PersonInOffice> GetInOfficeAvailableOrCreate(string dateAsString, string organisation, string person)
        { 
            var selectedDate = DateTime.ParseExact(dateAsString, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            var items = await GetInOfficeAvailability(organisation);
            var theDay = items.FirstOrDefault(x => x.Date == selectedDate.Date && x.Person == person);
            if (theDay == null)
            {
                theDay = new PersonInOffice(selectedDate, person) {ETag = "*"};
            }

            return theDay;
        }

        private async Task<List<PersonInOffice>> GetInOfficeAvailability(string organisation)
        {
            var items = new List<PersonInOffice>();
            await foreach (var item in _availabilityRepository.Find(DateTime.Now, organisation, 14))
            {
                items.Add(item);
            }

            return items;
        }
    }
}
