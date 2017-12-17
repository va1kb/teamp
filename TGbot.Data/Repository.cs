using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TGbot.DB_model;

namespace TGbot
{
    public class BotRequest
    {

        private static TelegramBotClient Bot;

        //действия пользователя
        enum States { None, Note, Remind, Date, Image }

        //обработчик сообщений
        public static async void Request(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            Bot = (TelegramBotClient)sender;
            States st = 0;
            //если сообщение не бота, то чекаем что делал человек в послежний раз
            if (!e.Message.From.IsBot)
                st = await getState(e.Message.From);

            //выбираем следующее действие
            switch (st)
            {
                case States.None:
                    await chooseState(e.Message);
                    break;
                case States.Note:
                    await getNoteContext(e.Message);
                    break;
                case States.Remind:
                    await getRemindContext(e.Message);
                    break;
                case States.Date:
                    await setDate(e.Message);
                    break;
                case States.Image:
                    await getImageNote(e.Message);
                    break;
                

            }
        }

        //проверка валидности url 
        public static bool CheckURLValid(string source)
        {
            Uri uriResult;
            return Uri.TryCreate(source, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
        }

        private static async Task getImageNote(Message msg)
        {
            var Context = msg.Text;
            if (CheckURLValid(Context))
            {
                await Bot.SendTextMessageAsync(msg.Chat.Id, "Ссылка некорректна, введите ещё раз");
                return;
            }
            using (var db = new MyDB())
            {
                try
                {
                    //создание нового ноута
                    var note = db.Note.Create();
                    //достаем из базы юзера по его телеграм айди
                    var user = db.Users.Include(s => s.notes).FirstOrDefault(x => x.UserId == msg.From.Id);
                    //запихиваем весь контент в ноут
                    note.CreationDate = DateTime.Now;
                    note.userID = msg.From.Id;
                    note.Text = "";
                    note.Img = Context;
                    //изменяем последнее действие юзера
                    user.state = 0;

                    //пихаем ноут к юзеру
                    user.notes.Add(note);
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            await Bot.SendTextMessageAsync(msg.Chat.Id, "Заметка готова!");
        }


        private static async Task getAllImages(Message msg)
        {
            using (var db = new MyDB())
            {
                try
                {
                    //ищем юзера
                    var user = db.Users.Include(s => s.notes).Single(x => x.UserId == msg.From.Id);
                    int noteCount = 0;
                    foreach (var it in user.notes)
                        //проверка на то что есть картинка
                        if (it.Img.Length > 0)
                        {
                            await Bot.SendTextMessageAsync(msg.Chat.Id, $"note {++noteCount}:\nDate: {it.CreationDate}");
                            await Bot.SendPhotoAsync(msg.Chat.Id, new FileToSend(it.Img));
                        }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// Поиск юзера и его последнего действия
        /// Если юзер новый, то создает в бд
        /// </summary>
        /// <param name="msg"></param>сообщение
        /// <returns></returns>
        private static async Task<States> getState(Telegram.Bot.Types.User msg)
        {
            using (MyDB db = new MyDB())
            {
                try
                {
                    var res = db.Users.First(x => x.UserId == msg.Id);
                    return (States)res.state;
                }
                catch (Exception)
                {
                    db.Users.Add(new DB_model.User { UserId = msg.Id });
                    await db.SaveChangesAsync();
                }

            }
            return 0;
        }

        /// <summary>
        /// выбор действия юзера
        /// </summary>
        /// <param name="msg"></param>сообщение юзера
        /// <returns></returns>
        private static async Task<bool> chooseState(Message msg)
        {
            States st;
            try
            {
                if (msg.Text == null)
                    return true;
                switch (msg.Text.ToLower().Trim())
                {

                    case "/createnote":
                        st = States.Note;
                        await Bot.SendTextMessageAsync(msg.Chat.Id, "Напишите текст заметки");
                        break;
                    case "/createremind":
                        st = States.Remind;
                        await Bot.SendTextMessageAsync(msg.Chat.Id, "Напишите текст напоминания");
                        break;
                    case "/notes":
                        await getNotes(msg);
                        st = States.None;
                        break;
                    case "/reminds":
                        await getReminds(msg);
                        st = States.None;
                        break;
                    case "/info":
                        st = States.None;
                        await Bot.SendTextMessageAsync(msg.Chat.Id,
                            "Команды:\n/createnote - создать заметки\n/createremind - создать напоминание\n/createimagenote - создать заметку-изображение\n/notes - вывести все заметки\n/reminds - вывести все запланированные напоминания\n/imagenotes - вывести все заметки-изображения");
                        break;
                    case "/start":
                        st = States.None;
                        await Bot.SendTextMessageAsync(msg.Chat.Id,
                            "Привет!\nДавай начнём работать.\nЧтобы получить список доступных команд - введите /info");
                        break;
                    case "/imagenote":
                        await Bot.SendTextMessageAsync(msg.Chat.Id, "Введите url картинки");
                        st = States.Image;
                        break;
                    case "/imagenotes":
                        await getAllImages(msg);
                        st = States.None;
                        break;
                    default:
                        st = States.None;
                        await Bot.SendTextMessageAsync(msg.Chat.Id, "Я не понимаю этой команды! :\nПомощь /info");
                        break;

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return true;
            }
            using (var db = new MyDB())
            {
                try
                {
                    var res = db.Users.Single(x => x.UserId == msg.From.Id);
                    //изменение действий юзера
                    res.state = (int)st;
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    
                }
            }
            return true;
        }

        /// <summary>
        /// Добавляем новую заметку
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private static async Task<bool> getNoteContext(Message msg)
        {
            var Context = msg.Text;
            using (var db = new MyDB())
            {
                try
                {
                    var note = db.Note.Create();
                    var user = db.Users.Include(s => s.notes).FirstOrDefault(x => x.UserId == msg.From.Id);
                    note.CreationDate = DateTime.Now;
                    note.userID = msg.From.Id;
                    note.Text = Context;
                    note.Img = "";
                    user.state = 0;

                    user.notes.Add(note);
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            await Bot.SendTextMessageAsync(msg.Chat.Id, "Заметка готова!");
            return true;
        }

        /// <summary>
        /// Создаем новое напоминание и ждем пока пользователь добавит дату
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private static async Task<bool> getRemindContext(Message msg)
        {
            var Context = msg.Text;
            await Bot.SendTextMessageAsync(msg.Chat.Id, "Введите дату в формате: dd:mm:yyyy hh:mm");
            using (var db = new MyDB())
            {
                try
                {
                    var remind = db.Reminds.Create();
                    var user = db.Users.Include(s => s.reminds).Single(x => x.UserId == msg.From.Id);
                    remind.CreationDate = DateTime.Now;
                    remind.userId = msg.From.Id;
                    remind.Text = Context;
                    remind.RemindDate = DateTime.Now.AddYears(1);
                    remind.isInvoked = false;
                    user.state = 3;

                    user.reminds.Add(remind);
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return true;
        }

        /// <summary>
        /// Устанавливаем дату напоминания
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private static async Task<bool> setDate(Message msg)
        {
            var context = msg.Text;
            DateTime date = DateTime.Now;
            if(!DateTime.TryParse(msg.Text, out date))
            {
                await Bot.SendTextMessageAsync(msg.Chat.Id, "Неправильная дата, введите заново:\nФормат даты: dd:mm:yyyy hh:mm");
                return true;
            }



            await Bot.SendTextMessageAsync(msg.Chat.Id, "Напоминание успешно создано!");
            using (var db = new MyDB())
            {
                try
                {
                    var user = db.Users.Include(s => s.reminds).Single(x => x.UserId == msg.From.Id);
                    
                    
                    user.reminds[user.reminds.Count - 1].RemindDate = date;
                    user.state = 0;
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                return true;
            }
        }

        /// <summary>
        /// Вывод всех текстовых записок пользователя
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private static async Task getNotes(Message msg)
        {
            using (var db = new MyDB())
            {
                try
                {
                    var user = db.Users.Include(s => s.notes).Single(x => x.UserId == msg.From.Id);
                    int noteCount = 0;
                    foreach (var it in user.notes)
                        if (!(it.Img.Length > 0))
                            await Bot.SendTextMessageAsync(msg.Chat.Id, $"Заметка {++noteCount}:\nТекст: {it.Text}\nДата: {it.CreationDate}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// Вывод всех предстоящих напоминий пользователя
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private static async Task getReminds(Message msg)
        {
            using (var db = new MyDB())
            {
                try
                {
                    var user = db.Users.Include(s => s.reminds).Single(x => x.UserId == msg.From.Id);
                    int noteCount = 0;
                    foreach (var it in user.reminds)
                        if (it.RemindDate > DateTime.Now)
                            await Bot.SendTextMessageAsync(msg.Chat.Id, $"Напоминание {++noteCount}:\nТекст: {it.Text}\nДата:{it.CreationDate}\nВремя напоминания: {it.RemindDate}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}