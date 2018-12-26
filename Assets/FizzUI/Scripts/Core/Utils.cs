//
//  Utils.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using UnityEngine;
using System;
using Fizz.Common.Json;

namespace FIZZ.UI.Core
{
	public static class Utils
	{
		#region UI

		public enum OffsetCalcMode
		{
			Centered,
			ScreenTop,
			ScreenRight,
			ScreenLeft,
			ScreenBottom
		}

		private static float CalcNDCMargin (OffsetCalcMode mode, float dim, float parentDim)
		{
			dim = parentDim = 1;
			float margin = 0.0f;

			switch (mode) {
			case OffsetCalcMode.Centered:
				margin = (1.0f - (dim / parentDim)) * 0.5f;
				break;
			case OffsetCalcMode.ScreenRight:
			case OffsetCalcMode.ScreenTop:
				margin = 1.0f;
				break;
			case OffsetCalcMode.ScreenLeft:
			case OffsetCalcMode.ScreenBottom:
				margin = -1.0f;
				break;
			}

			return margin;
		}

		public static void CalcPanelNDCRect (RectTransform rc, OffsetCalcMode modeX, OffsetCalcMode modeY, ref Rect outRect)
		{
			if (rc == null) {
				Debug.LogError ("RecTransform component required to calculate NDC rect.");
				return;
			}

			outRect.Set (0.0f, 0.0f, 0.0f, 0.0f);

			RectTransform rcParent = rc.parent != null ? rc.parent.GetComponent<RectTransform> () : null;
			if (rc == null || rcParent == null) {
				Debug.LogError ("Can not run transition on component with no RectTransform or parent object.");
				return;
			}

			Vector2 dim = Vector2.zero;
			Utils.CalcRectTransformDimensions (rc, ref dim);
			Vector2 parentDim = Vector2.zero;
			Utils.CalcRectTransformDimensions (rcParent, ref parentDim);

			float normalizedMarginX = CalcNDCMargin (modeX, dim.x, parentDim.x);// modeX == OffsetCalcMode.Centered ? (1.0f-(dim.x/parentDim.x))*0.5f : 1.0f;
			float offsetMinX = (normalizedMarginX -  0/*rc.anchorMin.x*/) * dim.x;
			float offsetMaxX = (normalizedMarginX - 1/*rc.anchorMax.x*/) * dim.x + dim.x;

			float normalizedMarginY = CalcNDCMargin (modeY, dim.y, parentDim.y);//modeY == OffsetCalcMode.Centered ? (1.0f-(dim.y/parentDim.y))*0.5f : 1.0f;
			float offsetMinY = (normalizedMarginY - 0/*rc.anchorMin.y*/) * dim.y;
			float offsetMaxY = (normalizedMarginY - 1/*rc.anchorMax.y*/) * dim.y + dim.y;

			outRect.xMin = offsetMinX;
			outRect.yMin = offsetMinY;
			outRect.xMax = offsetMaxX;
			outRect.yMax = offsetMaxY;
		}

		public static void CalcRectTransformDimensions (RectTransform rect, ref Vector2 dim)
		{
			dim.Set (0.0f, 0.0f);

			if (rect == null) {
				return;
			}

			Canvas canvas = rect.gameObject.GetComponent<Canvas> ();
			if (canvas == null) {
				if (rect.parent != null) {
					Vector2 parentDim = Vector2.zero;
					CalcRectTransformDimensions (rect.parent.GetComponent<RectTransform> (), ref parentDim);

					Vector2 screenAnchorMin = Vector2.Scale (rect.anchorMin, parentDim);
					Vector2 screenAnchorMax = Vector2.Scale (rect.anchorMax, parentDim);
					Vector2 screenMin = screenAnchorMin + rect.offsetMin;
					Vector2 screenMax = screenAnchorMax + rect.offsetMax;
					dim = screenMax - screenMin;
				}
			} else {
				if (canvas.renderMode == RenderMode.WorldSpace) {
					dim = rect.sizeDelta;
				} else if (canvas.renderMode == RenderMode.ScreenSpaceOverlay ||
				           canvas.renderMode == RenderMode.ScreenSpaceCamera) {
					dim = new Vector2 (Screen.width, Screen.height);
				} 
			}
		}

		public static void SetInteractable (Transform inObject, bool state)
		{
			CanvasGroup cv = inObject.gameObject.GetComponent<CanvasGroup> ();
			if (cv == null)
				cv = inObject.gameObject.AddComponent<CanvasGroup> ();
			cv.interactable = state;
		}

		public static void IterateComponents<T> (Transform inObject, Action<T> iteratorCB)
		{
			T[] components = inObject.gameObject.GetComponents<T> ();
			foreach (T comp in components) {
				iteratorCB.Invoke (comp);
			}

			for (int ci = 0; ci < inObject.childCount; ci++) {
				Transform child = inObject.GetChild (ci);
				IterateComponents (child, iteratorCB);
			}
		}

