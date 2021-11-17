using System;
using System.Collections.Generic;
using System.Text;

namespace Bibs_Discord_dotNET.Commons.Structs
{
    public class Variables
    {
        public static string[] pottyMouth = new string[] {
                "faggot",
                "faggots",
                "nigger",
                "niggers",
                "nigga",
                "niggas",
                "fag",
                "fags",
                "faggy",
                "niglet",
                "niglets",
                "towelhead",
                "towelheads",
                "negro",
                "negros",
                "chink",
                "chinks",
                "zipperhead",
                "zipperheads",
                "beaner",
                "beaners",
                "coon",
                "coons",
                "kike",
                "kikes",
                "jewboy",
                "jewboys"
            };

        public static Dictionary<string, string> lelic = new Dictionary<string, string> 
        {
            {"fuck", "garden"},
            {"shit","pudding"},
            {"cunt","flower"},
            {"nigger", "buddy"},
            {"nigga", "buddy"},
            {"faggot", "nice person"}
        };
        public static List<string> compliments = new List<string> {
            {"your're cute"},
            {"you're so cute"},
            {"you are so cute" },
            {"you're a cutie" },
            {"short and cute" },
            {"i like you" },
            {"i love you" },
            {"marry me" },
            {"do you like being a girl" },
            {"go on a date" },
            {"best girl" }
        };
        

        public static Random randomRepliesToCompliments = new Random();
        public static string[] response =
        {
                "Wh-Wha?!",
                "Wh-What are you saying?",
                "?!!",
                "Don't say those things out loud!",
                "Hmph",
                "Sheesh!",
                "Can you not?",
                "Hehhh!?",
                "...",
                "G-Geez!"
            };
        public static Random randomRepliesToInsults = new Random();
        public static string[] comebacks =
        {
                "If your opinion mattered to me, I'd be offended",
                "I'd punch you, but that's animal abuse",
                "I could eat a bowl of alphabet soup and shit out a smarter statement than what you just said",
                "Is your ass jealous of the amount of crap that just came out of your mouth?",
                "Oh yeah? The jerk store called and they ran out of you."
            };
    }
}
