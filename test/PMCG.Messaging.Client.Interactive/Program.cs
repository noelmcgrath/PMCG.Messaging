﻿using System;


namespace PMCG.Messaging.Client.Interactive
{
	class Program
	{
		static void Main()
		{
			//new Publisher().Run_Where_We_Instruct_To_Stop_The_Broker();
			//new Publisher().Run_Where_We_Publish_Messages();
			//new Publisher().Run_Where_We_Publish_Async_Messages();

			//new ConnectionManager().Run_Open_Where_Server_Is_Already_Stopped_And_Instruct_To_Start_Server();

			//new Consumer().Run_Where_We_Create_A_Transient_Queue_And_Then_Close_Connection();

			//new Consumers().Run_Where_We_Instruct_To_Stop_The_Broker();

			//new Bus().Run_Where_We_Instantiate_And_Try_To_Connect_To_Non_Existent_Broker();
			//new Bus().Run_Where_We_Publish_A_Message_And_Consume_For_The_Same_Messsage();
			//new Bus().Run_Where_We_Publish_Multiple_Messages_And_Consume_For_The_Same_Messsages();
			//new Bus().Run_Where_We_Publish_A_Message_And_Consume_For_The_Same_Messsage_On_A_Transient_Queue();
			new Bus().Run_Where_We_Publish_A_Message_And_Consume_For_The_Same_Messsage_On_A_Transient_Queue();
		}
	}
}
