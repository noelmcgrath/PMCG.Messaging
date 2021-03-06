﻿using System;
using System.Diagnostics;


namespace PMCG.Messaging.Client.Configuration
{
	public class MessageConsumer
	{
		public readonly Type Type;
		public readonly string QueueName;
		public readonly string TypeHeader;
		public readonly Func<Message, ConsumerHandlerResult> Action;
		public readonly string ExchangeName;


		public bool UseTransientQueue { get { return this.ExchangeName != null; } }


		public MessageConsumer(
			Type type,
			string queueName,
			string typeHeader,
			Func<Message, ConsumerHandlerResult> action)
		{
			Check.RequireArgumentNotNull("type", type);
			Check.RequireArgument("type", type, type.IsSubclassOf(typeof(Message)));
			Check.RequireArgumentNotEmpty("queueName", queueName);
			Check.RequireArgumentNotEmpty("typeHeader", typeHeader);
			Check.RequireArgumentNotNull("action", action);

			this.Type = type;
			this.QueueName = queueName;
			this.TypeHeader = typeHeader;
			this.Action = action;
			this.ExchangeName = null;
		}


		public MessageConsumer(
			Type type,
			string typeHeader,
			Func<Message, ConsumerHandlerResult> action,
			string exchangeName)
		{
			Check.RequireArgumentNotNull("type", type);
			Check.RequireArgument("type", type, type.IsSubclassOf(typeof(Message)));
			Check.RequireArgumentNotEmpty("typeHeader", typeHeader);
			Check.RequireArgumentNotNull("action", action);
			Check.RequireArgumentNotEmpty("exchangeName", exchangeName);

			this.Type = type;
			this.TypeHeader = typeHeader;
			this.Action = action;
			this.ExchangeName = exchangeName;

			// In case of a transient queue, we want a deterministic queue name (Exclusive and competing consumers if two bus's in same app domain)
			this.QueueName = string.Format("{0}_{1}_{2}_{3}",
				this.ExchangeName,
				Environment.MachineName,
				Process.GetCurrentProcess().Id,
				AppDomain.CurrentDomain.Id);
		}
	}
}