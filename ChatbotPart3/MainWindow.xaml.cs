using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatbotPart3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChatbotEngine bot;
        private bool isNameSet = false;

        public MainWindow()
        {
            InitializeComponent();
            bot = new ChatbotEngine(AppendToChat);
            bot.Initialize();
            
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            ProcessUserInput();
        }

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessUserInput();
            }
        }

        private void ProcessUserInput()
        {
            string userText = UserInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(userText)) return;

            AppendToChat($"You: {userText}");

            if (!isNameSet)
            {
                bot.SetUserName(userText);
                isNameSet = true;
            }
            else
            {
                bot.ProcessInput(userText);
            }

            UserInput.Clear();
        }

       
           private void AppendToChat(string message)
        {
            Dispatcher.Invoke(() =>
            {
                ChatHistory.Text += message;  // use += to append live text
                ChatHistory.ScrollToEnd();
            });
        }


    
    }
}
