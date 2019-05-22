using System;
using System.Collections.Generic;

using Fizz.Common;
using Fizz.Common.Json;

namespace Fizz.Moderation.Impl
{
	public class FizzModerationClient : IFizzModerationClient
	{
		private static readonly FizzException ERROR_INVALID_REST_CLIENT = new FizzException (FizzError.ERROR_BAD_ARGUMENT, "invalid_rest_client");
		private static readonly FizzException ERROR_INVALID_TEXT_LIST = new FizzException (FizzError.ERROR_BAD_ARGUMENT, "invalid_text_list");
		private static readonly FizzException ERROR_INVALID_TEXT_LIST_SIZE = new FizzException (FizzError.ERROR_BAD_ARGUMENT, "invalid_text_list_size");
		private static readonly FizzException ERROR_INVALID_RESPONSE_FORMAT = new FizzException (FizzError.ERROR_REQUEST_FAILED, "invalid_response_format");

		private IFizzAuthRestClient _restClient;

		public FizzModerationClient ()
		{
		}

		public void Open (IFizzAuthRestClient client)
		{
			IfClosed (() =>
				{

					if (client == null)
					{
						throw ERROR_INVALID_REST_CLIENT;
					}

					_restClient = client;

				});
		}

		public void Close ()
		{
			IfOpened (() =>
				{
					_restClient = null;
				});
		}

		public void SanitizeText (IList<string> texts, 
			Action<IList<string>, FizzException> callback)
		{
			IfOpened (() =>
				{
					if (texts == null)
					{
						FizzUtils.DoCallback<IList<string>> (null, ERROR_INVALID_TEXT_LIST, callback);
						return;
					}

					if (texts.Count < 1 || texts.Count > 5)
					{
						FizzUtils.DoCallback<IList<string>> (null, ERROR_INVALID_TEXT_LIST_SIZE, callback);
						return;
					}

					foreach (string text in texts) {
						if (string.IsNullOrEmpty(text)) {
							FizzUtils.DoCallback<IList<string>> (null, ERROR_INVALID_TEXT_LIST, callback);
							return;
						}
					}

					try
					{
						string path = FizzConfig.API_PATH_CONTENT_MODERATION;
						JSONArray json = new JSONArray ();
						foreach (string text in texts) {
							json.Add(new JSONData(text));
						}

						_restClient.Post (FizzConfig.API_BASE_URL, path, json.ToString (), (response, ex) =>
							{
								if (ex != null)
								{
									FizzUtils.DoCallback<IList<string>> (null, ex, callback);
								}
								else
								{
									try
									{
										JSONArray textResultArr = JSONNode.Parse (response).AsArray;
										IList<string> moderatedTextList = new List<string> ();
										foreach (JSONNode message in textResultArr.Childs)
										{
											moderatedTextList.Add (message);
										}
										FizzUtils.DoCallback<IList<string>> (moderatedTextList, null, callback);
									}
									catch
									{
										FizzUtils.DoCallback<IList<string>> (null, ERROR_INVALID_RESPONSE_FORMAT, callback);
									}
								}
							});
					}
					catch (FizzException ex)
					{
						FizzUtils.DoCallback<IList<string>> (null, ex, callback);
					}
				});
		}

		private void IfOpened (Action callback)
		{
			if (_restClient != null)
			{
				FizzUtils.DoCallback (callback);
			}
			else
			{
				FizzLogger.W ("Moderation client should be opened before usage.");
			}
		}

		private void IfClosed (Action callback)
		{
			if (_restClient == null)
			{
				FizzUtils.DoCallback (callback);
			}
			else
			{
				FizzLogger.W ("Moderation client should be closed before opening.");
			}
		}
	}
}