		#endregion

		public static JSONClass GetJsonClass (string jsonString)
		{
			JSONClass jsonClass = null;
			try {
				if (string.IsNullOrEmpty (jsonString)) {
					// Raise Exception
				} else {
					jsonClass = JSON.Parse (jsonString).AsObject;
				}
			} catch (Exception ex) {
				UnityEngine.Debug.Log ("FIZZUIUtility: GetJsonClass: jsonString = " + jsonString + " Exception = " + ex.ToString ());
			}
			return jsonClass;
		}

		public static string GetSystemLanguage ()
		{
			string languageCode = string.Empty;
			SystemLanguage systemLanguage = Application.systemLanguage;
			switch (systemLanguage) {
			case SystemLanguage.English:
				languageCode = "en";
				break;
			case SystemLanguage.French:
				languageCode = "fr";
				break;
			case SystemLanguage.Afrikaans:
				languageCode = "af";
				break;
			case SystemLanguage.Arabic:
				languageCode = "ar";
				break;
			case SystemLanguage.Basque:
				languageCode = "eu";
				break;
			case SystemLanguage.Belarusian:
				languageCode = "be";
				break;
			case SystemLanguage.Bulgarian:
				languageCode = "bg";
				break;
			case SystemLanguage.Catalan:
				languageCode = "bg";
				break;
			case SystemLanguage.Czech:
			languageCode = "cs";
				break;
			case SystemLanguage.Danish:
			languageCode = "da";
				break;
			case SystemLanguage.Dutch:
			languageCode = "nl";
				break;
			case SystemLanguage.Estonian:
			languageCode = "et";
				break;
			case SystemLanguage.Finnish:
			languageCode = "fi";
				break;
			case SystemLanguage.German:
			languageCode = "de";
				break;
			case SystemLanguage.Greek:
			languageCode = "el";
				break;
			case SystemLanguage.Hebrew:
			languageCode = "he";
				break;
			
			case SystemLanguage.Indonesian:
			languageCode = "id";
				break;
			case SystemLanguage.Italian:
			languageCode = "it";
				break;
			case SystemLanguage.Japanese:
			languageCode = "ja";
				break;
			case SystemLanguage.Korean:
			languageCode = "ko";
				break;
			case SystemLanguage.Latvian:
			languageCode = "lv";
				break;
			case SystemLanguage.Lithuanian:
			languageCode = "lt";
				break;
			case SystemLanguage.Norwegian:
			languageCode = "no";
				break;
			case SystemLanguage.Polish:
			languageCode = "pl";
				break;
			case SystemLanguage.Portuguese:
			languageCode = "pt";
				break;
			case SystemLanguage.Romanian:
			languageCode = "ro";
				break;
			case SystemLanguage.Russian:
			languageCode = "ru";
				break;
			
			case SystemLanguage.Slovak:
			languageCode = "sk";
				break;
			case SystemLanguage.Slovenian:
			languageCode = "sl";
				break;
			case SystemLanguage.Spanish:
			languageCode = "es";
				break;
			case SystemLanguage.Swedish:
			languageCode = "sw";
				break;
			case SystemLanguage.Thai:
			languageCode = "th";
				break;
			case SystemLanguage.Turkish:
			languageCode = "tr";
				break;
			case SystemLanguage.Ukrainian:
			languageCode = "uk";
				break;
			case SystemLanguage.Vietnamese:
			languageCode = "vi";
				break;
			case SystemLanguage.ChineseSimplified:
			languageCode = "zh-CHS";
				break;
			case SystemLanguage.ChineseTraditional:
			languageCode = "zh-CHT";
				break;
			case SystemLanguage.Hungarian:
			languageCode = "hu";
				break;
			case SystemLanguage.Unknown:
				languageCode = "en";
				break;
			default:
				languageCode = "en";
				break;
			}
			return languageCode;
		}
	}

	public static class FizzExtensions
	{
		public static Vector2 GetMinExtents (this Rect inRect)
		{
			return new Vector2 (inRect.xMin, inRect.yMin);
		}

		public static Vector2 GetMaxExtents (this Rect inRect)
		{
			return new Vector2 (inRect.xMax, inRect.yMax);
		}

		public static void SetMinExtents (this Rect inRect, Vector2 inExtents)
		{
			inRect.xMin = inExtents.x;
			inRect.yMin = inExtents.y;
		}

		public static void SetMaxExtents (this Rect inRect, Vector2 inExtents)
		{
			inRect.xMax = inExtents.x;
			inRect.yMax = inExtents.y;
		}
	}
}