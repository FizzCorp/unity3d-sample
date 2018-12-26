//
//  UITransitionSlide.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using UnityEngine;
using System;
using System.Collections;

namespace FIZZ.UI.Core
{
	public class UITransitionSlide : UITransition
	{
		public enum SlideDirection
		{
			left,
			right,
			bottom,
			top,
			random
		}

		protected struct ScreenOffset
		{
			public Utils.OffsetCalcMode	offsetX;
			public Utils.OffsetCalcMode offsetY;

			public ScreenOffset (Utils.OffsetCalcMode x, Utils.OffsetCalcMode y)
			{
				offsetX = x;
				offsetY = y;
			}
		}

		protected struct AnimationContext
		{
			public Rect rectFrom;
			public Rect rectTo;
			public float elapsedTime;
		}

		protected bool Validate (IUITransitable obj)
		{
			if (obj == null) {
				Debug.LogError ("A transitable needs to be defined for a transition.");
				return false;
			}
			if (obj == null) {
				Debug.LogError ("Transitions require a RectTransform component.");
				return false;
			}
			return true;
		}

		protected virtual void PreExecute (UITransitionContext context, ScreenOffset from, ScreenOffset to, ref AnimationContext animContext)
		{
			Utils.CalcPanelNDCRect (context.to.rect, 
				from.offsetX,
				from.offsetY,
				ref animContext.rectFrom);
			Utils.CalcPanelNDCRect (context.to.rect, 
				to.offsetX,
				to.offsetY,
				ref animContext.rectTo);
			EnableComponent (context.to);

			RectTransform rc = context.to.rect;
			rc.offsetMin = new Vector2 (animContext.rectFrom.xMin, animContext.rectFrom.yMin);
			rc.offsetMax = new Vector2 (animContext.rectFrom.xMax, animContext.rectFrom.yMax);
			rc.SetAsLastSibling ();

			if (context.from != null && context.from.rect != null) {
				Utils.SetInteractable (context.from.rect.transform, false);
				context.from.OnUIDisable ();
			}
		}

		protected virtual void ExecuteStep (UITransitionContext context, ref AnimationContext animContext)
		{
			RectTransform rc = context.to.rect;

			float t = animContext.elapsedTime / context.config.duration;
			if (t > 1.0f) {
				t = 1.0f;
			}

			rc.offsetMin = animContext.rectFrom.GetMinExtents () * (1.0f - t) + animContext.rectTo.GetMinExtents () * t;
			rc.offsetMax = animContext.rectFrom.GetMaxExtents () * (1.0f - t) + animContext.rectTo.GetMaxExtents () * t;

			animContext.elapsedTime += Time.deltaTime;
		}

		protected virtual void PostExecute (UITransitionContext context, ref AnimationContext animContext, bool hideSourcePanel)
		{
			RectTransform rc = context.to.rect;

			rc.offsetMin = animContext.rectTo.GetMinExtents ();
			rc.offsetMax = animContext.rectTo.GetMaxExtents ();

			if (context.from != null && context.from.rect != null) {
				if (hideSourcePanel) {
					context.from.rect.gameObject.SetActive (false);
					context.from.OnUIHide ();
				} 
			}
		}

		protected ScreenOffset CalcScreenOffset (SlideDirection inDir)
		{
			SlideDirection dir = inDir;
			if (inDir == SlideDirection.random) {
				int idx = UnityEngine.Random.Range (0, 4);
				switch (idx) {
				case 0:
					dir = SlideDirection.bottom;
					break;
				case 1:
					dir = SlideDirection.left;
					break;
				case 2:
					dir = SlideDirection.top;
					break;
				case 3:
					dir = SlideDirection.right;
					break;
				}
			}

			switch (dir) {
			case SlideDirection.bottom:
				return new ScreenOffset (Utils.OffsetCalcMode.Centered, Utils.OffsetCalcMode.ScreenBottom);
			case SlideDirection.left:
				return new ScreenOffset (Utils.OffsetCalcMode.ScreenLeft, Utils.OffsetCalcMode.Centered);
			case SlideDirection.top:
				return new ScreenOffset (Utils.OffsetCalcMode.Centered, Utils.OffsetCalcMode.ScreenTop);
			case SlideDirection.right:
				return new ScreenOffset (Utils.OffsetCalcMode.ScreenRight, Utils.OffsetCalcMode.Centered);
			}

			return new ScreenOffset (Utils.OffsetCalcMode.Centered, Utils.OffsetCalcMode.ScreenBottom);
		}
	}

	public class UITransitionSlideIn : UITransitionSlide
	{
		public class Config : UITransitionConfig
		{
			public SlideDirection direction = SlideDirection.right;

			public Config (float duration, SlideDirection inDirection) : base (typeof(UITransitionSlideIn), duration)
			{
				direction = inDirection;
			}
		}

