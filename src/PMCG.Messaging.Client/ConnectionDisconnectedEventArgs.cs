﻿using System;


namespace PMCG.Messaging.Client
{
	public class ConnectionDisconnectedEventArgs
	{
		public readonly ushort Code;
		public readonly string Reason;


		public ConnectionDisconnectedEventArgs(
			ushort code,
			string reason)
		{
			this.Code = code;
			this.Reason = reason;
		}
	}
}