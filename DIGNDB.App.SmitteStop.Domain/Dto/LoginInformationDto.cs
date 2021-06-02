using System.Collections.Generic;

namespace DIGNDB.App.SmitteStop.Domain.Dto
{
    public class LoginInformationDto
    {
        public Tags tags { get; set; }
        public List<Event> events { get; set; }
    }
    
    public class Tags
    {
        public string source { get; set; }
    }

    public class Attributes
    {
        public string negativ { get; set; }
        public string blokeret { get; set; }
        public string positiv { get; set; }
        public string fejl { get; set; }
        public string timestamp { get; set; }
    }

    public class Event
    {
        public Attributes attributes { get; set; }
        public string timestamp { get; set; }
    }
}