		public override void Do (UITransitionContext context, Action<string,UITransitionContext> onComplete)
		{
			if (!Validate (context.to)) {
				return;
			}
				
			StartCoroutine (Execute (context, onComplete));
		}

		private IEnumerator Execute (UITransitionContext context, Action<string,UITransitionContext> onComplete)
		{
			AnimationContext animContext = new AnimationContext ();
			Config config = (Config)context.config;

			PreExecute (context, 
				CalcScreenOffset (config.direction),
				new ScreenOffset (Utils.OffsetCalcMode.Centered, Utils.OffsetCalcMode.Centered),
				ref animContext);
			while (animContext.elapsedTime < context.config.duration) {
				ExecuteStep (context, ref animContext);
				yield return null;
			}
			onComplete.Invoke (null, context);
			PostExecute (context, ref animContext, true);

			yield break;
		}

	}

	public class UITransitionSlideOnTop : UITransitionSlide
	{
		public class Config : UITransitionConfig
		{
			public SlideDirection direction = SlideDirection.right;

			public Config (float duration, SlideDirection inDirection) : base (typeof(UITransitionSlideOnTop), duration)
			{
				direction = inDirection;
			}
		}

		public override void Do (UITransitionContext context, Action<string,UITransitionContext> onComplete)
		{
			if (!Validate (context.to)) {
				return;
			}
			StartCoroutine (Execute (context, onComplete));
		}

		private IEnumerator Execute (UITransitionContext context, Action<string,UITransitionContext> onComplete)
		{
			AnimationContext animContext = new AnimationContext ();
			Config config = (Config)context.config;

			PreExecute (context, 
				CalcScreenOffset (config.direction),
				new ScreenOffset (Utils.OffsetCalcMode.Centered, Utils.OffsetCalcMode.Centered),
				ref animContext);
			while (animContext.elapsedTime < context.config.duration) {
				ExecuteStep (context, ref animContext);
				yield return null;
			}
			onComplete.Invoke (null, context);
			PostExecute (context, ref animContext, false);

			yield break;
		}
	}

	public class UITransitionSlideOut : UITransitionSlide
	{
		public class Config : UITransitionConfig
		{
			public SlideDirection direction = SlideDirection.right;

			public Config (float duration, SlideDirection inDirection) : base (typeof(UITransitionSlideOut), duration)
			{
				direction = inDirection;
			}
		}

		public override void Do (UITransitionContext context, Action<string,UITransitionContext> onComplete)
		{
			if (!Validate (context.from)) {
				return;
			}
			StartCoroutine (Execute (context, onComplete));
		}

		private IEnumerator Execute (UITransitionContext context, Action<string,UITransitionContext> onComplete)
		{
			AnimationContext animContext = new AnimationContext ();
			Config config = (Config)context.config;

			PreExecute (context, 
				new ScreenOffset (Utils.OffsetCalcMode.Centered, Utils.OffsetCalcMode.Centered),
				CalcScreenOffset (config.direction),
				ref animContext);
			while (animContext.elapsedTime < context.config.duration) {
				ExecuteStep (context, ref animContext);
				yield return null;
			}
			PostExecute (context, ref animContext, false);
			onComplete.Invoke (null, context);

			yield break;
		}

		protected override void PreExecute (UITransitionContext context, ScreenOffset from, ScreenOffset to, ref AnimationContext animContext)
		{
			Utils.CalcPanelNDCRect (context.from.rect, from.offsetX, from.offsetY, ref animContext.rectFrom);
			Utils.CalcPanelNDCRect (context.from.rect, to.offsetX, to.offsetY, ref animContext.rectTo);
			EnableComponent (context.to);

			if (context.from != null && context.from.rect != null) {
				Utils.SetInteractable (context.from.rect.transform, false);
				context.from.OnUIDisable ();
			}
		}

		protected override void ExecuteStep (UITransitionContext context, ref AnimationContext animContext)
		{
			RectTransform rc = context.from.rect;

			float t = animContext.elapsedTime / context.config.duration;
			if (t > 1.0f) {
				t = 1.0f;
			}

			rc.offsetMin = animContext.rectFrom.GetMinExtents () * (1.0f - t) + animContext.rectTo.GetMinExtents () * t;
			rc.offsetMax = animContext.rectFrom.GetMaxExtents () * (1.0f - t) + animContext.rectTo.GetMaxExtents () * t;

			animContext.elapsedTime += Time.deltaTime;
		}

		protected override void PostExecute (UITransitionContext context, ref AnimationContext animContext, bool hideSourcePanel)
		{
			RectTransform rc = context.from.rect;

			rc.offsetMin = animContext.rectTo.GetMinExtents ();
			rc.offsetMax = animContext.rectTo.GetMaxExtents ();

			if (context.from != null && context.from.rect != null) {
				context.from.rect.gameObject.SetActive (false);
				context.from.OnUIHide ();
			}
		}
	}
}