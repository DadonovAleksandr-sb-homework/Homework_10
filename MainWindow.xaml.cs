using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
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

        /// <summary>
        /// Запуск бота
        /// </summary>
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

        /// <summary>
        /// Отправка сообщений
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMsgSendClick(object sender, RoutedEventArgs e)
        {
            if (bot != null && bot.IsActiveChat)
                bot.SendMessage(txtMsgSend.Text);
            txtMsgSend.Clear();
        }

        /// <summary>
        /// Выбор текущего корреспондента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    bot.SetCurUser(selectionUser);
                }
            }
        }

        /// <summary>
        /// Команды меню
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if(sender is MenuItem menuItem)
            {
                switch(menuItem.Tag)
                {
                    case "Import":
                        ImportToJson();
                        break;
                    case "Export":
                        ExportFromJson();
                        break;
                    case "Exit":
                        Environment.Exit(0);
                        break;
                }
            }
        }

        /// <summary>
        /// Импорт текущего списка корреспондентов и журналов сообщений (по каждому пользователю) в JSON файл
        /// </summary>
        private void ImportToJson()
        {
            string userData = JsonConvert.SerializeObject(bot.UserList);
            try
            {
                File.WriteAllText("user_data.json", userData);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        /// <summary>
        /// Экспорт текущего списка корреспондентов и журналов сообщений (по каждому пользователю) из JSON файл
        /// </summary>
        private void ExportFromJson()
        {
            try
            {
                string userData = File.ReadAllText("user_data.json");
                if(JsonConvert.DeserializeObject<ObservableCollection<TelegramUser>>(userData) is ObservableCollection<TelegramUser> userList)
                {
                    if (userList is null)
                        return;
                    bot.UserList.Clear();
                    foreach(var user in userList)
                    {
                        bot.UserList.Add(user);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}
