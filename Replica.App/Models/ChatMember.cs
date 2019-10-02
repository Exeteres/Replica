using System;
using LiteDB;

namespace Replica.App.Models
{
    public class ChatMember : Model
    {
        public int ChatId { get; set; }
        public virtual Chat Chat { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public UserScope Scope { get; set; }
        public MuteState Mute { get; set; }
    }
}