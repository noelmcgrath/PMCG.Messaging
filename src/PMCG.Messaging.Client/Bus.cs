﻿using Common.Logging;
using PMCG.Messaging.Client.BusState;
using PMCG.Messaging.Client.Configuration;
using System;
using System.Collections.Concurrent;
using System.IO;


namespace PMCG.Messaging.Client
{
	public class Bus : IBusContext, IBusController, IBus
	{
		private readonly ILog c_logger;


		public State State { get; set; }


		public Bus(
			BusConfiguration configuration)
		{
			this.c_logger = LogManager.GetCurrentClassLogger();
			this.c_logger.Info("ctor Starting");

			Check.RequireArgumentNotNull("configuration", configuration);
			Check.RequireArgument("configuration.DisconnectedMessagesStoragePath", configuration.DisconnectedMessagesStoragePath,
				Directory.Exists(configuration.DisconnectedMessagesStoragePath));

			var _connectionManager = ServiceLocator.GetConnectionManager(configuration);

			this.State = new Initialised(
				configuration,
				_connectionManager,
				new BlockingCollection<QueuedMessage>(),
				this);

			this.c_logger.Info("ctor Completed");
		}


		public void Connect()
		{
			this.c_logger.Info("Connect Starting");
			this.State.Connect();
			this.c_logger.Info("Connect Completed");
		}


		public void Close()
		{
			this.c_logger.Info("Close Starting");
			this.State.Close();
			this.c_logger.Info("Close Completed");
		}


		public void Publish<TMessage>(
			TMessage message)
			where TMessage : Message
		{
			this.c_logger.Info("Publish Starting");
			Check.RequireArgumentNotNull("message", message);

			this.State.Publish(message);

			this.c_logger.Info("Publish Completed");
		}
	}
}