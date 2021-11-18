using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
        // TODO: удалить, общий чат не нужен. Даешь чат для каждого пользователя!
        //public ObservableCollection<TelegramMessage> BotMessageLog { get; set; }
        public ObservableCollection<TelegramUser> UserList { get; set; }

        private TelegramBotClient _client;
        private string _name;
        private List<BotFile> _files;
        private enum BotCmd { ShowFiles }
        private const string RootPath = "BotData";
        private const int MaxFileName = 60;

        private int curUserIndex = 0;

        public string Name { get { return _name; } }

        /// <summary>
        /// Инициализация клиента
        /// </summary>
        /// <param name="token"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public MyTelegramBot(MainWindow w, string token)
        {
            // TODO: удалить, общий чат не нужен. Даешь чат для каждого пользователя!
            //BotMessageLog = new ObservableCollection<TelegramMessage>();      
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

        internal async void SendMessage(string message, string Id)
        {
            await _client.SendTextMessageAsync(Id, message);
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
            curUserIndex = CheckUser(e.Message.From.Username, userName, e.Message.Chat.Id);
            switch (e.Message.Type)
            {
                case MessageType.Text:
                    Debug.WriteLine($"{DateTime.Now} \t\t {userName} \t\t send text:{e.Message.Text}");
                    UserList[curUserIndex].Messages.Add(new TelegramMessage(e.Message.Text, userName, e.Message.Chat.Id));
                    TextHandler(e.Message);
                    break;
                case MessageType.Document:
                    Debug.WriteLine($"{DateTime.Now} \t\t {userName} \t\t send doc: {e.Message.Document.FileName} {e.Message.Document.FileSize}");
                    UserList[curUserIndex].Messages.Add(new TelegramMessage($"{userName} send doc: {e.Message.Document.FileName} {e.Message.Document.FileSize}", userName, e.Message.Chat.Id));
                    DocumentHandler(e.Message.Document.FileId, e.Message.Document.FileName);
                    break;
                case MessageType.Audio:
                    Debug.WriteLine($"{DateTime.Now} \t\t {userName} \t\t send audio: {e.Message.Audio.FileName} {e.Message.Audio.FileSize}");
                    UserList[curUserIndex].Messages.Add(new TelegramMessage($"{userName} send audio: {e.Message.Audio.FileName} {e.Message.Audio.FileSize}", userName, e.Message.Chat.Id));
                    DocumentHandler(e.Message.Audio.FileId, e.Message.Audio.FileName);
                    break;
                case MessageType.Photo:
                    Debug.WriteLine($"{DateTime.Now} \t\t {userName} \t\t send photo: {e.Message.Photo[e.Message.Photo.Length - 1].FileId} {e.Message.Photo[e.Message.Photo.Length - 1].FileSize}");
                    UserList[curUserIndex].Messages.Add(new TelegramMessage($"{userName} send photo: {e.Message.Photo[e.Message.Photo.Length - 1].FileId} {e.Message.Photo[e.Message.Photo.Length - 1].FileSize}", userName, e.Message.Chat.Id));
                    DocumentHandler(e.Message.Photo[e.Message.Photo.Length - 1].FileId, e.Message.Photo[e.Message.Photo.Length - 1].FileId);
                    break;
                case MessageType.Video:
                    Debug.WriteLine($"{DateTime.Now} \t\t {userName} \t\t send video: {e.Message.Video.FileName} {e.Message.Video.FileSize}");
                    UserList[curUserIndex].Messages.Add(new TelegramMessage($"{userName} send video: {e.Message.Video.FileName} {e.Message.Video.FileSize}", userName, e.Message.Chat.Id));
                    DocumentHandler(e.Message.Video.FileId, (string.IsNullOrEmpty(e.Message.Video.FileName) ? e.Message.Video.FileId : e.Message.Video.FileName));
                    break;
                case MessageType.Voice:
                    Debug.WriteLine($"{DateTime.Now} \t\t {userName} \t\t send voice: {e.Message.Video.FileName} {e.Message.Video.FileSize}");
                    UserList[curUserIndex].Messages.Add(new TelegramMessage($"{userName} send voice: {e.Message.Video.FileName} {e.Message.Video.FileSize}", userName, e.Message.Chat.Id));
                    DocumentHandler(e.Message.Voice.FileId, e.Message.Voice.FileId);
                    break;
                default:
                    Debug.WriteLine($"{DateTime.Now} \t\t {userName} \t\t send {e.Message.Type}");
                    UserList[curUserIndex].Messages.Add(new TelegramMessage($"{userName} send {e.Message.Type}", userName, e.Message.Chat.Id));
                    await _client.SendTextMessageAsync(e.Message.Chat.Id, "Прости, я не понимаю такие сообщения.");
                    UserList[curUserIndex].Messages.Add(new TelegramMessage("Прости, я не понимаю такие сообщения.", userName, e.Message.Chat.Id));
                    break;
            }

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
            // TODO: удалить, общий чат не нужен. Даешь чат для каждого пользователя!
            //window.Dispatcher.Invoke(() =>
            //{
            //    BotMessageLog.Add(new TelegramMessage(
            //        msg.Text, $"{msg.Chat.FirstName} {msg.Chat.LastName}", msg.Chat.Id));
            //    //BotMessageLog = UserList[curUserIndex].Messages;
            //});

            switch (msg.Text.ToLower())
            {
                case "/start":
                    var answer =
                        @"Список команд
/start - запуск бота
/menu - вывод меню
/keyboard - вывод клавиатуры";
                    await _client.SendTextMessageAsync(msg.Chat.Id, answer);
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
                    await _client.SendTextMessageAsync(msg.Chat.Id, "Прости, я не понял.");
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

        private int CheckUser(string Nick, string Name, long Id)
        {
            var user = new TelegramUser(Nick, Name, Id);
            if (!UserList.Contains(user))
            {
                window.Dispatcher.Invoke(() =>
                {
                    UserList.Add(user);
                });
            }
            return UserList.IndexOf(user);
        }

    }
}