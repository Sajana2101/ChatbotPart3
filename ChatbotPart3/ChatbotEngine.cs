
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatbotPart3
{
    public class ChatbotEngine
    { 
        //used to send output messages back to the UI or caller
        private readonly Action<string> output;
        // List to store the user's added task
        private List<TaskItem> userTasks = new List<TaskItem>();
        //timer to check and trigger task reminders
        private System.Windows.Threading.DispatcherTimer reminderTimer;
        //list to keep track of activity log entires for user actions
        private List<string> activityLog = new List<string>();
        //index to tracj whcih portion of the activity log is currently displayed 
        private int activityDisplayIndex = 0;

        //random number generator used for selecting random tips, greetings , or quiz questions
        private readonly Random rnd = new Random();
        //flag for the quiz to indicate if user is taking the quiz
        private bool isInQuiz = false;
        //track the current question index in the quiz
        private int currentQuestionIndex = 0;
        //counts the number of correctly answered quiz questions
        private int correctAnswers = 0;
        //holds the list of quiz questions 
        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>();
        //list of topics the user has shown interest in

        private List<string> rememberedTopics = new List<string>();
        private string userInterestTopic = "";
        // Counter tracking how many user inputs have been processed, for timed prompts or reminders
        private int userPromptCounter = 0;

        // Flag indicating if the user has expressed interest in a topic
        private bool userExpressedInterest = false;
        // Index used to track progress through follow-up questions or explanations
        private int followUpIndex = 0;
        // Flag indicating whether the chatbot is currently in an ongoing conversation state
        private bool inConversation = false;

        private string currentTopic = null;
        private string lastFollowUpTopic = null;
        private bool awaitingFollowUpResponse = false;
        private int inputCounter = 0;
        private string chatHistoryPath = "chathistory.txt";
        private string username = "Friend";

        public ChatbotEngine(Action<string> outputCallback)
        {
            // Save the output callback delegate, which allows the bot to send messages back to the UI or caller.
            output = outputCallback;
            // Initialize the timer that will check for any task reminders to notify the user.
            InitializeReminderTimer();
        }



        public void Initialize()

        // When the chatbot starts, run the welcome sequence to greet the user.
        {
            RunWelcomeSequence();

        }


        //SetUserName(name) saves the user’s name and sends a
        //personalized greeting plus a single message listing all topics and commands the user can try.
        public void SetUserName(string name)
        {
            username = name;
            TypeResponse($"Hello {username}, nice to meet you!");
            TypeResponse("Let's delve into the world of cybersecurity where you can learn how to beat those pesky cybercriminals!");
            TypeResponse("Here are some things you can ask me about:" +

          "\n- Cybersecurity" +
          "\n  Virus" +
          "\n- Phishing " +
          "\n- Malware." +
          "\n- Password Saftey " +
          "\n- Safe online browsing " +
          "\n- Ransomware " +
          "\n- Social Engineering " +
          "\n- Security Updates " +
          "\n- Wifi " +
          "\n- VPN " +
          "\n- Firewalls " +
          "\n- Online Scams"+
          "\n- Encryption " +
          "\n- Windows Defender " +
          "\n- Add task"+
          "\n- View Tasks"+
          "\n- Add reminders to tasks"+
          "\n- Update task status to complete"+
          "\n- Delete tasks"+
          "\n- View Activity log"+

          "\n- Exit");
        }
        private TaskItem lastAddedTask = null;              // Stores the most recently added task
        private bool awaitingReminderConfirmation = false; // Tracks if the bot is waiting for user to confirm setting a reminder
        private TaskItem awaitingReminderTimeForTask = null; // Stores the task that is waiting for a reminder time input from the user



        public void ProcessInput(string input)
        {

            // If currently in quiz mode, process quiz answer and exit early

            if (isInQuiz)
            {
                ProcessQuizAnswer(input.ToLower().Trim());
                return;
            }
            // Normalize input to lowercase
            string loweredInput = input.ToLower();
            // Log input for history or analysis
            LogUserInput(loweredInput);
            // Check if input contains special keywords
            CheckForKeywords(loweredInput);
            // Increment total input counter
            inputCounter++;
            // Increment prompt counter for interest tracking
            userPromptCounter++;
            // Handle user's reply to reminder confirmation question
            if (awaitingReminderConfirmation)
            {
                // User wants to add reminder
                if (loweredInput.Contains("yes"))
                {
                    
                    TypeResponse("Great! Please tell me when to remind you (e.g. 'in 3 days', 'tomorrow at 5pm').");
                    // Stop waiting for confirmation
                    awaitingReminderConfirmation = false;
                    // Exit to wait for reminder time input
                    awaitingReminderTimeForTask = lastAddedTask;
                    return;
                }
                // User does not want reminder now
                else
                {
                    TypeResponse("No problem! If you want to set a reminder later, just ask me.");
                    lastAddedTask = null;
                    awaitingReminderConfirmation = false;
                    return;
                }
            }

            // Handle the actual reminder time input after confirmation
            if (awaitingReminderTimeForTask != null)
            {
                string timePart = loweredInput;
                // Try to parse relative time e.g. "in 2 days"
                if (TryParseTime(timePart, out TimeSpan offset))
                {
                    awaitingReminderTimeForTask.ReminderTime = DateTime.Now.Add(offset);
                    TypeResponse($"Got it! I'll remind you about \"{awaitingReminderTimeForTask.Title}\" in {FormatTimeSpan(offset)}.");
                    LogAction($"Reminder set for task '{awaitingReminderTimeForTask.Title}' in {FormatTimeSpan(offset)}.");
                    // Clear reminder waiting state
                    awaitingReminderTimeForTask = null;
                }
                // Or try to parse specific date/time
                else if (DateTime.TryParse(timePart, out DateTime specificDate))
                {
                    awaitingReminderTimeForTask.ReminderTime = specificDate;
                    TypeResponse($"Got it! I'll remind you about \"{awaitingReminderTimeForTask.Title}\" at {specificDate}.");
                    LogAction($"Reminder set for task '{awaitingReminderTimeForTask.Title}' at {specificDate}.");
                    awaitingReminderTimeForTask = null;
                }
                else// Could not parse time
                {
                    TypeResponse("Sorry, I couldn't understand that time. Please try again.");
                }
                return; // Wait for next input after handling reminder time
            }

            // Add a new task if input starts with "add task"
            if (loweredInput.StartsWith("add task"))
            {
                // Extract task description text after "add task"
                string taskDesc = input.Substring(input.ToLower().IndexOf("add task") + 8).Trim();

                // Remove leading '-' if any
                if (taskDesc.StartsWith("-"))
                    taskDesc = taskDesc.Substring(1).Trim();
                // If task description empty, ask user to provide one
                if (string.IsNullOrWhiteSpace(taskDesc))
                {
                    TypeResponse("Please provide a task description after 'add task'.");
                    return;
                }
                // Create new TaskItem with title capped at 30 characters
                var task = new TaskItem
                {
                    Title = taskDesc.Length > 30 ? taskDesc.Substring(0, 30) : taskDesc, 
                    Description = taskDesc,
                    IsComplete = false
                };
                // Add task to user's task list
                userTasks.Add(task);
                // Store task for reminder confirmation
                lastAddedTask = task;
                // Ask if user wants reminder now
                awaitingReminderConfirmation = true;
                TypeResponse($"Task added with the description \"{task.Description}\". Would you like a reminder?");
                LogAction($"Task '{task.Title}' added.");
                return;
            }
            // Show activity log commands
            if (loweredInput.Contains("activity log") || loweredInput.Contains("what have you done") || loweredInput.Contains("activity history") || loweredInput.Contains("log") || loweredInput.StartsWith("show me") || loweredInput.Contains("activities"))
            {
                ShowActivityLog();
                return;
            }

            else if (loweredInput.Contains("show more"))
            {
                ShowMoreActivity();
                return;
            }
            // Start quiz if user mentions quiz/game/play
            if (loweredInput.Contains("quiz") || loweredInput.Contains("play") || loweredInput.Contains("game"))
            {
                StartQuiz(username);
                return;
            }
            // Exit command
            if (loweredInput.Contains("exit"))
            {
                TypeResponse($"Goodbye {username}! Stay safe online.");
                return;
            }

            // Periodically prompt user based on interest
            if (userPromptCounter >= 3 && userExpressedInterest && !string.IsNullOrEmpty(userInterestTopic))
            {
                string randomTopic = rememberedTopics[rnd.Next(rememberedTopics.Count)];
                TypeResponse($"As someone who is curious about {randomTopic}, this is particularly important.");
                userPromptCounter = 0;
            }

            // Add task shortcut "add task to ..."
            if (loweredInput.StartsWith("add task to "))
            {
                string title = input.Substring(12).Trim();
                if (!string.IsNullOrWhiteSpace(title))
                {
                    var task = new TaskItem
                    {
                        Title = title,
                        Description = "No description provided.",
                        IsComplete = false
                    };
                    userTasks.Add(task);
                    TypeResponse($"✅ Task added: \"{task.Title}\". Would you like to set a reminder for this task?");
                    LogAction($"Task '{task.Title}' added.");
                }
                else
                {
                    TypeResponse("Please provide a task title after 'add task to'.");
                }
                return;
            }
            // Reminder commands like "remind me to ..."

            if (loweredInput.StartsWith("remind me to "))
            {
                string remainder = input.Substring(13).Trim();
                // Split remainder on keywords to separate task title from time info
                string[] parts = remainder.Split(new[] { " in ", " at ", " tomorrow", " tonight" }, StringSplitOptions.None);
                // Task title assumed before time phrase
                string title = parts[0].Trim();
                var task = userTasks.FirstOrDefault(t => t.Title.ToLower() == title.ToLower());

                // If task not found, create new one with no description
                if (task == null)
                {
                    task = new TaskItem { Title = title, Description = "No description provided.", IsComplete = false };
                    userTasks.Add(task);
                    LogAction($"Task '{task.Title}' added via reminder.");
                }

                DateTime reminderTime = DateTime.Now;
                // Set reminder time based on keywords
                if (loweredInput.Contains("tonight"))
                {
                    reminderTime = DateTime.Today.AddHours(20); 
                }
                else if (loweredInput.Contains("tomorrow"))
                {
                    reminderTime = DateTime.Now.AddDays(1);

                }
                else if (loweredInput.Contains(" in ") || loweredInput.Contains(" at "))
                {
                    string timePart;
                    if (loweredInput.Contains(" in "))
                    {
                        string[] timeSplitParts = loweredInput.Split(new[] { " in " }, StringSplitOptions.None);
                        timePart = timeSplitParts.Length > 1 ? timeSplitParts[1] : "";
                    }
                    else if (loweredInput.Contains(" at "))
                    {
                        string[] timeSplitParts = loweredInput.Split(new[] { " at " }, StringSplitOptions.None);
                        timePart = timeSplitParts.Length > 1 ? timeSplitParts[1] : "";
                    }
                    else
                    {
                        timePart = "";
                    }

                    if (TryParseTime(timePart.Trim(), out TimeSpan offset))
                    {
                        reminderTime = DateTime.Now.Add(offset);
                    }
                    else if (DateTime.TryParse(timePart.Trim(), out DateTime specific))
                    {
                        reminderTime = specific;
                    }
                }

                task.ReminderTime = reminderTime;
                TypeResponse($"⏰ Reminder set for task \"{task.Title}\" at {reminderTime}.");
                //save activity 
                LogAction($"Reminder set for task '{task.Title}' at {reminderTime}.");
                return;
            }

            
            if (loweredInput.Contains("task"))
            {
                if (loweredInput.StartsWith("add task"))
                {
                    // Handle adding a task with the format "add task Title | Description "
                    string taskData = input.Substring(input.ToLower().IndexOf("add task") + 8).Trim();
                   // Split by '|' to separate Title, Description
                    string[] parts = taskData.Split('|');
                    if (parts.Length < 2)
                    {
                        TypeResponse("Please enter the task like this:\nadd task Title| description");
                    }
                    else
                    {
                        var task = new TaskItem
                        {
                            Title = parts[0].Trim(),
                            Description = parts[1].Trim(),
                            IsComplete = false
                        };
                      

                        if (parts.Length >= 3)
                        {
                            string timePart = parts[2].Trim().ToLower();
                            if (timePart.StartsWith("remind me in "))
                            {
                                string duration = timePart.Substring("remind me in ".Length);
                                if (TryParseTime(duration, out TimeSpan offset))
                                {
                                    task.ReminderTime = DateTime.Now.Add(offset);
                                }
                            }
                            else if (DateTime.TryParse(timePart, out DateTime specificDate))
                            {
                                task.ReminderTime = specificDate;
                            }
                        }

                        userTasks.Add(task);
                        TypeResponse($"Task \"{task.Title}\" added.");
                        LogAction($"Task '{task.Title}' added.");
                        AskFollowUp();
                    }
                    return;
                }
                // Handle showing/listing tasks
                else if (loweredInput.Contains("show tasks") || loweredInput.Contains("list tasks") || loweredInput.Contains("view tasks"))
                {
                    
    
                    if (userTasks.Count == 0)
                    {
                        TypeResponse("You have no tasks.");
                    }
                    else
                    {
                        // Loop through tasks and display details and status
                        foreach (var task in userTasks)
                        {
                            string status = task.IsComplete ? "✅ Completed" : "❗ Incomplete";
                            string reminder = task.ReminderTime.HasValue ? $"(Reminder: {task.ReminderTime})" : "";
                            TypeResponse($"• {task.Title} - {task.Description} {reminder} - {status}");
                        }
                        LogAction("Viewed tasks");
                        AskFollowUp();
                    }
                    return;
                }
                // Mark a task as complete using phrases "complete task", "mark task", or "task is"

                else if (loweredInput.StartsWith("complete task ") ||
         loweredInput.StartsWith("mark task ") ||
         loweredInput.StartsWith("task is "))
                {
                    string title = null;
                    // Extract task title based on which phrase user used
                    if (loweredInput.StartsWith("complete task "))
                        title = loweredInput.Substring("complete task ".Length).Trim();
                    else if (loweredInput.StartsWith("mark task "))
                        title = loweredInput.Substring("mark task ".Length).Trim();
                    else if (loweredInput.StartsWith("task is "))
                        title = loweredInput.Substring("task is ".Length).Trim();

                   
                    title = title.Replace("as complete", "")
                                 .Replace("completed", "")
                                 .Replace("done", "")
                                 .Trim();

                    if (!string.IsNullOrEmpty(title))
                    {
                        // Search for task whose title contains the given title (case-insensitive)
                        var task = userTasks.FirstOrDefault(t =>
                            t.Title != null &&
                            t.Title.ToLower().Contains(title.ToLower()));

                        if (task != null)
                        {
                            // Mark as complete
                            task.IsComplete = true;
                            TypeResponse($"✅ Task \"{task.Title}\" marked as complete.");
                            LogAction($"Task '{task.Title}' marked as complete.");
                            AskFollowUp();
                        }
                        else
                        {
                            TypeResponse($"❌ Task matching \"{title}\" not found.");
                        }
                    }
                    else
                    {
                        TypeResponse("⚠️ Please specify which task you'd like to mark as complete.");
                    }

                    return;
                }
                // Delete a task with "delete task" phrase
                else if (loweredInput.StartsWith("delete task ")|| loweredInput.Contains("delete task "))
                {
                    // Extract task title after "delete task"
                    string title = loweredInput.Replace("delete task ", "").Trim();
                    // Find exact matching task by title (case-insensitive)
                    var task = userTasks.Find(t => t.Title.ToLower() == title.ToLower());
                    if (task != null)

                    {
                        

                        userTasks.Remove(task);
                        TypeResponse($"Task \"{task.Title}\" deleted.");

                        LogAction($"Task '{task.Title}' deleted.");
                        AskFollowUp();

                    }
                    else
                    {
                        TypeResponse("Task not found.");
                    }
                    return;
                }
                // Set reminder for a task using phrases "remind", "remind me", or "set reminder"
                else if (loweredInput.StartsWith("remind") || loweredInput.StartsWith("remind me") || loweredInput.StartsWith("set reminder"))
                {
                    string remainder = input.ToLower();
                    // Remove the command phrase from start
                    if (remainder.StartsWith("set reminder"))
                        remainder = remainder.Substring("set reminder".Length).Trim();
                    else if (remainder.StartsWith("remind me"))
                        remainder = remainder.Substring("remind me".Length).Trim();
                    else if (remainder.StartsWith("remind"))
                        remainder = remainder.Substring("remind".Length).Trim();
                    // Find where the word "task" occurs to separate title and time
                    int taskIndex = remainder.IndexOf("task");
                    if (taskIndex == -1)
                    {
                        TypeResponse("Please specify the task name like: 'set reminder for task TaskName in 10 minutes'");
                        return;
                    }
                    // Extract part after "task"
                    string afterTask = remainder.Substring(taskIndex + 4).Trim();
                    // Try to split time from title using " in " or " at "
                    int inIndex = afterTask.IndexOf(" in ");
                    int atIndex = afterTask.IndexOf(" at ");

                    string title, timePart;
                    if (inIndex != -1)
                    {
                        title = afterTask.Substring(0, inIndex).Trim();
                        timePart = afterTask.Substring(inIndex + 4).Trim();
                    }
                    else if (atIndex != -1)
                    {
                        title = afterTask.Substring(0, atIndex).Trim();
                        timePart = afterTask.Substring(atIndex + 4).Trim();
                    }
                    else
                    {
                        TypeResponse("Please specify the reminder time like 'in 10 minutes' or 'at 15:00'.");
                        return;
                    }
                    // Find the task by exact title match (case-insensitive)
                    var task = userTasks.Find(t => t.Title.ToLower() == title.ToLower());
                    if (task == null)
                    {
                        TypeResponse($"I couldn't find a task named \"{title}\".");
                        return;
                    }
                    // Parse time and set reminder accordingly
                    if (timePart.Contains(":") || timePart.Contains("/"))
                    {
                        if (DateTime.TryParse(timePart, out DateTime specificDateTime))
                        {
                            task.ReminderTime = specificDateTime;
                            TypeResponse($"⏰ Reminder set for task \"{task.Title}\" at {specificDateTime}.");
                            AskFollowUp();
                            LogAction($"Reminder set for task '{task.Title}' at {specificDateTime}.");
                        }
                        else
                        {
                            TypeResponse("Invalid date/time format. Try using: 2025-06-26 15:30");
                        }
                    }
                    else if (TryParseTime(timePart, out TimeSpan offset))
                    {
                        DateTime reminderTime = DateTime.Now.Add(offset);
                        task.ReminderTime = reminderTime;
                        string humanReadableTime = FormatTimeSpan(offset);
                        TypeResponse($"⏰ Reminder set for task \"{task.Title}\" in {humanReadableTime}.");
                        AskFollowUp();
                        LogAction($"Reminder set for task '{task.Title}' in {humanReadableTime}.");
                    }
                    else
                    {
                        TypeResponse("Sorry, I couldn't understand the time duration. Try '10 minutes', '2 hours', or '1 day'.");
                    }

                    return;
                }
            }

            // Handle questions starting with "what is" or containing "definition"
            if (loweredInput.StartsWith("what is ") || loweredInput.Contains("definition"))
            {
                string possibleTopic = loweredInput.Replace("what is ", "").Trim();
                if (descriptions.ContainsKey(possibleTopic))
                {
                    // Provide definition/description
                    TypeResponse(descriptions[possibleTopic]);
                    // Offer related follow-up info
                    HandleFollowUp(possibleTopic);
                    return;
                }
                else
                {
                    TypeResponse("Hmm, I don't understand. Please check your spelling or ask about another cybersecurity topic!");
                    return;
                }
            }
            // Detect user sentiment and topic from input
            string detectedSentiment = DetectSentiment(loweredInput);
            string detectedTopic = DetectTopic(loweredInput);
            if (detectedSentiment != null && detectedTopic != null)
            {
                TypeResponse(sentiments[detectedSentiment]);

                // Prepare for follow-up conversation on detected topic
                lastFollowUpTopic = detectedTopic;
                awaitingFollowUpResponse = true;
                inConversation = true;
                followUpIndex = 0;

                TypeResponse($"Would you like to learn more about {detectedTopic}?");
                return;
            }
            // Handle user response to follow-up prompts
            if (awaitingFollowUpResponse)
            {
                if (loweredInput.Contains("yes") || loweredInput.Contains("confused") || loweredInput.Contains("explain"))
                {
                    // Provide more info on topic
                    RespondToTopic(lastFollowUpTopic);
                    return;
                }
                else if (loweredInput.Contains("nothing") || loweredInput.Contains("no"))
                {
                    TypeResponse("No problem! Let me know if you'd like to learn about something else.");
                    awaitingFollowUpResponse = false;
                    inConversation = false;
                    followUpIndex = 0;
                    return;
                }
            }
            // Handle requests for more explanation on current topic
            if (currentTopic != null && (loweredInput.Contains("explain") || loweredInput.Contains("more")))
            {
                RespondToTopic(currentTopic);
                return;
            }
            // Respond directly to detected topic
            if (detectedTopic != null)
            {
                RespondToTopic(detectedTopic);
                return;
            }
            // Fallback response if input not understood

            TypeResponse("I'm sorry, I don't understand that. Try asking about phishing, malware, or cybersecurity.");
        }

        // Responds with a random reply for the given topic and triggers a follow-up prompt
        private void RespondToTopic(string topic)
        {
            if (!responses.ContainsKey(topic)) return;
            currentTopic = topic;
            string reply = responses[topic][rnd.Next(responses[topic].Length)];
            TypeResponse(reply);
            HandleFollowUp(topic);
        }
        // Sends a random follow-up question or asks if user wants another topic
        private void HandleFollowUp(string topic)
        {
            if (followUps.ContainsKey(topic))
            {
                string followUp = followUps[topic][rnd.Next(followUps[topic].Length)];
                TypeResponse(followUp);
                awaitingFollowUpResponse = true;
                lastFollowUpTopic = topic;
            }
            else
            {
                TypeResponse("Would you like to explore another topic?");
                awaitingFollowUpResponse = false;
                lastFollowUpTopic = null;
            }
        }
        // Detects if any known topic keyword is contained in input
        private string DetectTopic(string input)
        {
            foreach (var topic in responses.Keys)
            {
                if (input.Contains(topic)) return topic;
            }
            return null;
        }
        // Detects if any sentiment keyword is contained in input
        private string DetectSentiment(string input)
        {
            foreach (var sentiment in sentiments.Keys)
            {
                if (input.Contains(sentiment)) return sentiment;
            }
            return null;
        }
        // Queue and flag for managing asynchronous typing effect messages
        private readonly Queue<string> messageQueue = new Queue<string>();
        private bool isTyping = false;
        // Initializes a timer that checks for due reminders every 5 seconds
        private void InitializeReminderTimer()
        {
            reminderTimer = new System.Windows.Threading.DispatcherTimer();
            reminderTimer.Interval = TimeSpan.FromSeconds(5);
            reminderTimer.Tick += (s, e) => CheckReminders();
            reminderTimer.Start();
        }
        // Sends a prompt asking the user if they want more help
        private void AskFollowUp()
        {
            TypeResponse($"🤖 Is there anything else I can do for you, {username}?");
        }
        // Checks all tasks for due reminders and notifies user
        private void CheckReminders()
        {
            DateTime now = DateTime.Now;
            foreach (var task in userTasks)
            {
                if (!task.IsComplete && task.ReminderTime.HasValue && task.ReminderTime.Value <= now)
                {
                    TypeResponse($"⏰ Reminder: Task \"{task.Title}\" is due now!");
                    task.ReminderTime = null;
                    LogAction($"Reminder triggered for task '{task.Title}'.");
                }
            }
        }

        // method to display writing
        private async void TypeResponse(string message)
        {
            messageQueue.Enqueue(message);

            if (isTyping) return;
            isTyping = true;

            while (messageQueue.Count > 0)
            {
                string nextMessage = messageQueue.Dequeue();
                string display = "";

                foreach (char c in nextMessage)
                {
                    display += c;
                    await Task.Delay(10); 
                }

                output("\n" + display + "\n");
            }

            isTyping = false;
        }
        // Plays a greeting audio file, with error handling
        private void PlayGreetingAudio(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    using (SoundPlayer player = new SoundPlayer(filePath))
                    {
                        player.PlaySync();  
                    }
                }
                else
                {
                    output($"Error: The file '{filePath}' was not found.");
                }
            }
            catch (Exception ex)
            {
                output($"Error playing audio: {ex.Message}");
            }
        }
        // Starts the welcome message sequence with tips and greetings, then asks for user name
        public async void RunWelcomeSequence()
        {
            TypeResponse("Welcome to Maven Cybersecurity ChatBot!");
            TypeResponse(GetRandomTip());
            TypeResponse(GetRandomGreeting());
           

          
            while (isTyping || messageQueue.Count > 0)
            {
                await Task.Delay(50); 
            }

          
            PlayGreetingAudio("MavenAudio.wav");

            
            output("\nWhat is your name? : \n");
        }
        // Logs user input to a chat history file, with periodic confirmation
        private void LogUserInput(string input)
        {
            File.AppendAllText(chatHistoryPath, $"User: {input}\n");
            if (inputCounter % 3 == 0)
                TypeResponse("Chat history saved to ChatHistory.txt");
        }
        // Detects if user expresses interest in cybersecurity topics and remembers them
        private void CheckForKeywords(string input)
        {
            string[] interestKeywords = { "interested", "curious", "keen", "fascinated", "interesting", "favourite", "fascinating", "like" };
            string[] cybersecurityTopics = { "cybersecurity", "malware", "viruses", "virus", "spam", "scams", "phishing", "ransomware", "vpn", "firewall", "firewalls", "social engineering", "encryption", "defender", "wifi", "updates" };

            foreach (string interest in interestKeywords)
            {
                if (input.Contains(interest))
                {
                    foreach (string topic in cybersecurityTopics)
                    {
                        if (input.Contains(topic))
                        {
                            if (!rememberedTopics.Contains(topic)) rememberedTopics.Add(topic);
                            userInterestTopic = topic;
                            userExpressedInterest = true;
                            userPromptCounter = 0;
                            TypeResponse($"Maven: That's great you're interested in {topic}, I'll remember that {username}!");
                            return;
                        }
                    }
                }
            }
        }
        // Parses natural language time expressions into TimeSpan
        private bool TryParseTime(string input, out TimeSpan timeSpan)
        {
            timeSpan = TimeSpan.Zero;
            input = input.Trim().ToLower();

            try

            {
                // Parse seconds, minutes, hours, days, and specific keywords like tomorrow, tonight, next week, this weekend
                // Returns true if parsed successfully, false otherwise
                if (input.Contains("second"))
                {
                    int secs = int.Parse(new string(input.Where(char.IsDigit).ToArray()));
                    timeSpan = TimeSpan.FromSeconds(secs);
                    return true;
                }
                else if (input.Contains("minute"))
                {
                    int minutes = int.Parse(new string(input.Where(char.IsDigit).ToArray()));
                    timeSpan = TimeSpan.FromMinutes(minutes);
                    return true;
                }
                else if (input.Contains("hour"))
                {
                    int hours = int.Parse(new string(input.Where(char.IsDigit).ToArray()));
                    timeSpan = TimeSpan.FromHours(hours);
                    return true;
                }
                else if (input.Contains("day"))
                {
                    int days = int.Parse(new string(input.Where(char.IsDigit).ToArray()));
                    timeSpan = TimeSpan.FromDays(days);
                    return true;
                }
                else if (input == "tomorrow")
                {
                    timeSpan = DateTime.Today.AddDays(1) - DateTime.Now;
                    return true;
                }
                else if (input == "tonight")
                {
                    var tonight = DateTime.Today.AddHours(21);
                    if (tonight <= DateTime.Now)
                        tonight = tonight.AddDays(1); 
                    timeSpan = tonight - DateTime.Now;
                    return true;
                }
                else if (input == "next week")
                {
                    timeSpan = TimeSpan.FromDays(7);
                    return true;
                }
                else if (input == "this weekend")
                {
                    int daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)DateTime.Now.DayOfWeek + 7) % 7;
                    var weekendTime = DateTime.Today.AddDays(daysUntilSaturday).AddHours(10); // 10 AM Saturday
                    timeSpan = weekendTime - DateTime.Now;
                    return true;
                }
            }
            catch { }

            return false;
        }

        // Returns a random cybersecurity tip from predefined list
        private string GetRandomTip()
        {
            string[] tips = {
                "Tip: Use a password manager to create and store strong, unique passwords.",
                "Tip: Enable two-factor authentication (2FA) wherever possible.",
                "Tip: Keep your software and operating system up to date.",
                "Tip: Never click on suspicious links in emails or messages.",
                "Tip: Back up important data regularly to an external drive or cloud service."
            };
            return tips[rnd.Next(tips.Length)];
        }
        // Returns a random greeting phrase for welcoming the user
        private string GetRandomGreeting()
        {
            string[] greetings = new[] {
                "Hello there! I'm Maven, your cybersecurity companion.",
                "Hey! Maven here, secure and ready to assist!",
                "Hi! I’m Maven. Let’s explore some digital safety tips.",
                "Greetings, friend! I am ready to assist you today!",
                "Yo! Maven here — your personal cyber safety sidekick."
            };
            return greetings[rnd.Next(greetings.Length)];
        }

       //classes with the dictionaries of responses, sentiments, followups and descriptions
        private Dictionary<string, string[]> responses = ResponsesDictionary.Data;
        private Dictionary<string, string[]> followUps = FollowUpsDictionary.Data;
        private Dictionary<string, string> sentiments = SentimentsDictionary.Data;
        private Dictionary<string, string> descriptions = DescriptionsDictionary.Data;

        // Represents a user task with title, description, optional reminder time, and completion status

        private class TaskItem
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime? ReminderTime { get; set; }
            public bool IsComplete { get; set; }
        }
        // Represents a quiz question with text, optional answer options, correct answer, and explanation
        private class QuizQuestion
        {
            public string QuestionText { get; set; }
            public List<string> Options { get; set; } 
            public string CorrectAnswer { get; set; }
            public string Explanation { get; set; }
        }

        // Starts the quiz by initializing counters and question list, then presents first question
        private void StartQuiz(string username)
        {
            isInQuiz = true;
            currentQuestionIndex = 0;
            correctAnswers = 0;
            quizQuestions = GenerateQuizQuestions();

            TypeResponse($"🧠 Let's start the Cybersecurity Quiz {username}! Type the letter (A/B/C/D) or 'true/false' for each question.");
            LogAction("User started the cybersecurity quiz.");

            FollowUpQuestion();
        }
        // Presents the current quiz question and answer options (if any) to the user
        private void FollowUpQuestion()
        {
            if (currentQuestionIndex >= quizQuestions.Count)
            {
                EndQuiz(username);
                return;
            }

            var q = quizQuestions[currentQuestionIndex];
            StringBuilder questionBubble = new StringBuilder();

            questionBubble.AppendLine($"\n🧠 Q{currentQuestionIndex + 1}: {q.QuestionText}");

            if (q.Options != null)
            {
                for (int i = 0; i < q.Options.Count; i++)
                {
                    char option = (char)('A' + i);
                    questionBubble.AppendLine($"{option}) {q.Options[i]}");
                }
            }

            TypeResponse(questionBubble.ToString());
        }
        // Processes the user’s answer, checks correctness, responds with feedback, and moves to next question
        private void ProcessQuizAnswer(string input)
        {
            var q = quizQuestions[currentQuestionIndex];
            string answer = q.CorrectAnswer.ToLower();

            bool isCorrect = false;

            if (q.Options != null)
            {
                
                int index = answer[0] - 'a';
                isCorrect = input == answer || (index >= 0 && index < q.Options.Count && input == q.Options[index].ToLower());
            }
            else
            {
               
                isCorrect = input == answer;
            }

            if (isCorrect)
            {
                correctAnswers++;
                TypeResponse("✅ Correct!");
            }
            else
            {
                TypeResponse($"❌ Incorrect. The correct answer is: {q.CorrectAnswer.ToUpper()}");
            }

            TypeResponse($"📝 {q.Explanation}");
            currentQuestionIndex++;
            FollowUpQuestion();
        }

        // Ends the quiz, shows score and personalized feedback based on performance
        private void EndQuiz(string username)
        {
            isInQuiz = false;
            TypeResponse($"\n🎉 Quiz complete! You scored {correctAnswers} out of {quizQuestions.Count}.");
            LogAction($"User completed quiz with score {correctAnswers}/{quizQuestions.Count}.");


            string feedback;
            if (correctAnswers >= 9)
                feedback = $"🏆 Amazing {username} ! You're a cybersecurity pro!";
            else if (correctAnswers >= 7)
                feedback = $"👍 Great job {username}! You know your stuff.";
            else if (correctAnswers >= 4)
                feedback = $"🙂 Not bad {username}, keep learning to stay safe online!";
            else
                feedback = $"📚 Don't worry {username}— keep studying and you'll get there!";

            TypeResponse(feedback);
        }
        // Generates and returns a predefined list of quiz questions with answers and explanations
        private List<QuizQuestion> GenerateQuizQuestions()
        {
            return new List<QuizQuestion>
    {
        new QuizQuestion {
            QuestionText = "What does 'phishing' mean in cybersecurity?",
            Options = new List<string> { "A cyber sport", "A hacking method", "Tricking users into giving information", "A data backup process" },
            CorrectAnswer = "C",
            Explanation = "Phishing is when attackers trick users into giving up sensitive info like passwords."
        },
        new QuizQuestion {
            QuestionText = "True or False: A strong password should include letters, numbers, and symbols.",
            Options = null,
            CorrectAnswer = "true",
            Explanation = "Strong passwords use a mix of characters to resist brute-force attacks."
        },
        new QuizQuestion {
            QuestionText = "Which of these is a common form of malware?",
            Options = new List<string> { "Firewall", "Trojan", "VPN", "Patch" },
            CorrectAnswer = "B",
            Explanation = "Trojans disguise themselves as legitimate software to infect your system."
        },
        new QuizQuestion {
            QuestionText = "True or False: Public Wi-Fi is always safe if it has a password.",
            Options = null,
            CorrectAnswer = "false",
            Explanation = "Even password-protected public Wi-Fi can be compromised by attackers on the same network."
        },
        new QuizQuestion {
            QuestionText = "Which of the following is used to encrypt communications online?",
            Options = new List<string> { "SSL/TLS", "HTML", "CSS", "HTTP" },
            CorrectAnswer = "A",
            Explanation = "SSL/TLS protocols secure data transmitted over the internet."
        },
        new QuizQuestion {
            QuestionText = "True or False: Antivirus software can detect every type of malware.",
            Options = null,
            CorrectAnswer = "false",
            Explanation = "No antivirus is perfect — it's important to also practice safe browsing habits."
        },
        new QuizQuestion {
            QuestionText = "What is the purpose of two-factor authentication (2FA)?",
            Options = new List<string> { "To create longer passwords", "To back up your data", "To secure login with an extra step", "To share credentials securely" },
            CorrectAnswer = "C",
            Explanation = "2FA adds an extra layer of protection beyond just your password."
        },
        new QuizQuestion {
            QuestionText = "Which of the following is NOT a good security habit?",
            Options = new List<string> { "Reusing passwords", "Updating software", "Using antivirus", "Enabling 2FA" },
            CorrectAnswer = "A",
            Explanation = "Reusing passwords is risky — if one is stolen, all accounts become vulnerable."
        },
        new QuizQuestion {
            QuestionText = "True or False: A VPN hides your IP address and encrypts traffic.",
            Options = null,
            CorrectAnswer = "true",
            Explanation = "VPNs protect your privacy by encrypting your internet connection and masking your IP."
        },
        new QuizQuestion {
            QuestionText = "What is ransomware?",
            Options = new List<string> { "A type of antivirus", "A tool for data backups", "Malware that locks data for ransom", "A firewall rule" },
            CorrectAnswer = "C",
            Explanation = "Ransomware locks files and demands payment to unlock them."
        }
    };
        }
        // Logs a description with a timestamp in the activity log, keeps log size limited to 100
        private void LogAction(string description)
        {
            string timestamp = DateTime.Now.ToString("g"); 
            activityLog.Add($"[{timestamp}] {description}");

          
            if (activityLog.Count > 100)
            {
                activityLog.RemoveAt(0);
            }
        }
        // Displays the last 10 logged activities to the user
        private void ShowActivityLog()
        {
            if (activityLog.Count == 0)
            {
                TypeResponse("🗒️ There's no activity to show yet.");
                return;
            }

            activityDisplayIndex = Math.Max(0, activityLog.Count - 10); 
            TypeResponse("📋 Here's what I've done for you recently:");

            for (int i = activityDisplayIndex; i < activityLog.Count; i++)
            {
                TypeResponse(activityLog[i]);
            }
        }
        // Shows older logged activities in batches of 10 when user asks for more
        private void ShowMoreActivity()
        {
            if (activityDisplayIndex <= 0)
            {
                TypeResponse("📁 No more activity to show.");
                return;
            }

            int nextBatchStart = Math.Max(0, activityDisplayIndex - 10);
            int nextBatchEnd = activityDisplayIndex;

            for (int i = nextBatchStart; i < nextBatchEnd; i++)
            {
                TypeResponse(activityLog[i]);
            }

            activityDisplayIndex = nextBatchStart;
        }
        // Converts a TimeSpan into a readable string format like "5 minutes" or "2 days"
        private string FormatTimeSpan(TimeSpan span)
        {
            if (span.TotalSeconds < 60)
                return ((int)span.TotalSeconds).ToString() + " seconds";
            if (span.TotalMinutes < 60)
                return ((int)span.TotalMinutes).ToString() + " minutes";
            if (span.TotalHours < 24)
                return ((int)span.TotalHours).ToString() + " hours";
            return ((int)span.TotalDays).ToString() + " days";
        }




    }

}
