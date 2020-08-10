using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TLSharp.Core;

namespace GuideOneServer.Helpers
{
	public class TelegramClientHelper
	{
		public string api_hash;
		public int api_id;
		public string phone_number;
		public string code;
	//	async void func(string phone)
	//	{
		
	//		var store = new FileSessionStore();
	//		var client = new TelegramClient(api_id, api_hash);
	//		await client.ConnectAsync();
	//				/*
	//					* phoneNumber — ваш номер телефона в интернациональном формате (например, 79184981723)
	//					* code — код который вы получите от Telegram, после выполнения метода SendCodeRequest
	//				 */
	//		var hash = await client.SendCodeRequestAsync(phone_number);
	//		var user = await client.MakeAuthAsync(phone_number, hash, code);

	//		if (await client.IsPhoneRegisteredAsync(phone))
	//		var userByPhoneId = await client.ImportContactsAsync(phone);
	//		var userByUserNameId = await await client.ImportByUserName("userName");
	//		await client.SendMessageAsync(userId, "Hello Habr!");
	//}
}
}
