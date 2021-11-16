using Discord;
using Discord.WebSocket;

namespace Pugbot
{
    class Commands
    {
        private Dictionary<SocketGuildUser, Team> puggers;
        private bool initiative;
        private bool predefinedTeams;
        private int pugSize;
        public Commands()
        {
            puggers = new Dictionary<SocketGuildUser, Team>();
            var rand = new Random();
            initiative = RandomizeInitiative();
            predefinedTeams = false;
            pugSize = 10;
        }

        enum Team
        {
            NO,
            JINRAI,
            NSF
        }

        public EmbedBuilder Help()
        {
            return new EmbedBuilder()
                .WithColor(Color.Green)
                .WithTitle("Command List")
                .AddField("!pug", "join pug")
                .AddField("!unpug", "leave pug")
                .AddField("!clear", "clear players list")
                .AddField("!puggers", "show players list")
                .AddField("!size", "Set pug size (10 players by default)")
                .AddField("!random", "randomize teams")
                .AddField("!start", "ping players who have subscribed to this pug " +
                "(if teams are not predefined it will randomize teams before start)")
                .AddField("TODO LIST:", "Implement commands related to team captains and players pick");
        }

        public string Pug(SocketGuildUser player)
        {
            if (puggers.ContainsKey(player))
                return $"{RetrieveName(player)} is already on the list";

            puggers.Add(player, Team.NO);

            if(puggers.Count == pugSize)
            {
                string mention = "";
                foreach (var p in puggers)
                    mention += p.Key.Mention + " ";

                return $"{RetrieveName(player)} added \n\n The pug is ready to start! " +
                    $"Randomize teams with !random (you can do this until you are satisfied with the result) " +
                    $"or just !start the pug \n {mention}";
            }

            return $"{RetrieveName(player)} added";
        }

        public string Unpug(SocketGuildUser player)
        {
            if (!puggers.ContainsKey(player))
                return $"Can't find {RetrieveName(player)} in players list";

            puggers.Remove(player);
            return $"{RetrieveName(player)} removed from players list";
        }

        public string Clear()
        {
            puggers.Clear();
            return "Cleared!";
        }

        public string Puggers()
        {
            if (puggers.Count == 0)
                return "Puggers list is empty";

            List<string> nicknames = new List<string>();
            List<SocketGuildUser> players = new List<SocketGuildUser>(this.puggers.Keys);
            foreach (var player in players)
                nicknames.Add(RetrieveName(player));

            return string.Join(", ", nicknames);
        }

        public string Size(string message)
        {
            var toRemove = "!size ";
            string argument = message.Remove(0, toRemove.Length);

            if(Int32.TryParse(argument, out int size) && size != 0 && size != 1)
            {
                pugSize = size;
                return "Pug size changed to " + size;
            }

            pugSize = 10;
            return "Can't set this value. Changing pug size to default (10 players)";
        }

        public string Random()
        {
            if (puggers.Count == 0)
                return "Puggers list is empty";

            if (puggers.Count == 1)
                return "There's only one person on the list";

            RandomizeTeams();

            var jinraiPlayers = puggers.Where(p => p.Value == Team.JINRAI).Select(p => RetrieveName(p.Key)).ToList();
            var nsfPlayers = puggers.Where(p => p.Value == Team.NSF).Select(p => RetrieveName(p.Key)).ToList();

            predefinedTeams = true;

            return $"Jinrai: {string.Join(", ", jinraiPlayers)} \n NSF: {string.Join(", ", nsfPlayers)}";
        }

        public Tuple<List<SocketGuildUser>, List<SocketGuildUser>> Start()
        {
            if(puggers.Count == 0 || puggers.Count == 1)
            {
                throw new InvalidOperationException();
            }

            if (predefinedTeams)
            {
                var jinraiPlayers = puggers.Where(p => p.Value == Team.JINRAI).Select(p => p.Key).ToList();
                var nsfPlayers = puggers.Where(p => p.Value == Team.NSF).Select(p => p.Key).ToList();
                ReturnDefaultValues();
                return Tuple.Create(jinraiPlayers, nsfPlayers);
            }
            else
            {
                RandomizeTeams();
                var jinraiPlayers = puggers.Where(p => p.Value == Team.JINRAI).Select(p => p.Key).ToList();
                var nsfPlayers = puggers.Where(p => p.Value == Team.NSF).Select(p => p.Key).ToList();
                ReturnDefaultValues();
                return Tuple.Create(jinraiPlayers, nsfPlayers);
            }
        }

        private void RandomizeTeams()
        {
            Random rand = new Random();

            List<SocketGuildUser> notInTeam = puggers.Where(p => p.Value == Team.NO).Select(p => p.Key).ToList();

            while (notInTeam.Count > 0)
            {
                var index = rand.Next(0, notInTeam.Count - 1);
                SocketGuildUser player = notInTeam[index];
                if(initiative == true)
                {
                    puggers[player] = Team.JINRAI;
                    initiative = false;
                }
                else
                {
                    puggers[player] = Team.NSF;
                    initiative = true;
                }

                notInTeam.RemoveAt(index);
            }
        }

        private string RetrieveName(SocketGuildUser player)
        {
            if (string.IsNullOrEmpty(player.Nickname) || string.IsNullOrWhiteSpace(player.Nickname))
                return player.Username;

            return player.Nickname;
        }

        private bool RandomizeInitiative()
        {
            Random rand = new Random(); 
            return rand.Next(0, 1) == 1 ? true : false;
        }

        private void ReturnDefaultValues()
        {
            predefinedTeams = false;
            initiative = RandomizeInitiative();
            pugSize = 10;
            puggers.Clear();
        }
    }
}
