using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework_10
{
    public class TelegramUser
    {
        /// <summary>
        /// Ник пользователя
        /// </summary>
        public string Nick { get; set; }

        /// <summary>
        /// Идентификатор чата
        /// </summary>
        public long ChatId { get; set; }

        /// <summary>
        /// Все сообщения от пользователя
        /// </summary>
        public List<string> Messages { get; set; }

        public TelegramUser(string nickName, long chatId)
        {
            Nick = nickName;
            ChatId = chatId;
            Messages = new List<string>();
        }

        /// <summary>
        /// Сравнение двух пользователей
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(TelegramUser other) => other.ChatId == this.ChatId;

        /// <summary>
        /// Добавление сообщения
        /// </summary>
        /// <param name="text"></param>
        public void AddMessage(string text) => Messages.Add(text);

    }

}
