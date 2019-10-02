using System.Collections.Generic;
using System.Linq;

namespace Replica.App.Models
{
    public class Route : Model
    {
        public virtual List<ChatRoute> Chats { get; set; } = new List<ChatRoute>();

        public int OwnerId { get; set; }
        public virtual User Owner { get; set; }

        public override string ToString()
            => $"[{Id}] {string.Join(" ⇆ ", Chats.Select(x => x.ChatId.ToString("D2")))}";

        public string RenderInfo()
            => $"[{Id}] [{Owner.Username}] {string.Join(" ⇆ ", Chats.Select(x => x.ChatId.ToString("D2")))}";

        public static Route Resolve(User owner, ICollection<Chat> chats)
        {
            var db = new RepoContext();

            // Ищем частичное совпадение в более чем 2 маршрутах
            var routes = db.Routes
                .Where(x => x.Chats
                    .Any(y => chats.Contains(y.Chat) && y.Route.Owner.Id == owner.Id))
                .ToList();

            if (routes.Any())
            {
                routes[0].Chats = routes
                    .Select(x => x.Chats.AsEnumerable())
                    .Aggregate((a, b) => a.Concat(b))
                    .Distinct()
                    .ToList();
                db.Update(routes[0]);
                foreach (var other in routes.Skip(1))
                    db.Remove(other);
                db.SaveChanges();
                return routes[0];
            }

            // Или полное совпадение
            var route = db.Routes
                .FirstOrDefault(x => x.Chats.Select(y => y.Chat)
                    .Intersect(chats).Count() == chats.Count);
            if (route != null) return route;

            // Или создаем новый маршрут
            return new Route();
        }

        public static (bool, Route) Sync(User owner, ICollection<Chat> chats)
        {
            var db = new RepoContext();
            var route = Resolve(owner, chats);
            var created = route.Id == 0;
            foreach (var chat in chats)
            {
                if (route.Chats.Any(x => x.ChatId == chat.Id)) continue;
                route.Chats.Add(new ChatRoute { Chat = chat });
            }
            route.Owner = owner;
            db.Update(route);
            db.SaveChanges();
            return (created, route);
        }

        public void DeleteIfNotUsed()
        {
            if (Chats.Count < 2)
            {
                var db = new RepoContext();
                db.Remove(this);
                db.SaveChanges();
            }
        }

        public bool Exclude(ICollection<Chat> chats)
        {
            var db = new RepoContext();
            Chats.RemoveAll(x => chats.Any(y => x.ChatId == y.Id));
            if (Chats.Count < 2)
            {
                db.Remove(this);
                db.SaveChanges();
                return true;
            }
            db.Update(this);
            db.SaveChanges();
            return false;
        }


        #region Equality
        public override int GetHashCode() => Id.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is Route other)
            {
                return Id == other.Id;
            }
            return false;
        }
        #endregion
    }
}