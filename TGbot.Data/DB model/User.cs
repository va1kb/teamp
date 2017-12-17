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
        //Храним userid, заметки/напоминания


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
