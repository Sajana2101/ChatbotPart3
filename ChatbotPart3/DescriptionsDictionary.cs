using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotPart3
{
    //class to hold descriptions dictionary for definitions
    class DescriptionsDictionary
    {
        public static readonly Dictionary<string, string> Data = new Dictionary<string, string>
        {

     { "malware", "Malware is software specifically designed to disrupt, damage, or gain unauthorized access to a computer system." },
    { "phishing", "Phishing is a type of online scam where attackers trick users into revealing personal information by pretending to be trustworthy entities." },
    { "ransomware", "Ransomware is a type of malware that locks or encrypts a victim's data and demands payment to restore access." },
    { "viruses", "A computer virus is a type of malware that, when executed, replicates by inserting copies of itself into other programs." },
    { "cybersecurity", "Cybersecurity refers to the practice of protecting systems, networks, and programs from digital attacks." },
    { "encryption", "Encryption is the process of converting data into a coded form to prevent unauthorized access." },
    { "defender", "Microsoft Defender is a built-in security program in Windows that provides real-time protection against viruses and other threats." },
    { "firewall", "A firewall is a security system that monitors and controls incoming and outgoing network traffic based on predetermined rules." },
    { "vpn", "A VPN (Virtual Private Network) encrypts your internet connection to secure data and protect your online identity." },
    { "updates", "Updates include patches and improvements that fix security vulnerabilities and keep your system protected." },
    { "social engineering", "Social engineering is a manipulation technique that exploits human error to gain private information or access." },
    { "safe browsing", "Safe browsing involves practices like using HTTPS websites, avoiding suspicious links, and not sharing personal information online." },
    { "password", "A password is a secret string of characters used to authenticate a user and protect access to systems or data." },
    {"online scams", "Online scams are deceptive schemes carried out over the internet to trick people into giving away money, personal information, or access to their accounts." }
};
    }
}
