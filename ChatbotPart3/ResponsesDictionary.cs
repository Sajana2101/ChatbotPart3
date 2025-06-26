using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotPart3
{
    //class to hold the responses to cybersecurity questions
    public static class ResponsesDictionary
    {
        public static readonly Dictionary<string, string[]> Data = new Dictionary<string, string[]>
        {
            { "phishing", new[] {
                "Phishing is when attackers trick you into giving personal info, often through fake emails.",
                "Phishing attacks often mimic trusted sources. Always verify links before clicking.",
                "Avoid phishing by checking sender details and not clicking unknown links.",
                "Phishing can steal passwords and credit card info. Be skeptical of urgent messages.",
                "Always hover over links to check authenticity before clicking in suspicious emails."
            }},
            { "malware", new[] {
                "Malware is software designed to harm or exploit devices and networks.",
                "Antivirus programs help detect and remove malware threats.",
                "Malware includes viruses, worms, Trojans, ransomware, and spyware.",
                "Keep your OS and apps updated to prevent malware infections.",
                "Avoid downloading unknown attachments to reduce malware risk."
            }},
            { "password", new[] {
                "Use long, complex passwords and avoid reusing them.",
                "Consider using a password manager to store secure passwords.",
                "Never share your passwords with anyone.",
                "Two-factor authentication adds an extra layer of password security.",
                "Avoid using personal info like birthdays in passwords."
            }},
            { "cybersecurity", new[] {
                "Cybersecurity is about protecting systems, networks, and data from digital attacks.",
                "Strong cybersecurity practices include firewalls, encryption, and user awareness.",
                "Good cybersecurity habits can protect your identity and data online.",
                "Cybersecurity is a shared responsibility – always think before you click.",
                "Education is key to staying safe in today’s digital world."
            }},
            { "virus", new[] {
                "Computer viruses replicate and spread, often causing data loss or corruption.",
                "Viruses can come through infected files, USBs, or websites.",
                "Install antivirus software to help prevent and remove computer viruses.",
                "Email attachments are a common way viruses are spread.",
                "Avoid pirated software – it can contain hidden viruses."
            }},
            { "browsing", new[] {
                "Use HTTPS websites for safer browsing.",
                "Private/incognito mode doesn’t make you invisible, but avoids storing history.",
                "Be cautious of pop-ups and fake download buttons.",
                "Avoid entering sensitive info on public Wi-Fi without a VPN.",
                "Browser extensions can be risky — only install from trusted sources."
            }},
            { "ransomware", new[] {
                "Ransomware locks or encrypts your files and demands payment for access.",
                "Never pay a ransom — it doesn’t guarantee file recovery and encourages more attacks.",
                "Keep backups of important data to recover from ransomware attacks.",
                "Ransomware often spreads through phishing emails and malicious downloads.",
                "Use up-to-date antivirus software to detect and block ransomware."
            }},
            { "social engineering", new[] {
                "Social engineering manipulates people into giving up confidential info.",
                "Attackers often pretend to be trusted individuals or authorities.",
                "Be cautious of unexpected calls or requests for sensitive data.",
                "Always verify someone's identity before sharing personal info.",
                "Security awareness can help prevent social engineering attacks."
            }},
            { "updates", new[] {
                "Regular updates patch security flaws that attackers exploit.",
                "Enable automatic updates for your operating system and apps.",
                "Outdated software is a common gateway for cyberattacks.",
                "Check for updates frequently — especially for browsers and plugins.",
                "Security updates are critical for staying protected online."
            }},
            { "wifi", new[] {
                "Public Wi-Fi can be insecure — avoid entering sensitive info on it.",
                "Use a VPN to protect your data on public wireless networks.",
                "Always change default router passwords at home.",
                "Secure your home Wi-Fi with WPA3 encryption if possible.",
                "Limit the number of devices allowed to connect to your network."
            }},
            { "vpn", new[] {
                "A VPN encrypts your internet traffic and hides your IP address for privacy.",
                "Use a VPN on public Wi-Fi to protect your personal data.",
                "VPNs can help bypass geographic restrictions safely and securely.",
                "Not all VPNs are equal — choose a reputable, no-log provider.",
                "A VPN adds a layer of security, especially on unsecured networks."
            }},
            { "firewall", new[] {
                "Firewalls monitor and control incoming/outgoing network traffic.",
                "Use both hardware and software firewalls for better protection.",
                "A firewall helps block unauthorized access to your system.",
                "Make sure your firewall is enabled and properly configured.",
                "Firewalls act as a gatekeeper between your device and the internet."
            }},
            { "defender", new[] {
                "Microsoft Defender is a built-in antivirus for Windows.",
                "Defender provides real-time protection and threat detection.",
                "Keep Defender updated to ensure it catches the latest threats.",
                "Use Defender alongside a firewall for stronger security.",
                "Windows Defender SmartScreen can help block malicious websites."
            }},
            { "encryption", new[] {
                "Encryption scrambles data so only authorized users can read it.",
                "Always use encrypted messaging apps for private conversations.",
                "End-to-end encryption ensures only the sender and receiver can see the message.",
                "Encrypted websites use HTTPS — avoid HTTP for sensitive transactions.",
                "Encryption protects your files and communications from prying eyes."
            }},
            { "online scams", new[] {
              "Online scams try to trick you into giving away money or personal info.",
              "Be cautious of unsolicited emails or messages asking for sensitive data.",
              "Never click on suspicious links or download attachments from unknown sources.",
              "Verify the sender’s identity before sharing any personal or financial information.",
              "Use multi-factor authentication to protect your accounts from scam attempts."
}}

        };
    }
}
