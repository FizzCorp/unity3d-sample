
#include <CoreGraphics/CoreGraphics.h>
#include "UnityAppController.h"
#include "UI/UnityView.h"

CGRect FizzCustomComputeSafeArea(UIView* view)
{
	CGSize screenSize = view.bounds.size;
	CGRect screenRect = CGRectMake(0, 0, screenSize.width, screenSize.height);
    
    UIEdgeInsets insets = UIEdgeInsetsMake(0, 0, 0, 0);
    if (@available(iOS 11.0, *)) {
        insets = [view safeAreaInsets];
    }
	
	screenRect.origin.x += insets.left;
	screenRect.origin.y += insets.bottom; // Unity uses bottom left as the origin
	screenRect.size.width -= insets.left + insets.right;
	screenRect.size.height -= insets.top + insets.bottom;
	
	float scale = view.contentScaleFactor;
	screenRect.origin.x *= scale;
	screenRect.origin.y *= scale;
	screenRect.size.width *= scale;
	screenRect.size.height *= scale;
	return screenRect;
}

extern "C" void _FizzGetSafeAreaImpl(float* x, float* y, float* w, float* h)
{
	UIView* view = GetAppController().unityView;
	CGRect area = FizzCustomComputeSafeArea(view);
	*x = area.origin.x;
	*y = area.origin.y;
	*w = area.size.width;
	*h = area.size.height;
}

