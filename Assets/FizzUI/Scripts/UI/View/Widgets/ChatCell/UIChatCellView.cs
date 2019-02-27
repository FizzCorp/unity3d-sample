//
//  UIChatCellView.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using UnityEngine;
using UnityEngine.UI;
using System;
using Fizz.UI.Components.Models;

namespace Fizz.UI.Components
{
	/// <summary>
	/// Cell view for chat messages
	/// </summary>
	public class UIChatCellView : Core.UIComponent
	{
		/// <summary>
		/// Chat cell background image.
		/// </summary>
		[SerializeField] protected Image backgroundImage;
		/// <summary>
		/// Chat message label.
		/// </summary>
		[SerializeField] protected TextWithEmoji messageLabel;
		/// <summary>
		/// Custom view node.
		/// </summary>
		[SerializeField] protected RectTransform customNode;
		/// <summary>
		/// Chat message time label.
		/// </summary>
		[SerializeField] protected Text timeLabel;

		public RectTransform messageRect {
			get { return messageLabel.GetComponent<RectTransform> (); }
		}

		/// <summary>
		/// Gets or sets the row number.
		/// </summary>
		/// <value>The row number.</value>
		public int rowNumber { get; set; }

		protected FizzUIMessage _model;
		protected bool _appTranslationEnabled;

		#region Public Methods

		/// <summary>
		/// Set the data to populate ChatCellView.
		/// </summary>
		/// <param name="model">FIZZChatMessageAction. Please see <see cref="FIZZ.Actions.IFIZZChatMessageAction"/></param>
        public virtual void SetData (FizzUIMessage model, bool appTranslationEnabled)
		{
			_model = model;
			_appTranslationEnabled = appTranslationEnabled;

			if (timeLabel != null) {
                DateTime dt = Utils.GetDateTimeToUnixTime (_model.Created);
				string timeFormat = string.Format ("{0:h:mm tt}", dt);
				timeLabel.text = timeFormat;
			}

			if (customNode != null) {
				customNode.DestroyChildren();
				customNode.gameObject.SetActive (false);
			}			
		}

		public virtual void SetCustomData (RectTransform customView) {
			if (customNode != null) {
				customNode.gameObject.SetActive (true);
				customNode.DestroyChildren();
				if (customView != null) {
					customView.SetParent (customNode, false);
				}
			}
		}

		#endregion
	}
}
