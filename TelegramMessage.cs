using System;

namespace Homework_10
{
    public class TelegramMessage
    {
        public string Time { get; set; }

        public long Id { get; set; }

        public string Text { get; set; }

        public string Name { get; set; }

        public TelegramMessage(string text, string userName, long id)
        {
            this.Time = DateTime.Now.ToLongTimeString();
            this.Text = text;
            this.Name = userName;       // TODO: убрать, не нужно хранить в каждом сообщении информаию, хранящуюся в User
            this.Id = id;               // TODO: убрать, не нужно хранить в каждом сообщении информаию, хранящуюся в User
        }
    }
}
