//
//  UIOwnChatCellView.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using Fizz.UI.Components.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI.Components
{
	/// <summary>
	/// User interface own chat cell view.
	/// </summary>
	public class UIRightChatCellView : UIChatCellView
	{
		/// <summary>
		/// Chat message delivery status image.
		/// </summary>
		[SerializeField] Image deliveryStatusImage;
		/// <summary>
		/// Chat message sent status image.
		/// </summary>
		[SerializeField] Image sentStatusImage;

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

            LoadChatMessageAction ();
		}

		#endregion

		#region Private Methods

		private void LoadChatMessageAction () {
			messageLabel.gameObject.SetActive (true);

            // if (Utils.ShouldFixRTLMessage (chatMessageAction)) {
                messageLabel.text = ArabicSupport.ArabicFixer.Fix ( _model.GetActiveMessage ());
			// } else {
                messageLabel.text = _model.Body;
			// }

			if (_model.PublishState == UIChannelMessageState.Pending) {
				sentStatusImage.gameObject.SetActive (false);
				deliveryStatusImage.gameObject.SetActive (false);
			} else if (_model.PublishState == UIChannelMessageState.Sent) {
				sentStatusImage.gameObject.SetActive (true);
				deliveryStatusImage.gameObject.SetActive (false);
			} else if (_model.PublishState == UIChannelMessageState.Published) {
				sentStatusImage.gameObject.SetActive (false);
				deliveryStatusImage.gameObject.SetActive (true);
			}
		}

		#endregion
	}


}