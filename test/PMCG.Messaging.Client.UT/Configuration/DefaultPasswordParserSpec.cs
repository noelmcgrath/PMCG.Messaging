﻿using NUnit.Framework;
using PMCG.Messaging.Client.Configuration;
using System;


namespace PMCG.Messaging.Client.UT.Configuration
{
	[TestFixture]
	public class DefaultPasswordParserSpec
	{
		private DefaultPasswordParser c_SUT = new DefaultPasswordParser();


		[Test]
		public void Parse_Where_Entry_For_This_Machine_Exists_Results_In_Parse_Result()
		{
			var _password = "ThePassword";
			var _passwordCipher = new DefaultPasswordParser().Encrypt(_password);
			var _passwordSetting = string.Format("{0}:{1}", Environment.MachineName, _passwordCipher);

			var _result = this.c_SUT.Parse(_passwordSetting);

			Assert.IsNotNull(_password, _result);
		}


		[Test]
		public void Parse_Where_Multiple_Entries_Including_One_For_This_Machine_Exists_Results_In_Parse_Result()
		{
			var _thisMachinesPassword = "kjfhf%£$£$%%%!!!&iii9977";
			var _otherPassword = "ThePassword1";
			var _thisMachinesPasswordCipher = new DefaultPasswordParser().Encrypt(_thisMachinesPassword);
			var _otherPasswordCipher = new DefaultPasswordParser().Encrypt(_otherPassword);

			var _passwordSetting = string.Format("Machine1:{0},{1}:{2},Machine3:{0}", _otherPasswordCipher, Environment.MachineName, _thisMachinesPasswordCipher);

			var _result = this.c_SUT.Parse(_passwordSetting);

			Assert.IsNotNull(_thisMachinesPassword, _result);
		}


		[Test]
		public void Parse_Where_No_Entry_For_This_Machine_Exists_Results_In_An_Exception()
		{
			Assert.That(() => this.c_SUT.Parse("Machine1:Pwd1,Machine2:OtherPwd"), Throws.TypeOf<ApplicationException>());
		}
	}
}
