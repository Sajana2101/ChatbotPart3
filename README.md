## Maven Cybersecurity Chatbot (WPF)

Cybersecurity Chatbot **Maven**, is a WPF-based cybersecurity chatbot application built in C# and XAML. This interactive assistant helps users learn about cybersecurity through asking questions, manage personal tasks with reminders, and test their knowledge through quizzes.
---

##  Features

###  Cybersecurity Education
- Explains common cybersecurity topics (e.g., phishing, malware, VPNs).
- Offers friendly follow-up prompts to guide learning.
- Includes sentiment detection and tailored educational suggestions.

### Task Assistant
- Natural language task creation (e.g., `add task Change my password`).
- Optional reminder support (`remind me in 10 minutes`, `tonight`, etc.).
- Task listing, completion marking, and deletion supported.
- Reminder system triggers alerts for due tasks.

### Cybersecurity Quiz
- 10-question quiz (multiple choice & true/false).
- Score tracking with feedback based on performance.
- Educational explanations after each question.

### Activity Log
- Records recent chatbot actions (e.g., task creation, quiz completion).
- View last 10 activities or show more on request.

### Chat Interface
- Clean WPF UI with chat bubbles for user/bot.
- Optional audio greeting and customizable chatbot responses.
- Typing-style response simulation (can be toggled for performance).

### NPL
- Natural conversation flow
- Able to detect key words and act accordingly

---

## Technologies Used

- **.NET 6 / .NET Core WPF**
- **C# (async/await, LINQ, OOP)**
- **XAML** for UI layout and design
- **System.Media** for audio playback
- **DispatcherTimer** for real-time reminder checking

---

## Getting Started

### Prerequisites

- Windows OS
- Visual Studio 2022 or later
- .NET Desktop Runtime 6 or later

### Setup

1. Clone the repository or download the source files.
2. Open the solution in Visual Studio.
3. Build the project to restore dependencies.
4. Ensure the following folders exist:
   - `Images/` → Contains chatbot logo and background image.
   - `MavenAudio.wav` → Greeting audio file (place in root or specify path).
5. Press `F5` to run the chatbot!

---

## Project Structure

ChatbotPart3
MainWindow.xaml # UI layout
MainWindow.xaml.cs # UI logic and event handling
ChatbotEngine.cs # Chatbot logic and feature handling
Dictionaries/ # Topic responses, follow-ups, and tips
Images/ # Background and bot image
MavenAudio.wav # Optional welcome audio
README.md

---

## ✨ Credits

This project was designed and implemented by **Sajana Motheram** as part of a learning and development journey into C# WPF applications, artificial intelligence interfaces, and cybersecurity awareness tools for my programming POE.

---

