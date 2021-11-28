﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Homework_10
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MyTelegramBot bot;

        public MainWindow()
        {
            InitializeComponent();
            StartBot();
        }

        private void StartBot()
        {
            try
            {
                var tokenFilePath = "token.txt";
                if (File.Exists(tokenFilePath))
                {
                    bot = new MyTelegramBot(this, File.ReadAllText(tokenFilePath));         // своя обертка для телеграм-клиента
                    if (bot.Start())
                        Debug.WriteLine($"Запуск бота {bot.Name}");
                    UserList.ItemsSource = bot.UserList;
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }

            Console.ReadLine();
        }

        private void btnMsgSendClick(object sender, RoutedEventArgs e)
        {
            if (bot != null && bot.UserList.Count > 0)
                bot.SendMessage(txtMsgSend.Text);
            txtMsgSend.Clear();
        }

        private void UserList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(sender is ListBox lb)
            {
                if(lb.SelectedItem is TelegramUser selectionUser)
                {
                    selectionUser.UnreadCount = 0;
                    foreach(var user in bot.UserList)
                    {
                        user.IsActive = false;
                    }
                    selectionUser.IsActive = true;
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if(sender is MenuItem menuItem)
            {
                switch(menuItem.Tag)
                {
                    case "Import":
                        MessageBox.Show("Import");
                        break;
                    case "Exit":
                        Environment.Exit(0);
                        break;
                }
            }
        }
    }
}
