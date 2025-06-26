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
   
    public partial class MainWindow : Window
    {
        private ChatbotEngine bot;
        private bool isNameSet = false;

        public MainWindow()
        {
            InitializeComponent();
            bot = new ChatbotEngine(msg => AppendToChat(msg));

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

            AppendToChat($"You: {userText}", true);

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


        private void AppendToChat(string message, bool isUser = false)
        {
            Dispatcher.Invoke(() =>
            {
                var bubble = CreateChatBubble(message, isUser);
                ChatPanel.Children.Add(bubble);

               
                ChatScrollViewer.ScrollToEnd();
            });
        }

        private Border CreateChatBubble(string message, bool isUser)
        {
            var textBlock = new TextBlock
            {
                
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Foreground = Brushes.Black,
                MaxWidth = 500,
                Margin = new Thickness(10)
            };

            var bubble = new Border
            {
                Background = isUser ? Brushes.LightBlue : Brushes.LightGray,
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(10),
                Margin = new Thickness(10),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Child = textBlock,
                MaxWidth = 520
            };

            return bubble;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


    }
}
