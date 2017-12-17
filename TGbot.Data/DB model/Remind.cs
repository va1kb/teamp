using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGbot.DB_model
{
    public class Remind
    {
        public int Id { get; set; }
        public int userId { get; set; }
        public string Text { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime RemindDate { get; set; }

        //сообщение прочитано
        public bool isInvoked { get; set; }
    }
}
