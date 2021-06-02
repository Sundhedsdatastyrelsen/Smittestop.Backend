using System;

namespace DIGNDB.App.SmitteStop.Domain.Db
{
    public class LoginInformation
    {
        public long Id { get; set; }
        public int Negative { get; set; }
        public int Blocked { get; set; }
        public int Positive { get; set; }
        public int Error { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return String.Format($"Positive: {Positive}; Negative: {Negative}; Blocked: {Blocked}; Error: {Error}; Date: {Timestamp.Date};");
        }
    }
}
