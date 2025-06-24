using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotPart3
{
    class SentimentsDictionary
    {

        public static readonly Dictionary<string, string> Data = new Dictionary<string, string>
        {

    { "worried", "It's completely understandable to feel that way. Would you like me to provide you with some tips on that topic?" },
    { "curious", "Curiosity is great! I can help you learn more about this topic." },
    { "excited", "I'm glad you're excited! Let's dive into some interesting info." },
    { "sad", "I'm sorry to hear you're feeling sad about this. Let's work through this together" },
    { "angry", "Feeling angry is natural with these threats. Let's find ways to tackle them." },
    { "frustrated", "I understand feeling frustrated. I'm here to help with any questions you have." },
    { "scared", "It's okay to feel scared. Knowledge is your best defense against cyber threats." },
    { "concerned", "Your concern is valid. I can share tips to help you stay safe." },
    { "anxious", "Feeling anxious is normal. Let's work through your worries together." },
    { "overwhelmed","Feeling this way is comepletely normal, let me share some a tip to help you feel better" },
     { "Stressed","I understand your frustration, let me help you through this with a helpful tip" }
};
    }
}
