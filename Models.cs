using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace ApiOfficeAttendance
{
    public class InOfficeAvailable
    {
        [JsonIgnore]
        public DateTime Date { get; set; }

        [JsonPropertyName("date")] 
        public string DateAsText => Date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

        public List<string> Persons { get; set; }
 
    }

    public class AppData
    {
        public int CurrentDayIndex { get; set; }

        public List<InOfficeAvailable> OfficeAvailability { get; set; }
    }
}
  