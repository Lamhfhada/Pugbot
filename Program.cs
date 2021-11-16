using Discord;
using Discord.WebSocket;
using System.Text.RegularExpressions;

namespace Pugbot
{
    /*Please don't judge me for this bullshit spaghetti code. I don't think it will ever have to be expanded,
    besides, I was too lazy to do something normal. So it just works and that's fine */
    class Pugbot
    {
        DiscordSocketClient client;
        /*This has nothing to do with SOLID or any other principles. I just store the code for commands in this 
        class in order to not make the main class even more confusing, lol */
        Commands command;
        static void Main(string[] args)
            => new Pugbot().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            client = new DiscordSocketClient();
            client.MessageReceived += CommandsHandler;
            client.SetGameAsync("!help");
            client.Log += Log;

            command = new Commands();

            var token = "INSERT_TOKEN_HERE";

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            Console.ReadLine();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        /*Yes, I handle commands with swith-case. So if you ever want to expand this shit you'll need to:
        add commands name here --> create it in Commands.cs --> call method from case block --> manually update "!help" */
        private Task CommandsHandler(SocketMessage msg)
        {
            if (!msg.Author.IsBot)
                switch (msg.Content)
                {
                    case "!help":
                        {;
                            msg.Channel.SendMessageAsync("", false, command.Help().Build());
                            break;
                        }
                    case "!pug":
                        {
                            var player = msg.Author as SocketGuildUser;
                            msg.Channel.SendMessageAsync(command.Pug(player));
                            break;
                        }
                    case "!unpug":
                        {
                            var player = msg.Author as SocketGuildUser;
                            msg.Channel.SendMessageAsync(command.Unpug(player));
                            break;
                        }
                    case "!clear":
                        {
                            msg.Channel.SendMessageAsync(command.Clear());
                            break;
                        }
                    case "!puggers":
                        {
                            msg.Channel.SendMessageAsync(command.Puggers());
                            break;
                        }
                    case var size when new Regex(@"^!size\s\d\d?").IsMatch(size):
                        {
                            msg.Channel.SendMessageAsync(command.Size(size));
                            break;
                        }
                    case "!random":
                        {
                            msg.Channel.SendMessageAsync(command.Random());
                            break;
                        }
                    case "!start":
                        {
                            try
                            {
                                var teams = command.Start();

                                string jinrai = "";
                                foreach (var player in teams.Item1)
                                {
                                    jinrai += player.Mention + " ";
                                }

                                string nsf = "";
                                foreach (var player in teams.Item2)
                                {
                                    nsf += player.Mention + " ";
                                }
                                msg.Channel.SendMessageAsync($"Jinrai: {jinrai} \n NSF: {nsf}");
                            } catch (InvalidOperationException ex)
                            {
                                msg.Channel.SendMessageAsync("Not enough players to start the pug");
                            }
                            break;
                        }
                    case "!pug?":
                        {
                            if(msg.Author.Id == 415952615264747540)
                            {
                                msg.Channel.SendMessageAsync("jungle: stop SPAMMING me you dutch prick");
                            }
                            else
                            {
                                msg.Channel.SendMessageAsync($"Majestic? Oh, it's just {msg.Author.Mention}");
                            }
                            break;
                        }
                }
            return Task.CompletedTask;
        }
    }
}