using Bibs_Discord_dotNET.Commons;
using Bibs_Discord_dotNET.Ultilities;
using Bibs_Infrastructure;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibs_Discord.NET.Modules
{
    public class Answer: ModuleBase<SocketCommandContext>
    {

        [Command("answer")]
        [Summary("Answers a yes no question, much like the magic eightball")]
        public async Task AnswerQuestion([Remainder] string question)
        {
            Random random = new Random();
            string[] answers = {
                "Yes",
                "No",
                "Maybe",
                "dunno",
                "Yesn't",
                "Perhaps",
                "Possibly",
                "Positively",
                "Conceivably",
                "I don't feel like answering right now, try again later",
                "In your dreams",
                "You sure you want to know the answer to that?",
                "*cricket noises*",
                "W-w-why would you ask that?"
            };
            string output = "";

            if ((question.IndexOf("?", StringComparison.CurrentCultureIgnoreCase) >= 0) == false)
            {
                await Context.Channel.TriggerTypingAsync();
                await ReplyAsync("That's not a question though? Make a question pls!");
                return;
            }
            if ((question.IndexOf("are you cute", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "I'm the cutest";
            }
            else if ((question.IndexOf("how old are you", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "Too young for you!";
            }
            else if (((question.IndexOf("are you a girl", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
                || (question.IndexOf("are you boy or a girl", StringComparison.CurrentCultureIgnoreCase) >= 0)
                || (question.IndexOf("what is your gender", StringComparison.CurrentCultureIgnoreCase) >= 0)
                || (question.IndexOf("what's your gender", StringComparison.CurrentCultureIgnoreCase) >= 0))
            {
                output += "I'm a boy!";
            }
            else if (((question.IndexOf("who is ribs", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
                    || ((question.IndexOf("who's ribs", StringComparison.CurrentCultureIgnoreCase) >= 0) == true))
            {
                output += "Some weeb that created me";
            }
            else if ((question.IndexOf("are you gay", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "You are gay";
            }
            else if ((question.IndexOf("vtubers", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "idk, vtubers kinda bad ngl";

            }
            else if ((question.IndexOf("city hunter", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "no idea, i only know that city hunter is bad";
            }
            else if ((question.IndexOf("ryo saeba", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "idk, i think Ryo Saeba is a pervert";
            }
            else if ((question.IndexOf("what is the definition of insanity", StringComparison.CurrentCultureIgnoreCase) >= 0) == true
                || (question.IndexOf("what's the definition of insanity", StringComparison.CurrentCultureIgnoreCase) >= 0) == true
                || (question.IndexOf("did i ever tell you what the definition of insanity is", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output = "Mentioning one Far Cry game, over and over again";
            }
            else
            {
                output = answers[random.Next(0, answers.Length)];
            }
            await Context.Channel.TriggerTypingAsync();
            await ReplyAsync(output.ToString());
        }

    }
}
