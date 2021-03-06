//
//  IFIZZUIChatDataSource.cs
//
//  Copyright (c) 2016 Fizz Inc
//

using Fizz.Chat;
using UnityEngine;

namespace Fizz.UI.Components
{
	/// <summary>
	/// Data source for chat user interface
	/// </summary>
	public interface IUIChatDataSource
	{
		/// <summary>
		/// Get custom drawable RectTransform for custom message
		/// which will be added to chat cell custom node container
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		RectTransform GetCustomMessageDrawable (FizzChannelMessage message);
	}
}