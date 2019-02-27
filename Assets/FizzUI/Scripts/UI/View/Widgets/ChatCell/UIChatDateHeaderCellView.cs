//
//  UIChatDateHeaderCellView.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using Fizz.UI.Components.Models;

namespace Fizz.UI.Components
{
	public class UIChatDateHeaderCellView : UIChatCellView
	{
		protected override void Awake ()
		{
			base.Awake ();
		}

		#region Public Methods

		/// <summary>
		/// Set the data to populate ChatCellView.
		/// </summary>
		/// <param name="model">FIZZChatMessageAction. Please see <see cref="FIZZ.Actions.IFIZZChatMessageAction"/></param>
		public override void SetData (FizzUIMessage model, bool appTranslationEnabled)
		{
			base.SetData (model, appTranslationEnabled);

            messageLabel.text = Utils.GetFormattedTimeForUnixTimeStamp (_model.Created);
		}

		#endregion
	}
}