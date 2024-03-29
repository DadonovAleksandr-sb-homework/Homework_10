﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Homework_10
{
    public class TelegramUser : INotifyPropertyChanged
    {
        /// <summary>
        /// Ник пользователя
        /// </summary>
        public string Nick { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Идентификатор чата
        /// </summary>
        public long ChatId { get; set; }

        /// <summary>
        /// Все сообщения от пользователя
        /// </summary>
        public ObservableCollection<TelegramMessage> Messages { get; }

        /// <summary>
        /// Последнее сообщение в чате пользователя
        /// </summary>
        public string LastMessage => Messages.LastOrDefault()?.Text;

        /// <summary>
        /// Кол-во сообщений в чате
        /// </summary>
        public int MessageCount => Messages.Count;

        private int _unreadCount;
        /// <summary>
        /// Кол-во непрочитанных сообщений
        /// </summary>
        public int UnreadCount
        {
            get
            {
                return _unreadCount;
            }
            set
            {
                _unreadCount = value;
                if (_unreadCount > 0)
                    IsUnreadMsg = Visibility.Visible;
                else
                    IsUnreadMsg = Visibility.Hidden;

                OnPropertyChanged(nameof(UnreadCount));
                OnPropertyChanged(nameof(IsUnreadMsg));
            }
        }

        /// <summary>
        /// Признак активности - сейчас открыт чат этого пользователя
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Есть непрочитанные сообщения
        /// </summary>
        public Visibility IsUnreadMsg { get; set; }

        public TelegramUser(string nick, string name, long chatId)
        {
            Nick = nick;
            Name = name;
            ChatId = chatId;
            Messages = new ObservableCollection<TelegramMessage>();
        }

        
        /// <summary>
        /// Сравнение двух пользователей
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(TelegramUser other) => other.ChatId == this.ChatId;

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if(obj is TelegramUser user)
            {
                return Equals(user);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (int)ChatId;
        }

        /// <summary>
        /// Добавление сообщения
        /// </summary>
        /// <param name="text"></param>
        public void AddMessage(TelegramMessage msg)
        {
            Messages.Add(msg);
            if(!IsActive)
                UnreadCount++;
            
            OnPropertyChanged(nameof(LastMessage));
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }

}
