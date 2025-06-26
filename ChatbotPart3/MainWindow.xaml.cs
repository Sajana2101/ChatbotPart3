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
// Namespace for the chatbot application
namespace ChatbotPart3
{
    // Main window class for the WPF application
    public partial class MainWindow : Window
    {
        // Instance of the chatbot logic
        private ChatbotEngine bot;
        // Tracks if user's name is set
        private bool isNameSet = false;
        // Constructor: Initializes UI and chatbot logic
        public MainWindow()
        {
            // Loads the XAML UI
            InitializeComponent();
            // Injects response handler
            bot = new ChatbotEngine(msg => AppendToChat(msg));
            // Initializes the bot (e.g., loading data)
            bot.Initialize();
            
        }
        // Triggered when the Send button is clicked
        private void Send_Click(object sender, RoutedEventArgs e)
        {
            ProcessUserInput();
        }
        // Allows pressing Enter to send message
        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessUserInput();
            }
        }
        // Handles and sends the user's input to the chatbot
        private void ProcessUserInput()
        {
            string userText = UserInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(userText)) return;
            // Show user's message
            AppendToChat($"You: {userText}", true);

            if (!isNameSet)
            {
                // Set name on first input
                bot.SetUserName(userText);
                isNameSet = true;
            }
            else
            {
                // Send input to bot for processing
                bot.ProcessInput(userText);
            }
            // Clear input box
            UserInput.Clear();
        }

        // Displays a message in the chat UI
        private void AppendToChat(string message, bool isUser = false)
        {
            Dispatcher.Invoke(() =>
            {
                // Create bubble style
                var bubble = CreateChatBubble(message, isUser);
                // Add to UI
                ChatPanel.Children.Add(bubble);

                // Auto-scroll to bottom
                ChatScrollViewer.ScrollToEnd();
            });
        }
        // Creates a styled chat bubble for the message
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
                // Different color for user/bot
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
        // Handles exit button click to close app
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


    }
}
