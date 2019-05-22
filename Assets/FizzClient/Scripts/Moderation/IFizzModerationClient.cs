using System;
using System.Collections.Generic;

namespace Fizz.Moderation
{
	public interface IFizzModerationClient
	{
		void SanitizeText(IList<string> texts, Action<IList<string>, FizzException> callback);
	}
}
