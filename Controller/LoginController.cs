using ConsoleApp1.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace ConsoleApp1.Controller
{
    public class LoginController : WebSocketBehavior
    {
        // Dictionary to keep track of topic subscriptions with connection IDs
        private static Dictionary<string, HashSet<string>> topicSubscriptions = new Dictionary<string, HashSet<string>>();
        // Variable to store the authenticated user
        private User authenticatedUser;
        // List of predefined users for authentication
        private User[] users = new User[]
        {
            new User { Email = "admin@gmail.com", Password = "admin", UserRole = "Admin" },
            new User { Email = "user@gmail.com", Password = "user", UserRole = "User" }
        };

        // Method to authenticate users based on email and password
        public User Authenticate(string email, string password)
        {
            var user = users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase) && u.Password == password);
            return user;
        }

        // Method to handle login messages and authenticate the user
        private void HandleLogin(string loginData)
        {
            try
            {
                // Split the login data into email and password
                var credentials = loginData.Split(',');
                if (credentials.Length == 2)
                {
                    var email = credentials[0].Trim();
                    var password = credentials[1].Trim();
                    Console.WriteLine($"Received login credentials - Email: {email}, Password: {password}");
                    var user = Authenticate(email, password);
                    if (user != null)
                    {
                        authenticatedUser = user; // Store the authenticated user
                        Send($"{user.UserRole}"); // Send the user role back to the client
                    }
                    else
                    {
                        Send("Authentication failed");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid login data format");
                    Send("Authentication failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                Send("Authentication failed");
            }
        }

        
        protected override void OnMessage(MessageEventArgs e)
        {
            string[] messageParts = e.Data.Split(':');
            //  login messages
            if (messageParts.Length == 2 && messageParts[0] == "login")
            {
                HandleLogin(messageParts[1]);
            }
            //subscription to a topic
            else if (messageParts.Length == 2 && messageParts[0] == "subscribe")
            {
                SubscribeToTopic(messageParts[1]);
            }
            // unsubscription from a topic
            else if (messageParts.Length == 2 && messageParts[0] == "unsubscribe")
            {
                UnsubscribeFromTopic(messageParts[1]);
            }
        }

        // subscribe the current connection to a specific topic
        private void SubscribeToTopic(string topic)
        {
            if (!topicSubscriptions.ContainsKey(topic))
            {
                topicSubscriptions[topic] = new HashSet<string>();
            }
            topicSubscriptions[topic].Add(ID);
        }

        // unsubscribe the current connection from a specific topic
        private void UnsubscribeFromTopic(string topic)
        {
            if (topicSubscriptions.ContainsKey(topic))
            {
                topicSubscriptions[topic].Remove(ID);
            }
        }

        // unsubscribe the current connection from all topics
        private void UnsubscribeFromAllTopics()
        {
            foreach (var subscribers in topicSubscriptions.Values)
            {
                subscribers.Remove(ID);
            }
        }

        // WebSocket connection is closed
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            // Unsubscribe the connection
            UnsubscribeFromAllTopics();
        }
    }
}
