/*****************************************************************//**
 * \file    MessageDispatcher.cs
 * \brief   Class for handling the transmission of system-level messages to their subscribers
 *
 * \author Lucas Jodon 
 * \date   8/14/2020
***********************************************************************/
using Android.App;
using System.Collections.Generic;
using System.Linq;

namespace SoftWing.System
{
    public class MessageDispatcher
    {
        /*------------------ Private member variables ------------------*/
        private static Dictionary<MessageType, List<MessageSubscriber>> system_message_subs = new Dictionary<MessageType, List<MessageSubscriber>>();
        public static Activity calling_activity = null;
        private static MessageDispatcher Instance = null;

        /*------------------ Public member functions ------------------*/
        public MessageDispatcher(Activity _calling_activity)
        {
            if (Instance != null)
            {
                return;
            }
            Instance = this;
            calling_activity = _calling_activity;
        }

        /**
         * Subscribe sub to system-level messages of type t.
         * 
         * \param t System-level message type to subscribe to
         * \param sub Subscriber to receive messages of type t
         */
        public void Subscribe(MessageType t, MessageSubscriber sub)
        {
            // Make sure all dispatcher functions are synced with the main thread
            calling_activity.RunOnUiThread(() =>
            {
                List<MessageSubscriber> sub_list;
                if (!system_message_subs.TryGetValue(t, out sub_list))
                {
                    // Create a new subscriber list
                    sub_list = new List<MessageSubscriber> { sub };
                    system_message_subs.Add(t, sub_list);
                }
                else
                {
                    // Append to existing subscriber list if it isn't already there
                    if (sub_list.Find(x => x == sub) != null)
                    {
                        return;
                    }
                    sub_list.Add(sub);
                    system_message_subs[t] = sub_list;
                }
            });
        }

        /**
         * Unsubscribe sub to system-level messages of type t.
         * 
         * \param t System-level message type to unsubscribe from
         * \param sub Subscriber to no longer receive messages of type t
         */
        public void Unsubscribe(MessageType t, MessageSubscriber sub)
        {
            // Make sure all dispatcher functions are synced with the main thread
            calling_activity.RunOnUiThread(() =>
            {
                List<MessageSubscriber> sub_list;
                if (system_message_subs.TryGetValue(t, out sub_list))
                {
                    // Remove from existing subscriber list
                    sub_list.Remove(sub);
                }
            });
        }

        /**
         * Add message to the dispatch queue.
         * 
         * \param message Message to add to the queue.
         */
        public void Post(SystemMessage message)
        {
            // Make sure all dispatcher functions are synced with the main thread
            calling_activity.RunOnUiThread(() =>
            {
                List<MessageSubscriber> sub_list;
                if (!system_message_subs.TryGetValue(message.getMessageType(), out sub_list))
                {
                    // No subscribers to message, nothing to do here.
                    return;
                }

                foreach (var sub in sub_list)
                {
                    sub.Accept(message);
                }
            });
        }
    }
}
