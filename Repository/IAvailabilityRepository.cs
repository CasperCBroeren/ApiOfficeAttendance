using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiOfficeAttendance.Repository
{
    public interface IAvailabilityRepository
    {
        IAsyncEnumerable<PersonInOffice> Find(DateTime startDate, string organization, int maxDays);

        Task Add(string organization, PersonInOffice item);

        Task Remove(string organization, PersonInOffice item);
    }
}
