using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TGbot.DB_model
{
    public class User
    {
        //Короче, тут просто показывается EF какие есть таблицы
        //В коде в юзера пихаются notes
        //Храним userid, заметки/напоминания...
        //Конец


        public int Id { get; set; }
        public int UserId { get; set; }

        //последнее действие пользователя
        public int state { get; set; }

        //заметки
        public List<Notes> notes { get; set; }

        //напоминания
        public List<Remind> reminds { get; set; }

        public User()
        {
            notes = new List<Notes>();
            reminds = new List<Remind>();
        }
    }
}
