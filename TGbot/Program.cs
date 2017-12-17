using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Data.Entity;
using TGbot.DB_model;

namespace TGbot
{
    class Program
    {
        static readonly string key = "450869292:AAG8ZUNG_KC66UuVSme8LdHXIufRTZZ7dQA";
        private static readonly TelegramBotClient Bot = new TelegramBotClient(key);

        static void Main(string[] args)
        {
            
            

            //начало разговора с ботом
            Bot.StartReceiving();


            //ивент ловит сообщения
            Bot.OnMessage += BotRequest.Request;
            Bot.SetWebhookAsync();
            Check();
            Console.ReadLine();
            //конец разговора с ботом
            Bot.StopReceiving();

        }

        /// <summary>
        /// проверка в базе напоминаний, которые унжно отослать
        /// </summary>
        private static async void Check()
        {
            while (true)
            {
                await CheckBD();

                // каждые 2 секунды
                Thread.Sleep(2000);
            }
        }

        private static async Task CheckBD()
        {
            using (var db = new MyDB())
            {
                //берем всех юзеров с их напоминаниями
                var user = db.Users.Include(s => s.reminds);
                foreach (var it in user)
                {
                    //проходим по каждому напоминани каждого юзера
                    foreach (var rem in it.reminds)
                        //если дата <= чем сейчас и ещё не было отправлено
                        if (rem.RemindDate <= DateTime.Now && !rem.isInvoked)
                        {
                            await Bot.SendTextMessageAsync(it.UserId, rem.Text);
                            //изменение флага на то, что отправил
                            rem.isInvoked = true;
                        }
                    
                }
                await db.SaveChangesAsync();
            }
        }

    }
}
