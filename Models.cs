using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.Azure.Cosmos.Table;

namespace ApiOfficeAttendance
{
    public class PersonInOffice : TableEntity
    {
        public PersonInOffice()
        {

        }

        public PersonInOffice(DateTime date,  string person)
        {
            RowKey = date.Ticks.ToString();
            PartitionKey = person; 
        }
         
        public DateTime Date => new DateTime(long.Parse(RowKey));
        public string Person => PartitionKey;
    }

    public class InOfficeAvailable
    {
        public InOfficeAvailable()
        {

        }

        public InOfficeAvailable(DateTime date, string organization = "test")
        {
            Date = date;
            Organization = organization;
            Persons = new List<string>();
        }

        [JsonIgnore] 
        public DateTime Date { get; }

        [JsonIgnore]
        public string Organization { get; }

        [JsonPropertyName("date")] 
        public string DateAsText => Date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

        public List<string> Persons { get; set; }
 
    }

    public class AppData
    {
        public int CurrentDayIndex { get; set; }

        public List<InOfficeAvailable> OfficeAvailability { get; set; }
    }

    public class OfficeAvailableMutation
    {
        public string DateAsString { get; set; }
    }
}
  