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
                    var bot = new MyTelegramBot(File.ReadAllText(tokenFilePath));         // своя обертка для телеграм-клиента
                    if (bot.Start())
                        Debug.WriteLine($"Запуск бота {bot.Name}");
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
    }
}
