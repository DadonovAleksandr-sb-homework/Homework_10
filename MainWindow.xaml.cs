using System;
using System.Diagnostics;
using System.IO;
using System.Windows;


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
                    // TODO: удалить, общий чат не нужен. Даешь чат для каждого пользователя!
                    //LogList.ItemsSource = bot.BotMessageLog;
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
            if(bot != null)
                bot.SendMessage(txtMsgSend.Text);
        }
    }
}
