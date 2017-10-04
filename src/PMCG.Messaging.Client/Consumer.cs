﻿using log4net;
using PMCG.Messaging.Client.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client
{
	public class Consumer
	{
		private readonly ILog c_logger;
		private readonly BusConfiguration c_configuration;
		private readonly IModel c_channel;
		private readonly CancellationToken c_cancellationToken;
		private readonly ConsumerMessageProcessor c_messageProcessor;


		private bool c_hasBeenStarted;
		private bool c_isCompleted;
		private QueueingBasicConsumer c_consumer;


		public bool IsCompleted { get { return this.c_isCompleted; } }


		public Consumer(
			IConnection connection,
			BusConfiguration configuration,
			CancellationToken cancellationToken)
		{
			this.c_logger = LogManager.GetLogger(this.GetType());
			this.c_logger.Info("ctor Starting");

			this.c_configuration = configuration;
			this.c_cancellationToken = cancellationToken;

			this.c_logger.Info("ctor About to create channel");
			this.c_channel = connection.CreateModel();
			this.c_channel.ModelShutdown += this.OnChannelShutdown;
			this.c_channel.BasicQos(0, this.c_configuration.ConsumerMessagePrefetchCount, false);

			this.c_logger.Info("ctor About to create consumer message processor");
			this.c_messageProcessor = new ConsumerMessageProcessor(this.c_configuration);

			this.c_logger.Info("ctor Completed");
		}


		public Task Start()
		{
			this.c_logger.Info("Start Starting");
			Check.Ensure(!this.c_hasBeenStarted, "Consumer has already been started, can only do so once");
			Check.Ensure(!this.c_cancellationToken.IsCancellationRequested, "Cancellation token is already canceled");

			var _result = new Task(
				() =>
					{
						try
						{
							this.c_hasBeenStarted = true;
							this.EnsureTransientQueuesExist();
							this.CreateAndConfigureConsumer();
							this.RunConsumeLoop();
						}
						catch (Exception exception)
						{
							this.c_logger.ErrorFormat("Start Exception : {0}", exception.InstrumentationString());
							throw;
						}
						finally
						{
							if (this.c_channel.IsOpen)
							{
								// Cater for race condition, when stopping - Is open but when we get to this line it is closed
								try { this.c_channel.Close(); } catch { }
							}

							this.c_isCompleted = true;
							this.c_logger.Info("Start Consumer task completed");
						}
					},
				this.c_cancellationToken,
				TaskCreationOptions.LongRunning);
			_result.Start();

			this.c_logger.Info("Start Completed consuming");
			return _result;
		}


		private void OnChannelShutdown(
			IModel channel,
			ShutdownEventArgs reason)
		{
			this.c_logger.InfoFormat("OnChannelShuutdown Code = {0} and text = {1}", reason.ReplyCode, reason.ReplyText);
		}


		private void EnsureTransientQueuesExist()
		{
			foreach (var _configuration in this.c_configuration.MessageConsumers.GetTransientQueueConfigurations())
			{
				this.c_logger.InfoFormat("EnsureTransientQueuesExist Verifying for transient queue {0}", _configuration.QueueName);
				this.c_channel.QueueDeclare(_configuration.QueueName, false, false, true, null);
				this.c_channel.QueueBind(_configuration.QueueName, _configuration.ExchangeName, string.Empty);
			}
		}


		private void CreateAndConfigureConsumer()
		{
			this.c_consumer = new QueueingBasicConsumer(this.c_channel);
			foreach (var _queueName in this.c_configuration.MessageConsumers.GetDistinctQueueNames())
			{
				this.c_logger.InfoFormat("CreateAndConfigureConsumer Consume for queue {0}", _queueName);
				var _consumerTag = this.c_channel.BasicConsume(_queueName, false, this.c_consumer);
				this.c_logger.InfoFormat("CreateAndConfigureConsumer Consume for queue {0}, consumer tag is {1}", _queueName, _consumerTag);
			}
		}


		private void RunConsumeLoop()
		{
			this.c_logger.Info("RunConsumeLoop Starting");
			var _timeoutInMilliseconds = (int)this.c_configuration.ConsumerDequeueTimeout.TotalMilliseconds;
			while (!this.c_cancellationToken.IsCancellationRequested)
			{
				try
				{
					BasicDeliverEventArgs _dequeueResult = null;
					if (this.c_consumer.Queue.Dequeue(_timeoutInMilliseconds, out _dequeueResult))
					{
						this.c_messageProcessor.Process(this.c_channel, _dequeueResult);
					}
				}
				catch (EndOfStreamException)
				{
					this.c_logger.Info("RunConsumeLoop End of stream");
					break;
				}
			}
		}
	}
}