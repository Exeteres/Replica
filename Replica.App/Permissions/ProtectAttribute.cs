using System;
using System.Linq;
using Replica.App.Extensions;
using Replica.App.Models;
using Replica.Core.Commands;
using Replica.Core.Contexts;

namespace Replica.App.Permissions
{
    public class ProtectAttribute : Attribute, IParameterRestriction
    {
        private User _me;
        internal UserScope Scope { get; private set; }
        private Context _context;

        public ProtectAttribute(UserScope scope) => Scope = scope;

        private bool CheckChat(Chat chat)
        {
            if (chat == null) chat = _context.GetChat();
            var position = chat.Members.FirstOrDefault(x => x.UserId == _me.Id);
            return position != null && position.Scope >= Scope;
        }

        public bool Check(Context context, object param)
        {
            _me = context.GetUser();
            if (_me.IsSuperUser) return true;
            _context = context;
            switch (param)
            {
                case Chat chat:
                    return CheckChat(chat);
                case Chat[] chats:
                    return chats.All(x => CheckChat(x));
                case Route route:
                    return route.Owner.Id == _me.Id;
            }
            return false;
        }
    }
}