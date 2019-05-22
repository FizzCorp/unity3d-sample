using System;
using System.Collections.Generic;

namespace Fizz.ContentModerator
{
	public interface IFizzContentModeratorClient
	{
		void Moderate(IList<string> texts, Action<IList<string>, FizzException> callback);
	}
}
