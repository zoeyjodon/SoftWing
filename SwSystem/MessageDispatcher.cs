/*****************************************************************//**
 * \file    MessageDispatcher.cs
 * \brief   Class for handling the transmission of system-level messages to their subscribers
 *
 * \author Lucas Jodon 
 * \date   8/14/2020
***********************************************************************/
using Android.App;
using Android.OS;
using System.Collections.Generic;

namespace SoftWing.SwSystem
{
    public class MessageDispatcher
    {
        /*------------------ Private member variables ------------------*/
        private Dictionary<MessageType, List<MessageSubscriber>> system_message_subs = new Dictionary<MessageType, List<MessageSubscriber>>();
        private static MessageDispatcher instance = null;

        /*------------------ Public member functions ------------------*/
        public static MessageDispatcher GetInstance()
        {
            if (instance == null)
            {
                instance = new MessageDispatcher();
            }
            return instance;
        }

        /**
         * Subscribe sub to system-level messages of type t.
         * 
         * \param t System-level message type to subscribe to
         * \param sub Subscriber to receive messages of type t
         */
        public void Subscribe(MessageType t, MessageSubscriber sub)
        {
            new Handler(Looper.MainLooper).Post(() =>
            {
                AddSubscription(t, sub);
            });
        }

        private void AddSubscription(MessageType t, MessageSubscriber sub)
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
        }

        /**
         * Unsubscribe sub to system-level messages of type t.
         * 
         * \param t System-level message type to unsubscribe from
         * \param sub Subscriber to no longer receive messages of type t
         */
        public void Unsubscribe(MessageType t, MessageSubscriber sub)
        {
            new Handler(Looper.MainLooper).Post(() =>
            {
                RemoveSubscription(t, sub);
            });
        }

        private void RemoveSubscription(MessageType t, MessageSubscriber sub)
        {
            List<MessageSubscriber> sub_list;
            if (system_message_subs.TryGetValue(t, out sub_list))
            {
                // Remove from existing subscriber list
                sub_list.Remove(sub);
            }
        }

        /**
         * Add message to the dispatch queue.
         * 
         * \param message Message to add to the queue.
         */
        public void Post(SystemMessage message)
        {
            new Handler(Looper.MainLooper).Post(() =>
            {
                Dispatch(message);
            });
        }

        private void Dispatch(SystemMessage message)
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
        }

        /*------------------ Private member functions ------------------*/
        private MessageDispatcher()
        {
            instance = this;
        }
    }
}
