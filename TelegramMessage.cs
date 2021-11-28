using System;
using System.Windows;
using System.Windows.Media;

namespace Homework_10
{
    public class TelegramMessage
    {
        public string Time { get; set; }

        public long Id { get; set; }

        public string Text { get; set; }

        public string Name { get; set; }

        public bool IsBotMsg { get; set; }

        public TelegramMessage(string text, string userName, long id, bool isBotMsg = false)
        {
            this.Time = DateTime.Now.ToLongTimeString();
            this.Text = text;
            this.Name = userName;      
            this.IsBotMsg = isBotMsg;
        }
    }
}
