using System.Threading;
using System;

using Replica.Core.LevelDB;
using Replica.Core;
using Replica.Core.Configuration;
using Replica.Controllers.Telegram;
using Replica.Controllers.VK;

using Replica.App.Commands;
using Replica.App.Middleware;
using Replica.App.Models;
using Replica.App.Converters;
using System.Collections.Generic;
using Replica.Core.Commands;
using Serilog;
using System.Linq;
using LibGit2Sharp;
using System.IO;
using Replica.Core.Entity;
using System.Reflection;
using Replica.App.Logic;

namespace Replica.App
{
    internal static class Program
    {
        public static BotCore Core { get; private set; }

        private static SettingsLoader<AppOptions> _settings;
        public static AppOptions Options => _settings?.Options;

        public static DateTime StartTime { get; } = DateTime.Now.ToUniversalTime();

        private static void CreateAdmins()
        {
            var db = new Models.RepoContext();
            foreach (var admin in Options.SU)
            {
                var user = db.Users.FirstOrDefault(x => x.Username == admin);
                if (user == null) user = new User { Username = admin };
                user.IsSuperUser = true;
                db.Update(user);
            }
            db.SaveChanges();
        }

        private static void BroadcastChangelog()
        {
            var gitdir = new string[] {
                ".git",
                "../.git"
            }.FirstOrDefault(x => Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), x)));
            if (gitdir == null) return;

            using var repo = new Repository(gitdir);
            if (repo.Tags.Count() < 2) return;

            var tags = repo.Tags.TakeLast(2).ToArray();
            var (old, n) = (tags[0], tags[1]);

            if (KeyValue.Get("glc") == n.Target.Sha) return;
            KeyValue.Set("glc", n.Target.Sha);

            var db = new Models.RepoContext();
            foreach (var chat in db.Chats)
            {
                if (!chat.Notifications) continue;

                var localizer = Program.Core.ResolveLocalizer(Assembly.GetExecutingAssembly(), chat.Language);
                var message = OutMessage.FromCode($"[{localizer["Update"]} | {old.FriendlyName} -> {n.FriendlyName}]\n{localizer["Changes"]}:\n{n.Annotation.Message}");

                Program.Core
                    .ResolveController(chat.Controller)
                    .SendMessage(chat.ChatId, message);
            }
        }

        public static void LoadSettings()
        {
            _settings = SettingsLoader<AppOptions>.FromFile("settings.json");
        }

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += OnProcessExit;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();

            LoadSettings();

            var core = new BotCore(_settings);
            core.EnableCaching<LevelCache>();
            core.EnableAssemblyLocalization();

            core.RegisterController<TelegramController>();
            core.RegisterController<VkController>();

            var commands = core.RegisterModule<CommandsModule>();
            commands.RegisterConverter<UserConverter>();
            commands.RegisterConverter<ChatConverter>();
            commands.RegisterConverter<RouteConverter>();

            var router = core.Router;
            router.AddHandler<PrimaryMiddleware>();
            router.AddHandler<UsersMiddleware>();
            router.AddHandler<ReplicatingMiddleware>();

            router.AddHandler<TestCommand>();
            router.AddHandler<SudoCommand>();
            router.AddHandler<ChatsCommand>();
            router.AddHandler<UsersCommand>();
            router.AddHandler<RoutesCommand>();
            router.AddHandler<HelpCommand>();

            HelpMessage.Init(commands.GetCommands());
            core.Start();
            Core = core;

            CreateAdmins();

            if (Options.Changelog)
                BroadcastChangelog();

            Log.Information("Replica started");

            Thread.Sleep(Timeout.Infinite);
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            Core.Dispose();
            Log.Information("Replica stopped");
        }
    }
}
