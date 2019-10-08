//
//  UIChatCellModel.cs
//
//  Copyright (c) 2016 Fizz Inc
//


using System.Collections.Generic;
using Fizz.Chat;
using Fizz.Common.Json;

namespace Fizz.UI.Components.Models
{
	public class UIChatCellModel
	{
		public FizzUIMessage Action {
			get;
			set;
		}

		public UIChatCellModelType Type {
			get;
			set;
		}

		public enum UIChatCellModelType
		{
			ChatAction,
			DateHeader
		}
	}

    public class FizzUIMessage : FizzChannelMessage
    {
        public static readonly string KEY_CLIENT_ID = "clientId";

        public FizzUIMessage(long id,
                                  string from,
                                  string nick,
                                  string to,
                                  string body,
                                  Dictionary<string, string> data,
                                  IDictionary<string, string> translations,
                              long created) : base(id, from, nick, to, body, data, translations, created)
        {
            TranslationState = UITranslationState.Translated;
            PublishState = UIChannelMessageState.Pending;

            if (data != null && data.ContainsKey (KEY_CLIENT_ID))
            {
                AlternateId = long.Parse(data[KEY_CLIENT_ID]);
            }
        }

        public long AlternateId { get; protected set; }
        public UIChannelMessageState PublishState { get; set; }
        public UITranslationState TranslationState { get; protected set; }

        public void ToggleTranslationState ()
        {
            if (TranslationState == UITranslationState.Original)
                TranslationState = UITranslationState.Translated;
            else
                TranslationState = UITranslationState.Original;
        }

        public string GetActiveMessage ()
        {
            if (TranslationState == UITranslationState.Original)
            {
                return Body;
            }
            else
            {
                if (Translations != null && Translations.ContainsKey (Core.Utils.GetSystemLanguage ())) {
                    return Translations[Core.Utils.GetSystemLanguage()];
                } else {
                    return Body;
                }
            }
        }

        public FizzUIMessage() { }
    }

    public enum UITranslationState 
    {
        Original,
        Translated
    }

    public enum UIChannelMessageState
    {
        Pending = 1,
        Sent = 2,
        Published = 3
    }
}

