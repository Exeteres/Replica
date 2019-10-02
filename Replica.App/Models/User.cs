using System.Collections.Generic;
using System.Linq;
using Replica.Core.Entity;

namespace Replica.App.Models
{
    public class User : Model
    {
        public virtual List<ChatMember> Chats { get; set; } = new List<ChatMember>();
        public virtual List<Account> Accounts { get; set; } = new List<Account>();
        public virtual List<Route> Routes { get; set; } = new List<Route>();

        public string Username { get; set; }
        public bool IsSuperUser { get; set; }

        public void AssignPermissions(Chat chat, UserScope scope)
        {
            var db = new RepoContext();
            var position = Chats.FirstOrDefault(x => x.ChatId == chat.Id);
            if (position == null)
            {
                position = new ChatMember();
                position.User = this;
                position.Chat = chat;
            }
            position.Scope = scope;
            db.ChatMembers.Update(position);
            db.SaveChanges();
        }

        private static User FromUsername(string username)
            => new User { Username = username.ToLower() };

        public static User EnsureCreated(string controller, UserInfo user)
        {
            var db = new RepoContext();

            // Поиск пользователя по айди одного из аккаунтов
            var existed = db.Users.FirstOrDefault(x => x.Accounts.Any(y => y.Controller == controller && y.UserId == user.Id));

            if (existed != null)
                return existed;

            // Ищем по юзернейму или создаем нового
            var usrn = user.Username != null
                ? db.Users.FirstOrDefault(x => x.Username == user.Username)
                ?? FromUsername(user.Username) : FromUsername(user.Username);

            // Добавляем ему аккаунт
            var account = new Account { Controller = controller, UserId = user.Id };
            usrn.Accounts.Add(account);
            db.Update(usrn);
            db.SaveChanges();
            return usrn;
        }

        #region Проверка на равенство
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (User)obj;
            return other.Username == Username;
        }

        public override int GetHashCode()
        {
            return Username.GetHashCode();
        }
        #endregion
    }
}