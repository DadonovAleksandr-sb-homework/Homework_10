using System;

namespace Homework_10
{
    public class MessageLog
    {
        public string Time { get; set; }

        public long Id { get; set; }

        public string Msg { get; set; }

        public string Name { get; set; }

        public MessageLog(string Time, string Msg, string FirstName, long Id)
        {
            this.Time = DateTime.Now.ToLongTimeString();
            this.Msg = Msg;
            this.Name = FirstName;
            this.Id = Id;
        }
    }
}
