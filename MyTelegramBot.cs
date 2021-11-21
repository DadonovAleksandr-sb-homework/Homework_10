using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Homework_10
{
    public class MyTelegramBot
    {
        private MainWindow window;
        public ObservableCollection<TelegramUser> UserList { get; set; }

        private TelegramBotClient _client;
        private string _name;
        private List<BotFile> _files;
        private enum BotCmd { ShowFiles }
        private const string RootPath = "BotData";
        private const int MaxFileName = 60;

        private TelegramUser curUser;

        public string Name { get { return _name; } }

        /// <summary>
        /// Инициализация клиента
        /// </summary>
        /// <param name="token"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public MyTelegramBot(MainWindow w, string token)
        {
            UserList = new ObservableCollection<TelegramUser>();
            window = w;
            if (string.IsNullOrEmpty(token))                        // если вместо токена передали пустую строку, 
                throw new ArgumentNullException(nameof(token));     // генерируем исключение

            _client = new TelegramBotClient(token);                 // инициализация клиента
            var me = _client.GetMeAsync().Result;               // получаем информацию о боте

            if (!(me is null) && !string.IsNullOrEmpty(me.Username))
                _name = me.Username;
            else
                throw new Exception("Не удалось получить информацию о боте!");
        }

        internal async void SendMessage(string message)
        {
            SendText(message);
        }

        /// <summary>
        /// Запуск бота
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            try
            {
                _client.OnMessage += MessageListener;
                _client.OnCallbackQuery += CallbackQuery;
                _client.StartReceiving();
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Обработка входящих текстовых сообщений
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MessageListener(object sender, MessageEventArgs e)
        {
            if (e.Message is null) return;
            var userName = $"{e.Message.From.FirstName} {e.Message.From.LastName}";
            curUser = CheckUser(e.Message.From.Username, userName, e.Message.Chat.Id);
            switch (e.Message.Type)
            {
                case MessageType.Text:
                    MessageLog(e.Message.Text);
                    TextHandler(e.Message);
                    break;
                case MessageType.Document:
                    MessageLog($"{userName} send doc: {e.Message.Document.FileName} {e.Message.Document.FileSize}");
                    DocumentHandler(e.Message.Document.FileId, e.Message.Document.FileName);
                    break;
                case MessageType.Audio:
                    MessageLog($"{userName} send audio: {e.Message.Audio.FileName} {e.Message.Audio.FileSize}");
                    DocumentHandler(e.Message.Audio.FileId, e.Message.Audio.FileName);
                    break;
                case MessageType.Photo:
                    MessageLog($"{userName} send photo: {e.Message.Photo[e.Message.Photo.Length - 1].FileId} {e.Message.Photo[e.Message.Photo.Length - 1].FileSize}");
                    DocumentHandler(e.Message.Photo[e.Message.Photo.Length - 1].FileId, e.Message.Photo[e.Message.Photo.Length - 1].FileId);
                    break;
                case MessageType.Video:
                    MessageLog($"{userName} send video: {e.Message.Video.FileName} {e.Message.Video.FileSize}");
                    DocumentHandler(e.Message.Video.FileId, (string.IsNullOrEmpty(e.Message.Video.FileName) ? e.Message.Video.FileId : e.Message.Video.FileName));
                    break;
                case MessageType.Voice:
                    MessageLog($"{userName} send voice: {e.Message.Video.FileName} {e.Message.Video.FileSize}");
                    DocumentHandler(e.Message.Voice.FileId, e.Message.Voice.FileId);
                    break;
                default:
                    MessageLog($"{userName} send {e.Message.Type}");
                    await _client.SendTextMessageAsync(e.Message.Chat.Id, "Прости, я не понимаю такие сообщения.");
                    MessageLog("Прости, я не понимаю такие сообщения.");
                    break;
            }

        }

        private async void MessageLog(string message, bool IsBotAnswer = false)
        {
            Debug.WriteLine($"{DateTime.Now} \t {(IsBotAnswer ? _name : curUser.Name)} : {message}");
            window.Dispatcher.Invoke(() =>
            {
                curUser.AddMessage(new TelegramMessage(message, (IsBotAnswer ? _name : curUser.Name), curUser.ChatId, IsBotAnswer));
                window.UpdateLayout();
            });

        }

        private async void SendText(string message)
        {
            Thread.Sleep(2000);
            MessageLog(message, true);
            
            await _client.SendTextMessageAsync(curUser.ChatId, message);
        }

        /// <summary>
        /// Обработка нажатия виртуальных кнопок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            Debug.WriteLine($"{DateTime.Now} \t\t {e.CallbackQuery.From.FirstName} {e.CallbackQuery.From.LastName} \t\t touch button:{e.CallbackQuery.Data}");
            MenuHandler(e.CallbackQuery);

        }

        /// <summary>
        /// Обработка текстовых сообщений
        /// </summary>
        /// <param name="msg"></param>
        public async void TextHandler(Message msg)
        {
            switch (msg.Text.ToLower())
            {
                case "/start":
                    var answer =
                        @"Список команд
/start - запуск бота
/menu - вывод меню
/keyboard - вывод клавиатуры";
                    SendText(answer);
                    break;
                case "/menu":
                case "/меню":
                case "menu":
                case "меню":
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]{ InlineKeyboardButton.WithCallbackData("Список загруженных файлов", BotCmd.ShowFiles.ToString()) },
                        new[]{ InlineKeyboardButton.WithCallbackData("Вернуться в чат", "ReturnToChat") }
                    });
                    await _client.SendTextMessageAsync(msg.From.Id, "Выберите пункт меню:",
                        replyMarkup: inlineKeyboard);
                    break;
                case "/keyboard":
                    var replyKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            new KeyboardButton("Button 1"),
                            new KeyboardButton("Button 2")
                        },
                        new[]
                        {
                            new KeyboardButton("Contact") {RequestContact = true},
                            new KeyboardButton("Geo"){RequestLocation = true}
                        }
                    });
                    await _client.SendTextMessageAsync(msg.Chat.Id, "Клавиатура включена", replyMarkup: replyKeyboard);
                    break;
                default:
                    SendText("Прости, я не понял.");
                    break;
            }
        }

        /// <summary>
        /// Обработка документов
        /// </summary>
        /// <param name="fileId">идентификатор файла на серверах Telegram</param>
        /// <param name="fileName">имя файла</param>
        /// <exception cref="NotImplementedException"></exception>
        public void DocumentHandler(string fileId, string fileName)
        {
            if (!Directory.Exists(RootPath)) { Directory.CreateDirectory(RootPath); }
            DownloadFile(fileId, Path.Combine(RootPath, fileName));
        }

        public async void MenuHandler(CallbackQuery query)
        {
            if (query.Data == "ShowFiles")
            {
                List<List<InlineKeyboardButton>> keyboard = new List<List<InlineKeyboardButton>>();
                _files = GetFileList();
                foreach (var file in _files)
                {
                    keyboard.Add(
                        new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData(
                                $"{(file.Name.Length > MaxFileName ? file.Name.Substring(0, MaxFileName) : file.Name)}",
                                file.Id.ToString())
                        });
                }

                var keyboardMarkup = new InlineKeyboardMarkup(keyboard);

                await _client.SendTextMessageAsync(query.From.Id, "Список файлов:",
                    replyMarkup: keyboardMarkup);
            }

            foreach (var file in _files)
            {
                if (query.Data == file.Id.ToString())
                {
                    using (FileStream fs = new FileStream(file.Path, FileMode.Open))
                    {
                        await _client.SendDocumentAsync(query.From.Id, new InputOnlineFile(fs, file.Name));
                    }

                }
            }
        }


        private async void DownloadFile(string fileId, string path)
        {
            try
            {
                var file = await _client.GetFileAsync(fileId);
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await _client.DownloadFileAsync(file.FilePath, fs);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error downloading: " + ex.Message);
            }
        }

        private List<BotFile> GetFileList()
        {
            Random rnd = new Random();
            var ls = new List<BotFile>();
            foreach (var file in GetRecursFiles(RootPath))
            {
                ls.Add(new BotFile(rnd.Next(), file));
            }
            return ls;
        }

        private List<string> GetRecursFiles(string startPath)
        {
            var ls = new List<string>();
            try
            {
                var folders = Directory.GetDirectories(startPath);
                foreach (var folder in folders)
                {
                    ls.AddRange(GetRecursFiles(folder));
                }

                var files = Directory.GetFiles(startPath);
                foreach (var file in files)
                {
                    ls.Add(file);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return ls;
        }

        private string CheckDirectory(string path)
        {
            if (!Directory.Exists(RootPath)) { Directory.CreateDirectory(RootPath); }
            if (!Directory.Exists($"{RootPath}/{path}")) { Directory.CreateDirectory($"{RootPath}/{path}"); }

            return $"{RootPath}/{path}";
        }

        private TelegramUser CheckUser(string Nick, string Name, long Id)
        {
            var user = new TelegramUser(Nick, Name, Id);
            if (!UserList.Contains(user))
            {
                window.Dispatcher.Invoke(() =>
                {
                    UserList.Add(user);
                });
            }
            return UserList[UserList.IndexOf(user)];
        }

    }
}