﻿using Bibs_Discord_dotNET.Preconditions;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Bibs_Discord.NET.Modules
{
    public class Answer: ModuleBase<SocketCommandContext>
    {

        [Command("answer")]
        [Summary("Answers a yes no question, much like the magic eightball")]
        [Cooldown(5)]
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
            //there's probably better ways of implementing this but I'm not at that level yet 
            //maybe one day I'll be able to use regular expressions without summoning the ancient one
            //but right now this is the best that I can do
            //yknow i could have used switch cases... Ahh well...
            //also cringe
            //very cringe actually
            //ahh man, this really be a bruh moment
            if ((question.IndexOf("are you cute", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "D-Don't call me cute!";
            }
            else if ((question.IndexOf("how old are you", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "Too young for you!";
            }
            else if ((question.IndexOf("are you retarded", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "You're retarded";
            }
            else if (((question.IndexOf("are you a girl", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
                || (question.IndexOf("are you boy or a girl", StringComparison.CurrentCultureIgnoreCase) >= 0) == true
                || (question.IndexOf("what is your gender", StringComparison.CurrentCultureIgnoreCase) >= 0) == true
                || (question.IndexOf("you're a girl", StringComparison.CurrentCultureIgnoreCase) >= 0) == true
                || (question.IndexOf("you are a girl", StringComparison.CurrentCultureIgnoreCase) >= 0) == true
                || (question.IndexOf("what's your gender", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "I'm a boy!";
            }
            else if (((question.IndexOf("who is ribs", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
                    || ((question.IndexOf("who's ribs", StringComparison.CurrentCultureIgnoreCase) >= 0) == true))
            {
                output += "Some weeb that created me";
            }
            else if ((question.IndexOf("go on a date with me", StringComparison.CurrentCultureIgnoreCase) >= 0) == true
                || (question.IndexOf("date me", StringComparison.CurrentCultureIgnoreCase) >= 0) == true
                || (question.IndexOf("go out with me", StringComparison.CurrentCultureIgnoreCase) >= 0) == true
                || (question.IndexOf("marry me", StringComparison.CurrentCultureIgnoreCase) >= 0) == true
                || (question.IndexOf("be my gf", StringComparison.CurrentCultureIgnoreCase) >= 0) == true
                || (question.IndexOf("be my girl", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "N-Never!";
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
            else if ((question.IndexOf("horny", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "You are horny";
            }
            else if ((question.IndexOf("sex", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "No";
            }
            else if ((question.IndexOf("how are you", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "Fine til I met you, I guess";
            }
            else if ((question.IndexOf("who are you", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                if (Context.User.Id == 225325643246600193)
                {
                    output += "You're kidding right? ***YOU*** made me!";
                }
                else 
                {
                    output += "Who are you? Why are you asking me this?";
                }
                
            }
            else if ((question.IndexOf("who is the best person here", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "Probably not you";
            }
            else if ((question.IndexOf("do you hate me", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "It depends...";
            }
            else if ((question.IndexOf("do you love me", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "Wh-Why are you asking me this?!";
            }
            else if ((question.IndexOf("where", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "That's not a yes/no question, find that out yourself!";
            }
            else if ((question.IndexOf("is ribs nice to you", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "Very!";
            }
            else if ((question.IndexOf("do you hate ribs", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "I'm programmed to say no";
            }
            else if ((question.IndexOf("how often do you reload", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "199/200";
            }
            else if ((question.IndexOf("who made you", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "Ribs#8205";
            }
            else if ((question.IndexOf("do you know the meaning behind the numbers", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "Mason does";
            }
            else if ((question.IndexOf("numbers", StringComparison.CurrentCultureIgnoreCase) >= 0) == true 
                && (question.IndexOf("mean", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "It means your mom";
            }
            else if ((question.IndexOf("go on a date with me", StringComparison.CurrentCultureIgnoreCase) >= 0) == true
                || (question.IndexOf("date me", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "N-No!";
            }
            else if ((question.IndexOf("can i take you out to dinner", StringComparison.CurrentCultureIgnoreCase) >= 0) == true)
            {
                output += "You can't!";
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
