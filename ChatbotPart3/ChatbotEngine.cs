
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
        private readonly Action<string> output;
        private List<TaskItem> userTasks = new List<TaskItem>();
        private System.Windows.Threading.DispatcherTimer reminderTimer;

        private List<string> activityLog = new List<string>();
        private int activityDisplayIndex = 0;


        private readonly Random rnd = new Random();
        private bool isInQuiz = false;
        private int currentQuestionIndex = 0;
        private int correctAnswers = 0;

        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>();

        private List<string> rememberedTopics = new List<string>();
        private string userInterestTopic = "";
        private int userPromptCounter = 0;
        private bool userExpressedInterest = false;
        private int followUpIndex = 0;
        private bool inConversation = false;

        private string currentTopic = null;
        private string lastFollowUpTopic = null;
        private bool awaitingFollowUpResponse = false;
        private int inputCounter = 0;
        private string chatHistoryPath = "chathistory.txt";
        private string username = "Friend";

        public ChatbotEngine(Action<string> outputCallback)
        {
            output = outputCallback;
            InitializeReminderTimer();
        }



        public void Initialize()
        {
            RunWelcomeSequence();

        }

        

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
        private TaskItem lastAddedTask = null;
        private bool awaitingReminderConfirmation = false;
        private TaskItem awaitingReminderTimeForTask = null;


        public void ProcessInput(string input)
        {



            if (isInQuiz)
            {
                ProcessQuizAnswer(input.ToLower().Trim());
                return;
            }

            string loweredInput = input.ToLower();
            LogUserInput(loweredInput);
            CheckForKeywords(loweredInput);
            inputCounter++;
            userPromptCounter++;

            if (awaitingReminderConfirmation)
            {
                if (loweredInput.Contains("yes"))
                {
                    TypeResponse("Great! Please tell me when to remind you (e.g. 'in 3 days', 'tomorrow at 5pm').");
                    awaitingReminderConfirmation = false;
                    awaitingReminderTimeForTask = lastAddedTask;
                    return;
                }
                else
                {
                    TypeResponse("No problem! If you want to set a reminder later, just ask me.");
                    lastAddedTask = null;
                    awaitingReminderConfirmation = false;
                    return;
                }
            }

         
            if (awaitingReminderTimeForTask != null)
            {
                string timePart = loweredInput;
                if (TryParseTime(timePart, out TimeSpan offset))
                {
                    awaitingReminderTimeForTask.ReminderTime = DateTime.Now.Add(offset);
                    TypeResponse($"Got it! I'll remind you about \"{awaitingReminderTimeForTask.Title}\" in {FormatTimeSpan(offset)}.");
                    LogAction($"Reminder set for task '{awaitingReminderTimeForTask.Title}' in {FormatTimeSpan(offset)}.");
                    awaitingReminderTimeForTask = null;
                }
                else if (DateTime.TryParse(timePart, out DateTime specificDate))
                {
                    awaitingReminderTimeForTask.ReminderTime = specificDate;
                    TypeResponse($"Got it! I'll remind you about \"{awaitingReminderTimeForTask.Title}\" at {specificDate}.");
                    LogAction($"Reminder set for task '{awaitingReminderTimeForTask.Title}' at {specificDate}.");
                    awaitingReminderTimeForTask = null;
                }
                else
                {
                    TypeResponse("Sorry, I couldn't understand that time. Please try again.");
                }
                return;
            }

           
            if (loweredInput.StartsWith("add task"))
            {
                string taskDesc = input.Substring(input.ToLower().IndexOf("add task") + 8).Trim();

               
                if (taskDesc.StartsWith("-"))
                    taskDesc = taskDesc.Substring(1).Trim();

                if (string.IsNullOrWhiteSpace(taskDesc))
                {
                    TypeResponse("Please provide a task description after 'add task'.");
                    return;
                }

                var task = new TaskItem
                {
                    Title = taskDesc.Length > 30 ? taskDesc.Substring(0, 30) : taskDesc, 
                    Description = taskDesc,
                    IsComplete = false
                };

                userTasks.Add(task);
                lastAddedTask = task;
                awaitingReminderConfirmation = true;
                TypeResponse($"Task added with the description \"{task.Description}\". Would you like a reminder?");
                LogAction($"Task '{task.Title}' added.");
                return;
            }

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

            if (loweredInput.Contains("quiz") || loweredInput.Contains("play") || loweredInput.Contains("game"))
            {
                StartQuiz(username);
                return;
            }

            if (loweredInput.Contains("exit"))
            {
                TypeResponse($"Goodbye {username}! Stay safe online.");
                return;
            }

            if (userPromptCounter >= 3 && userExpressedInterest && !string.IsNullOrEmpty(userInterestTopic))
            {
                string randomTopic = rememberedTopics[rnd.Next(rememberedTopics.Count)];
                TypeResponse($"As someone who is curious about {randomTopic}, this is particularly important.");
                userPromptCounter = 0;
            }

           
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

          
            if (loweredInput.StartsWith("remind me to "))
            {
                string remainder = input.Substring(13).Trim();
                string[] parts = remainder.Split(new[] { " in ", " at ", " tomorrow", " tonight" }, StringSplitOptions.None);

                string title = parts[0].Trim();
                var task = userTasks.FirstOrDefault(t => t.Title.ToLower() == title.ToLower());

               
                if (task == null)
                {
                    task = new TaskItem { Title = title, Description = "No description provided.", IsComplete = false };
                    userTasks.Add(task);
                    LogAction($"Task '{task.Title}' added via reminder.");
                }

                DateTime reminderTime = DateTime.Now;

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
                LogAction($"Reminder set for task '{task.Title}' at {reminderTime}.");
                return;
            }

            
            if (loweredInput.Contains("task"))
            {
                if (loweredInput.StartsWith("add task"))
                {
                    string taskData = input.Substring(input.ToLower().IndexOf("add task") + 8).Trim();
                    string[] parts = taskData.Split('|');
                    if (parts.Length < 2)
                    {
                        TypeResponse("Please enter the task like this:\nadd task Title | Description | remind me in 2 minutes (optional)");
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
                    }
                    return;
                }
                else if (loweredInput.Contains("show tasks") || loweredInput.Contains("list tasks") || loweredInput.Contains("view tasks"))
                {
                    if (userTasks.Count == 0)
                    {
                        TypeResponse("You have no tasks.");
                    }
                    else
                    {
                        foreach (var task in userTasks)
                        {
                            string status = task.IsComplete ? "✅ Completed" : "❗ Incomplete";
                            string reminder = task.ReminderTime.HasValue ? $"(Reminder: {task.ReminderTime})" : "";
                            TypeResponse($"• {task.Title} - {task.Description} {reminder} - {status}");
                        }
                        LogAction("Viewed tasks");
                    }
                    return;
                }
                else if (loweredInput.StartsWith("complete task ") ||
         loweredInput.StartsWith("mark task ") ||
         loweredInput.StartsWith("task is "))
                {
                    string title = null;

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
                        var task = userTasks.FirstOrDefault(t =>
                            t.Title != null &&
                            t.Title.ToLower().Contains(title.ToLower()));

                        if (task != null)
                        {
                            task.IsComplete = true;
                            TypeResponse($"✅ Task \"{task.Title}\" marked as complete.");
                            LogAction($"Task '{task.Title}' marked as complete.");
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

                else if (loweredInput.StartsWith("delete task ")|| loweredInput.Contains("delete task "))
                {
                    string title = loweredInput.Replace("delete task ", "").Trim();
                    var task = userTasks.Find(t => t.Title.ToLower() == title.ToLower());
                    if (task != null)
                    {
                        userTasks.Remove(task);
                        TypeResponse($"Task \"{task.Title}\" deleted.");
                        LogAction($"Task '{task.Title}' deleted.");
                    }
                    else
                    {
                        TypeResponse("Task not found.");
                    }
                    return;
                }
                else if (loweredInput.StartsWith("remind") || loweredInput.StartsWith("remind me") || loweredInput.StartsWith("set reminder"))
                {
                    string remainder = input.ToLower();

                    if (remainder.StartsWith("set reminder"))
                        remainder = remainder.Substring("set reminder".Length).Trim();
                    else if (remainder.StartsWith("remind me"))
                        remainder = remainder.Substring("remind me".Length).Trim();
                    else if (remainder.StartsWith("remind"))
                        remainder = remainder.Substring("remind".Length).Trim();

                    int taskIndex = remainder.IndexOf("task");
                    if (taskIndex == -1)
                    {
                        TypeResponse("Please specify the task name like: 'set reminder for task TaskName in 10 minutes'");
                        return;
                    }

                    string afterTask = remainder.Substring(taskIndex + 4).Trim();
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

                    var task = userTasks.Find(t => t.Title.ToLower() == title.ToLower());
                    if (task == null)
                    {
                        TypeResponse($"I couldn't find a task named \"{title}\".");
                        return;
                    }

                    if (timePart.Contains(":") || timePart.Contains("/"))
                    {
                        if (DateTime.TryParse(timePart, out DateTime specificDateTime))
                        {
                            task.ReminderTime = specificDateTime;
                            TypeResponse($"⏰ Reminder set for task \"{task.Title}\" at {specificDateTime}.");
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
                        LogAction($"Reminder set for task '{task.Title}' in {humanReadableTime}.");
                    }
                    else
                    {
                        TypeResponse("Sorry, I couldn't understand the time duration. Try '10 minutes', '2 hours', or '1 day'.");
                    }

                    return;
                }
            }

           
            if (loweredInput.StartsWith("what is ") || loweredInput.Contains("definition"))
            {
                string possibleTopic = loweredInput.Replace("what is ", "").Trim();
                if (descriptions.ContainsKey(possibleTopic))
                {
                    TypeResponse(descriptions[possibleTopic]);
                    HandleFollowUp(possibleTopic);
                    return;
                }
                else
                {
                    TypeResponse("Hmm, I don't understand. Please check your spelling or ask about another cybersecurity topic!");
                    return;
                }
            }

            string detectedSentiment = DetectSentiment(loweredInput);
            string detectedTopic = DetectTopic(loweredInput);
            if (detectedSentiment != null && detectedTopic != null)
            {
                TypeResponse(sentiments[detectedSentiment]);

               
                lastFollowUpTopic = detectedTopic;
                awaitingFollowUpResponse = true;
                inConversation = true;
                followUpIndex = 0;

                TypeResponse($"Would you like to learn more about {detectedTopic}?");
                return;
            }

            if (awaitingFollowUpResponse)
            {
                if (loweredInput.Contains("yes") || loweredInput.Contains("confused") || loweredInput.Contains("explain"))
                {
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

            if (currentTopic != null && (loweredInput.Contains("explain") || loweredInput.Contains("more")))
            {
                RespondToTopic(currentTopic);
                return;
            }

            if (detectedTopic != null)
            {
                RespondToTopic(detectedTopic);
                return;
            }

            TypeResponse("I'm sorry, I don't understand that. Try asking about phishing, malware, or cybersecurity.");
        }


        private void RespondToTopic(string topic)
        {
            if (!responses.ContainsKey(topic)) return;
            currentTopic = topic;
            string reply = responses[topic][rnd.Next(responses[topic].Length)];
            TypeResponse(reply);
            HandleFollowUp(topic);
        }

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

        private string DetectTopic(string input)
        {
            foreach (var topic in responses.Keys)
            {
                if (input.Contains(topic)) return topic;
            }
            return null;
        }

        private string DetectSentiment(string input)
        {
            foreach (var sentiment in sentiments.Keys)
            {
                if (input.Contains(sentiment)) return sentiment;
            }
            return null;
        }

        private readonly Queue<string> messageQueue = new Queue<string>();
        private bool isTyping = false;

        private void InitializeReminderTimer()
        {
            reminderTimer = new System.Windows.Threading.DispatcherTimer();
            reminderTimer.Interval = TimeSpan.FromSeconds(5);
            reminderTimer.Tick += (s, e) => CheckReminders();
            reminderTimer.Start();
        }

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

        private void LogUserInput(string input)
        {
            File.AppendAllText(chatHistoryPath, $"User: {input}\n");
            if (inputCounter % 3 == 0)
                TypeResponse("Chat history saved to ChatHistory.txt");
        }

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

        private bool TryParseTime(string input, out TimeSpan timeSpan)
        {
            timeSpan = TimeSpan.Zero;
            input = input.Trim().ToLower();

            try
            {
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

       
        private Dictionary<string, string[]> responses = ResponsesDictionary.Data;
        private Dictionary<string, string[]> followUps = FollowUpsDictionary.Data;
        private Dictionary<string, string> sentiments = SentimentsDictionary.Data;
        private Dictionary<string, string> descriptions = DescriptionsDictionary.Data;

        private class TaskItem
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime? ReminderTime { get; set; }
            public bool IsComplete { get; set; }
        }

        private class QuizQuestion
        {
            public string QuestionText { get; set; }
            public List<string> Options { get; set; } 
            public string CorrectAnswer { get; set; }
            public string Explanation { get; set; }
        }

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

        private void LogAction(string description)
        {
            string timestamp = DateTime.Now.ToString("g"); 
            activityLog.Add($"[{timestamp}] {description}");

          
            if (activityLog.Count > 100)
            {
                activityLog.RemoveAt(0);
            }
        }

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
