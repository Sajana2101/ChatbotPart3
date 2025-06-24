using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotPart3
{
    class FollowUpsDictionary
    {
        public static readonly Dictionary<string, string[]> Data = new Dictionary<string, string[]>
        {
            { "phishing", new[] {
                "Have you ever received a suspicious email or message?",
                "Would you like tips on how to identify fake emails?",
                "Would you like another tip?",
                "Would you like to learn more?",
                "Would you like me to expand on this topic?"
            }},
            { "malware", new[] {
                "Are you currently using antivirus software?",
                "Would you like to learn how malware spreads?",
                "Would you like another tip?",
                "Would you like to learn more?",
                 "Would you like me to expand on this topic?"
            }},
            { "password", new[] {
                "Do you know if your passwords have ever been leaked?",
                "Would you like a tip on creating strong passwords?",
                 "Would you like another tip?",
                "Would you like to learn more?",
                 "Would you like me to expand on this topic?"
            }},
            { "cybersecurity", new[] {
                "Are you using a firewall or antivirus at home?",
                "Want to hear more about keeping your network secure?",
                 "Would you like another tip?",
                "Would you like to learn more?",
                 "Would you like me to expand on this topic?"
            }},
            { "virus", new[] {
                "Have you ever had a virus on your computer?",
                "Would you like to know how to remove one safely?",
                 "Would you like another tip?",
                "Would you like to learn more?",
                 "Would you like me to expand on this topic?"
            }},
            { "browsing", new[] {
                "Do you use a VPN when browsing on public Wi-Fi?",
                "Would you like to hear tips for safe online shopping?",
                 "Would you like another tip?",
                "Would you like to learn more?",
                 "Would you like me to expand on this topic?"
            }},



            { "ransomware", new[] {
                "Do you know how to back up your files securely?",
                "Want to hear how ransomware spreads and how to avoid it?",
                "Would you like another tip?",
                "Would you like to learn more?",
                "Would you like me to expand on this topic?"
                }
            },

            { "social engineering", new[] {
                "Have you ever been tricked into giving personal info?",
                "Would you like tips on spotting manipulation techniques?",
                "Would you like another tip?",
                "Would you like to learn more?",
                "Would you like me to expand on this topic?"
                }
            },
            { "updates", new[] {
                "Do you keep your software and OS up to date?",
                "Would you like to know why updates are so important?",
                "Would you like another tip?",
                "Would you like to learn more?",
                "Would you like me to expand on this topic?"
                }
            },

            { "wifi", new[] {
                "Do you know if your home Wi-Fi is secured properly?",
                "Would you like tips on staying safe on public Wi-Fi?",
                "Would you like another tip?",
                "Would you like to learn more?",
                 "Would you like me to expand on this topic?"
                }
            },

            {   "vpn", new[] {
                 "Are you currently using a VPN for your devices?",
                 "Would you like to know which VPNs are most secure?",
                 "Would you like another tip?",
                "Would you like to learn more?",
                 "Would you like me to expand on this topic?"
                }
            },

            { "firewall", new[] {
                "Do you know if your firewall is turned on and configured properly?",
                "Would you like tips on setting up a personal firewall?",
                 "Would you like another tip?",
                "Would you like to learn more?",
                 "Would you like me to expand on this topic?"
                }
            },

             { "defender", new[] {
                  "Do you use Windows Defender for real-time protection?",
                  "Would you like to know how to run a quick scan with Defender?" ,
                  "Would you like another tip?",
                  "Would you like to learn more?",
                 "Would you like me to expand on this topic?"

                }
            },
            { "encryption", new[] {
                 "Do you use encrypted apps like Signal or WhatsApp?",
                 "Would you like to learn how to encrypt your files or emails?",
                 "Would you like another tip?",
                "Would you like to learn more?",
                 "Would you like me to expand on this topic?"
                }
            }
        };
    }
}
