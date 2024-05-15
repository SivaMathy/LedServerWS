using ConsoleApp1.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace ConsoleApp1.Controller
{
    public class LedControl : WebSocketBehavior
    {
        // List to maintain the state of 5 LEDs
        private static List<Led> ledStates = Enumerable.Range(0, 5).Select(id => new Led { Id = id }).ToList();
        // Dictionary to track topic subscriptions with connection IDs
        private static Dictionary<string, HashSet<string>> topicSubscriptions = new Dictionary<string, HashSet<string>>();

        // This method is called when a new WebSocket connection is opened
        protected override void OnOpen()
        {
            base.OnOpen();
            // Automatically subscribe new connections to LED updates topic
            SubscribeToTopic("ledUpdates");
        }

        // This method handles incoming messages from the WebSocket client
        protected override void OnMessage(MessageEventArgs e)
        {
            try
            {
                // Split the message data into parts
                string[] messageParts = e.Data.Split(':');
                // Handle request for initial LED states
                if (e.Data == "getInitialStates")
                {
                    SendInitialLedStates();
                }
                // Handle status change request for LEDs
                else if (messageParts.Length == 3 && messageParts[0] == "status")
                {
                    int ledId;
                    string userRole = messageParts[2]; // Extract user role from message
                    // Check if user is admin before toggling LED state
                    if (userRole == "Admin")
                    {
                        // Validate the LED ID and toggle the state if valid
                        if (int.TryParse(messageParts[1], out ledId) && ledId >= 0 && ledId < ledStates.Count)
                        {
                            ToggleLedState(ledId);
                            BroadcastLedStates();
                        }
                        else
                        {
                            Send("Invalid LED ID.");
                        }
                    }
                    else
                    {
                        Send("Only admins can toggle LED state.");
                    }
                }
                // Handle subscription to a topic
                else if (messageParts.Length == 2 && messageParts[0] == "subscribe")
                {
                    SubscribeToTopic(messageParts[1]);
                }
                // Handle unsubscription from a topic
                else if (messageParts.Length == 2 && messageParts[0] == "unsubscribe")
                {
                    UnsubscribeFromTopic(messageParts[1]);
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions for debugging purposes
                Console.WriteLine(ex.Message);
            }
        }

        //close webSocket connection 
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            // Unsubscribe the connection from all topics upon closing
            UnsubscribeFromAllTopics();
        }

        // toggle the state of an LED by ID
        private void ToggleLedState(int id)
        {
            Led led = ledStates.FirstOrDefault(l => l.Id == id);
            if (led != null)
            {
                led.Status = !led.Status;
            }
        }

        // broadcast the current states of all LEDs to subscribers
        private void BroadcastLedStates()
        {
            string stateMessage = JsonConvert.SerializeObject(ledStates);
            BroadcastToTopic("ledUpdates", stateMessage);
        }

        // send the initial states of all LEDs to the requesting client
        private void SendInitialLedStates()
        {
            string stateMessage = JsonConvert.SerializeObject(ledStates);
            Send(stateMessage);
        }

        //subscribe the current connection to a specific topic
        private void SubscribeToTopic(string topic)
        {
            if (!topicSubscriptions.ContainsKey(topic))
            {
                topicSubscriptions[topic] = new HashSet<string>();
            }
            topicSubscriptions[topic].Add(ID);
        }

        //unsubscribe the current connection from a specific topic
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

        //broadcast a message to all subscribers of a specific topic
        private void BroadcastToTopic(string topic, string message)
        {
            if (topicSubscriptions.ContainsKey(topic))
            {
                foreach (var subscriberId in topicSubscriptions[topic])
                {
                    Sessions.SendTo(message, subscriberId);
                }
            }
        }
    }
}
