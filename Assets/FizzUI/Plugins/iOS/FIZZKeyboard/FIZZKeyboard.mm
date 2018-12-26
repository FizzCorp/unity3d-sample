//
//  FIZZKeyboard.mm
//  Unity-iPhone
//
//  Created by Hassan Ali on 4/6/17.
//
//

#include "FIZZKeyboard.h"
#include "DisplayManager.h"
#include "UnityForwardDecls.h"

#include <string>

#ifndef FILTER_EMOJIS_IOS_KEYBOARD
#define FILTER_EMOJIS_IOS_KEYBOARD 0
#endif


static FIZZKeyboardDelegate*	_keyboard = nil;

static bool					_shouldHideInput = false;
static bool					_shouldHideInputChanged = false;
static const unsigned       kToolBarHeight = 40;

@implementation FIZZKeyboardDelegate
{
    // UI handling
    // in case of single line we use UITextField inside UIToolbar
    // in case of multi-line input we use UITextView with UIToolbar as accessory view
    // toolbar buttons are kept around to prevent releasing them
    // tvOS does not support multiline input thus only UITextField option is implemented

    UITextView*		textView;
    
    PHFComposeBarView* composerBar;
    
    UIToolbar*		viewToolbar;
    NSArray*		viewToolbarItems;

    
    UITextField*	textField;
    
    // keep toolbar items for both single- and multi- line edit in NSArray to make sure they are kept around

    UIToolbar*		fieldToolbar;
    NSArray*		fieldToolbarItems;

    
    // inputView is view used for actual input (it will be responder): UITextField [single-line] or UITextView [multi-line]
    // editView is the "root" view for keyboard: UIToolbar [single-line] or UITextView [multi-line]
    UIView*			inputView;
    UIView*			editView;
    
    
    CGRect			_area;
    NSString*		initialText;
    
    UIKeyboardType	keyboardType;
    
    BOOL			_multiline;
    BOOL			_inputHidden;
    BOOL			_active;
    BOOL			_done;
    BOOL			_canceled;
    
    // not pretty but seems like easiest way to keep "we are rotating" status
    BOOL			_rotating;
    
    BOOL            _isCustomMessageInput;
}

@synthesize area;
@synthesize active		= _active;
@synthesize done		= _done;
@synthesize canceled	= _canceled;
@synthesize text;

@synthesize sendMessageCallback = _sendMessageCallback;
@synthesize stickerCallback = _stickerCallback;

- (BOOL)textFieldShouldReturn:(UITextField*)textFieldObj
{
    [self hide];
    return YES;
}
- (void)textInputDone:(id)sender
{
    [self hide];
}
- (void)textInputCancel:(id)sender
{
    _canceled = true;
    [self hide];
}

- (BOOL)textViewShouldBeginEditing:(UITextView*)view
{
    return YES;
}

- (void)composeBarViewDidPressButton:(PHFComposeBarView *)composeBarView
{
    if (_sendMessageCallback) {
        _sendMessageCallback([composeBarView.text UTF8String]);
        composeBarView.text = @"";
    }
}

- (void)composeBarViewDidPressUtilityButton:(PHFComposeBarView *)composeBarView
{
    if (_stickerCallback) {
        _stickerCallback ();
    }
}

- (void)keyboardDidShow:(NSNotification*)notification
{
    if (notification.userInfo == nil || inputView == nil)
        return;
    
    CGRect srcRect	= [[notification.userInfo objectForKey:UIKeyboardFrameEndUserInfoKey] CGRectValue];
    CGRect rect		= [UnityGetGLView() convertRect:srcRect fromView:nil];
    rect = [self customComputeSafeArea:rect];
    [self positionInput:rect x:rect.origin.x y:rect.origin.y];
    _active = YES;
}

- (void)keyboardWillHide:(NSNotification*)notification
{
    [self systemHideKeyboard];
}
- (void)keyboardDidChangeFrame:(NSNotification*)notification
{
    _active = YES;
    
    CGRect srcRect	= [[notification.userInfo objectForKey:UIKeyboardFrameEndUserInfoKey] CGRectValue];
    CGRect rect		= [UnityGetGLView() convertRect:srcRect fromView: nil];
    
    rect = [self customComputeSafeArea:rect];
    
    if(rect.origin.y >= [UnityGetGLView() bounds].size.height)
        [self systemHideKeyboard];
    else
        [self positionInput:rect x:rect.origin.x y:rect.origin.y];
}

+ (void)Initialize
{
    NSAssert(_keyboard == nil, @"[FIZZKeyboardDelegate Initialize] called after creating keyboard");
    if(!_keyboard)
        _keyboard = [[FIZZKeyboardDelegate alloc] init];
}

+ (FIZZKeyboardDelegate*)Instance
{
    if(!_keyboard)
        _keyboard = [[FIZZKeyboardDelegate alloc] init];
    
    return _keyboard;
}

struct CreateToolbarResult
{
    UIToolbar*	toolbar;
    NSArray*	items;
};
- (CreateToolbarResult)createToolbarWithView:(UIView*)view
{
    UIToolbar* toolbar = [[UIToolbar alloc] initWithFrame:CGRectMake(0,160,320, kToolBarHeight)];
    UnitySetViewTouchProcessing(toolbar, touchesIgnored);
    toolbar.hidden = NO;
    
    UIBarButtonItem* inputItem	= view ? [[UIBarButtonItem alloc] initWithCustomView:view] : nil;
    UIBarButtonItem* doneItem	= [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemDone target:self action:@selector(textInputDone:)];
    UIBarButtonItem* cancelItem	= [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemCancel target:self action:@selector(textInputCancel:)];
    
    NSArray* items = view ? @[inputItem, doneItem, cancelItem] : @[doneItem, cancelItem];
    toolbar.items = items;
    
    inputItem = nil;
    doneItem = nil;
    cancelItem = nil;
    
    CreateToolbarResult ret = {toolbar, items};
    return ret;
}

- (id)init
{
    NSAssert(_keyboard == nil, @"You can have only one instance of FIZZKeyboardDelegate");
    self = [super init];
    if(self)
    {
        textView = [[UITextView alloc] initWithFrame:CGRectMake(0, 480, 480, 30)];
        textView.delegate = self;
        textView.font = [UIFont systemFontOfSize:18.0];
        textView.hidden = YES;
        
        CGRect viewBounds = [UnityGetGLView() frame];
        viewBounds = [self customComputeSafeArea:viewBounds];
        CGRect frame = CGRectMake(0.0f,
                                  viewBounds.size.height - PHFComposeBarViewInitialHeight,
                                  viewBounds.size.width,
                                  PHFComposeBarViewInitialHeight);
        composerBar = [[PHFComposeBarView alloc] initWithFrame:frame];
        [composerBar setMaxLinesCount:5];
        [composerBar setMaxCharCount:165];
        [composerBar setUtilityButtonImage:nil];
//        [composerBar setPlaceholder:@"Write a message..."];
        [composerBar setDelegate:self];
        
        UnitySetViewTouchProcessing(composerBar, touchesIgnored);
        
        textField = [[UITextField alloc] initWithFrame:CGRectMake(0,0,120,30)];
        textField.delegate = self;
        textField.borderStyle = UITextBorderStyleRoundedRect;
        textField.font = [UIFont systemFontOfSize:20.0];
        textField.clearButtonMode = UITextFieldViewModeWhileEditing;
        
#define CREATE_TOOLBAR(t, i, v)									\
do {															\
CreateToolbarResult res = [self createToolbarWithView:v];	\
t = res.toolbar;											\
i = res.items;												\
} while(0)
        
        CREATE_TOOLBAR(viewToolbar, viewToolbarItems, nil);
        CREATE_TOOLBAR(fieldToolbar, fieldToolbarItems, textField);
        
#undef CREATE_TOOLBAR
        
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(keyboardDidShow:) name:UIKeyboardDidShowNotification object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(keyboardWillHide:) name:UIKeyboardWillHideNotification object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(keyboardDidChangeFrame:) name:UIKeyboardDidChangeFrameNotification object:nil];
        
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(textInputDone:) name:UITextFieldTextDidEndEditingNotification object:nil];
    }
    
    return self;
}

- (void) setTextInputTraits: (id<UITextInputTraits>) traits
                  withParam: (KeyboardShowParam) param
                    withCap: (UITextAutocapitalizationType) capitalization
{
    traits.keyboardType	= param.keyboardType;
    traits.autocorrectionType = param.autocorrectionType;
    traits.secureTextEntry = param.secure;
    traits.keyboardAppearance = param.appearance;
    traits.autocapitalizationType = capitalization;
}

- (void)setKeyboardParams:(KeyboardShowParam)param
{
    if(_active)
        [self hide];
    
    initialText = param.text ? [[NSString alloc] initWithUTF8String: param.text] : @"";
    NSString *placeHolder =  param.placeholder ? [[NSString alloc] initWithUTF8String: param.placeholder] : @"";
    
    UITextAutocapitalizationType capitalization = UITextAutocapitalizationTypeSentences;
    if(param.keyboardType == UIKeyboardTypeURL || param.keyboardType == UIKeyboardTypeEmailAddress)
        capitalization = UITextAutocapitalizationTypeNone;
    
    _multiline = param.multiline;
    if (_multiline)
    {
        if (_isCustomMessageInput) {
            composerBar.text = initialText;
            [composerBar setPlaceholder:placeHolder];
            [self setTextInputTraits:composerBar.textView withParam:param withCap:capitalization];
        } else {
            textView.text = initialText;
            [self setTextInputTraits:textView withParam:param withCap:capitalization];
        }
    }
    else
    {
        textField.text = initialText;
        [self setTextInputTraits:textField withParam:param withCap:capitalization];
        textField.placeholder = [NSString stringWithUTF8String:param.placeholder];
    }
    
    if (_multiline) {
        inputView = _isCustomMessageInput ? composerBar : textView;
        editView = _isCustomMessageInput ? composerBar : textView;
    } else {
        inputView = textField;
        editView = fieldToolbar;
    }
    
    [self shouldHideInput:_shouldHideInput];
    
    _done		= NO;
    _canceled	= NO;
    _active		= NO;
}

// we need to show/hide keyboard to react to orientation too, so extract we extract UI fiddling

- (void)showUI
{
    NSLog(@"INPUT %d", _isCustomMessageInput);
    
    // if we unhide everything now the input will be shown smaller then needed quickly (and resized later)
    // so unhide only when keyboard is actually shown (we will update it when reacting to ios notifications)
    editView.hidden = YES;
    
    [UnityGetGLView() addSubview:editView];
    [inputView becomeFirstResponder];
}
- (void)hideUI
{
    [inputView resignFirstResponder];
    
    [editView removeFromSuperview];
    editView.hidden = YES;
}
- (void)systemHideKeyboard
{
    // when we are rotating os will bombard us with keyboardWillHide: and keyboardDidChangeFrame:
    // ignore all of them (we do it here only to simplify code: we call systemHideKeyboard only from these notification handlers)
    if(_rotating)
        return;
    
    _active = editView.isFirstResponder;
    editView.hidden = YES;
    
    _area = CGRectMake(0,0,0,0);
}

- (void)show
{
    [self showUI];
}
- (void)hide
{
    [self hideUI];
    _done = YES;
    _isCustomMessageInput = NO;
    _sendMessageCallback = NULL;
    _stickerCallback = NULL;
}

- (void)updateInputHidden
{
    if(_shouldHideInputChanged)
    {
        [self shouldHideInput:_shouldHideInput];
        _shouldHideInputChanged = false;
    }
    
    textField.returnKeyType = _inputHidden ? UIReturnKeyDone : UIReturnKeyDefault;
    
    editView.hidden		= _inputHidden ? YES : NO;
    inputView.hidden	= _inputHidden ? YES : NO;
}

- (void)positionInput:(CGRect)kbRect x:(float)x y:(float)y
{
    if(_multiline)
    {
        
        // use smaller area for iphones and bigger one for ipads
        int height = UnityDeviceDPI() > 300 ? 75 : 100;
        
        editView.frame	= CGRectMake(x, y - height, kbRect.size.width, height);
    }
    else
    {
        editView.frame	= CGRectMake(x, y - kToolBarHeight, kbRect.size.width, kToolBarHeight);
        inputView.frame	= CGRectMake(inputView.frame.origin.x,
                                     inputView.frame.origin.y,
                                     kbRect.size.width - 3*18 - 2*50,
                                     inputView.frame.size.height);
    }

    _area = CGRectMake(x, y, kbRect.size.width, kbRect.size.height);
    NSLog(@"positionInput  %f", _area.size.height);
    [self updateInputHidden];
}

- (CGRect) customComputeSafeArea:(CGRect) viewRect
{
    UIEdgeInsets insets = UIEdgeInsetsMake(0, 0, 0, 0);
    if (@available(iOS 11.0, *)) {
        insets = [UnityGetGLView() safeAreaInsets];
    }
    
    viewRect.origin.x += insets.left;
    viewRect.size.width -= insets.left + insets.right;
    return viewRect;
}

- (CGRect)queryArea
{
    return editView.hidden ? _area : CGRectUnion(_area, editView.frame);
}

+ (void)StartReorientation
{
    if(_keyboard && _keyboard.active)
        _keyboard->_rotating = YES;
}

+ (void)FinishReorientation
{
    if(_keyboard)
        _keyboard->_rotating = NO;
}

- (NSString*)getText
{
    if (_canceled)
        return initialText;
    else
    {
        if (_multiline) {
            return _isCustomMessageInput ? [composerBar text] : [textView text];
        } else {
            return [textField text];
        }
    }
}

- (void) setTextWorkaround:(id<UITextInput>)textInput text:(NSString*)newText
{
    UITextPosition* begin = [textInput beginningOfDocument];
    UITextPosition* end = [textInput endOfDocument];
    UITextRange* allText = [textInput textRangeFromPosition:begin toPosition:end];
    [textInput setSelectedTextRange:allText];
    [textInput insertText:newText];
}

- (void)setText:(NSString*)newText
{
    // We can't use setText on iOS7 because it does not update the undo stack.
    // We still prefer setText on other iOSes, because an undo operation results
    // in a smaller selection shown on the UI
    if(_ios70orNewer && !_ios80orNewer) {
        if (_multiline) {
            [self setTextWorkaround:(_isCustomMessageInput? composerBar.textView : textView) text:newText];
        } else {
            [self setTextWorkaround:textField text:newText];
        }
    }
    
    if(_multiline) {
        if (_isCustomMessageInput) {
            composerBar.text = newText;
        } else {
            textView.text = newText;
        }
    } else {
        textField.text = newText;
    }
}

- (void)shouldHideInput:(BOOL)hide
{
    if(hide)
    {
        switch(keyboardType)
        {
            case UIKeyboardTypeDefault:                 hide = YES;	break;
            case UIKeyboardTypeASCIICapable:            hide = YES;	break;
            case UIKeyboardTypeNumbersAndPunctuation:   hide = YES;	break;
            case UIKeyboardTypeURL:                     hide = YES;	break;
            case UIKeyboardTypeNumberPad:               hide = NO;	break;
            case UIKeyboardTypePhonePad:                hide = NO;	break;
            case UIKeyboardTypeNamePhonePad:            hide = NO;	break;
            case UIKeyboardTypeEmailAddress:            hide = YES;	break;
            default:                                    hide = NO;	break;
        }
    }
    
    _inputHidden = hide;
}

- (void)setCustomMessageInput:(BOOL)enable withMessageCallback:(SendMessageCallback)messageCallback andStickerCallbacl:(StickerCallback) stickerCallback
{
    _isCustomMessageInput = enable;
    _sendMessageCallback = messageCallback;
    _stickerCallback = stickerCallback;
}

#if FILTER_EMOJIS_IOS_KEYBOARD

static bool StringContainsEmoji(NSString *string);
-(BOOL)textField:(UITextField*)textField shouldChangeCharactersInRange:(NSRange)range replacementString:(NSString*)string_
{
    return !StringContainsEmoji(string_);
}
- (BOOL)textView:(UITextView*)textView shouldChangeTextInRange:(NSRange)range replacementText:(NSString*)text_
{
    return !StringContainsEmoji(text_);
}

#endif // FILTER_EMOJIS_IOS_KEYBOARD

@end



//==============================================================================
//
//  Unity Interface:

extern "C" void FIZZUnityKeyboard_Create(unsigned keyboardType, int autocorrection, int multiline, int secure, int alert, const char* text, const char* placeholder)
{
    static const UIKeyboardType keyboardTypes[] =
    {
        UIKeyboardTypeDefault,
        UIKeyboardTypeASCIICapable,
        UIKeyboardTypeNumbersAndPunctuation,
        UIKeyboardTypeURL,
        UIKeyboardTypeNumberPad,
        UIKeyboardTypePhonePad,
        UIKeyboardTypeNamePhonePad,
        UIKeyboardTypeEmailAddress,
    };
    
    static const UITextAutocorrectionType autocorrectionTypes[] =
    {
        UITextAutocorrectionTypeNo,
        UITextAutocorrectionTypeDefault,
    };
    
    static const UIKeyboardAppearance keyboardAppearances[] =
    {
        UIKeyboardAppearanceDefault,
        UIKeyboardAppearanceAlert,
    };
    
    KeyboardShowParam param =
    {
        text, placeholder,
        keyboardTypes[keyboardType],
        autocorrectionTypes[autocorrection],
        keyboardAppearances[alert],
        (BOOL)multiline, (BOOL)secure
    };
    
    [[FIZZKeyboardDelegate Instance] setKeyboardParams:param];
}

extern "C" void FIZZUnityKeyboard_Show()
{
    // do not send hide if didnt create keyboard
    // TODO: probably assert?
    if(!_keyboard)
        return;
    
    [[FIZZKeyboardDelegate Instance] show];
}
extern "C" void FIZZUnityKeyboard_Hide()
{
    // do not send hide if didnt create keyboard
    // TODO: probably assert?
    if(!_keyboard)
        return;
    
    [[FIZZKeyboardDelegate Instance] hide];
}

extern "C" void FIZZUnityKeyboard_SetText(const char* text)
{
    [FIZZKeyboardDelegate Instance].text = [NSString stringWithUTF8String: text];
}

extern "C" const char * FIZZUnityKeyboard_GetText()
{
    return [[FIZZKeyboardDelegate Instance].text UTF8String];
}

extern "C" int FIZZUnityKeyboard_IsActive()
{
    return (_keyboard && _keyboard.active) ? 1 : 0;
}

extern "C" int FIZZUnityKeyboard_IsDone()
{
    return (_keyboard && _keyboard.done) ? 1 : 0;
}

extern "C" int FIZZUnityKeyboard_WasCanceled()
{
    return (_keyboard && _keyboard.canceled) ? 1 : 0;
}

extern "C" void FIZZUnityKeyboard_SetInputHidden(int hidden)
{
    _shouldHideInput		= hidden;
    _shouldHideInputChanged	= true;
    
    // update hidden status only if keyboard is on screen to avoid showing input view out of nowhere
    if(_keyboard && _keyboard.active)
        [_keyboard updateInputHidden];
}

extern "C" int FIZZUnityKeyboard_IsInputHidden()
{
    return _shouldHideInput ? 1 : 0;
}

extern "C" void FIZZUnityKeyboard_GetRect(float* x, float* y, float* w, float* h)
{
    CGRect area = (_keyboard && _keyboard.active) ? _keyboard.area : CGRectMake(0,0,0,0);
    // convert to unity coord system
    
    float	multX	= (float)GetMainDisplaySurface()->targetW / UnityGetGLView().bounds.size.width;
    float	multY	= (float)GetMainDisplaySurface()->targetH / UnityGetGLView().bounds.size.height;
    
    NSLog(@"Area %f %f", area.size.height, multY);
    *x = 0;
    *y = area.origin.y * multY;
    *w = area.size.width * multX;
    *h = area.size.height * multY;
}

extern "C" void FIZZUnityKeyboard_CustomMessageInput (bool enable, SendMessageCallback messageCallback, StickerCallback stickerCallback)
{
    [[FIZZKeyboardDelegate Instance] setCustomMessageInput:enable withMessageCallback:messageCallback andStickerCallbacl:stickerCallback];
}

//==============================================================================
//
//  Emoji Filtering: unicode magic

#if FILTER_EMOJIS_IOS_KEYBOARD
static bool StringContainsEmoji(NSString *string)
{
    __block BOOL returnValue = NO;
    
    [string enumerateSubstringsInRange:NSMakeRange(0, string.length)
                               options:NSStringEnumerationByComposedCharacterSequences
                            usingBlock:^(NSString* substring, NSRange substringRange, NSRange enclosingRange, BOOL* stop)
     {
         const unichar hs = [substring characterAtIndex:0];
         const unichar ls = substring.length > 1 ? [substring characterAtIndex:1] : 0;
         
#define IS_IN(val, min, max) (((val) >= (min)) && ((val) <= (max)))
         
         if(IS_IN(hs, 0xD800, 0xDBFF))
         {
             if(substring.length > 1)
             {
                 const int uc = ((hs - 0xD800) * 0x400) + (ls - 0xDC00) + 0x10000;
                 
                 // Musical: [U+1D000, U+1D24F]
                 // Enclosed Alphanumeric Supplement: [U+1F100, U+1F1FF]
                 // Enclosed Ideographic Supplement: [U+1F200, U+1F2FF]
                 // Miscellaneous Symbols and Pictographs: [U+1F300, U+1F5FF]
                 // Supplemental Symbols and Pictographs: [U+1F900, U+1F9FF]
                 // Emoticons: [U+1F600, U+1F64F]
                 // Transport and Map Symbols: [U+1F680, U+1F6FF]
                 if(IS_IN(uc, 0x1D000, 0x1F9FF))
                     returnValue = YES;
             }
         }
         else if(substring.length > 1 && ls == 0x20E3)
         {
             // emojis for numbers: number + modifier ls = U+20E3
             returnValue = YES;
         }
         else
         {
             if(		// Latin-1 Supplement
                hs == 0x00A9 || hs == 0x00AE
                // General Punctuation
                ||	hs == 0x203C || hs == 0x2049
                // Letterlike Symbols
                ||	hs == 0x2122 || hs == 0x2139
                // Arrows
                ||	IS_IN(hs, 0x2194, 0x2199) || IS_IN(hs, 0x21A9, 0x21AA)
                // Miscellaneous Technical
                ||	IS_IN(hs, 0x231A, 0x231B) || IS_IN(hs, 0x23E9, 0x23F3) || IS_IN(hs, 0x23F8, 0x23FA) || hs == 0x2328 || hs == 0x23CF
                // Geometric Shapes
                ||	IS_IN(hs, 0x25AA, 0x25AB) || IS_IN(hs, 0x25FB, 0x25FE) || hs == 0x25B6 || hs == 0x25C0
                // Miscellaneous Symbols
                ||	IS_IN(hs, 0x2600, 0x2604) || IS_IN(hs, 0x2614, 0x2615) || IS_IN(hs, 0x2622, 0x2623) || IS_IN(hs, 0x262E, 0x262F)
                ||	IS_IN(hs, 0x2638, 0x263A) || IS_IN(hs, 0x2648, 0x2653) || IS_IN(hs, 0x2665, 0x2666) || IS_IN(hs, 0x2692, 0x2694)
                ||	IS_IN(hs, 0x2696, 0x2697) || IS_IN(hs, 0x269B, 0x269C) || IS_IN(hs, 0x26A0, 0x26A1) || IS_IN(hs, 0x26AA, 0x26AB)
                ||	IS_IN(hs, 0x26B0, 0x26B1) || IS_IN(hs, 0x26BD, 0x26BE) || IS_IN(hs, 0x26C4, 0x26C5) || IS_IN(hs, 0x26CE, 0x26CF)
                ||	IS_IN(hs, 0x26D3, 0x26D4) || IS_IN(hs, 0x26D3, 0x26D4) || IS_IN(hs, 0x26E9, 0x26EA) || IS_IN(hs, 0x26F0, 0x26F5)
                ||	IS_IN(hs, 0x26F7, 0x26FA)
                ||	hs == 0x260E || hs == 0x2611 || hs == 0x2618 || hs == 0x261D || hs == 0x2620 || hs == 0x2626 || hs == 0x262A
                ||	hs == 0x2660 || hs == 0x2663 || hs == 0x2668 || hs == 0x267B || hs == 0x267F || hs == 0x2699 || hs == 0x26C8
                ||	hs == 0x26D1 || hs == 0x26FD
                // Dingbats
                ||	IS_IN(hs, 0x2708, 0x270D) || IS_IN(hs, 0x2733, 0x2734) || IS_IN(hs, 0x2753, 0x2755)
                ||	IS_IN(hs, 0x2763, 0x2764) || IS_IN(hs, 0x2795, 0x2797)
                ||	hs == 0x2702 || hs == 0x2705 || hs == 0x270F || hs == 0x2712 || hs == 0x2714 || hs == 0x2716 || hs == 0x271D
                ||	hs == 0x2721 || hs == 0x2728 || hs == 0x2744 || hs == 0x2747 || hs == 0x274C || hs == 0x274E || hs == 0x2757
                ||	hs == 0x27A1 || hs == 0x27B0 || hs == 0x27BF
                // CJK Symbols and Punctuation
                ||	hs == 0x3030 || hs == 0x303D
                // Enclosed CJK Letters and Months
                ||	hs == 0x3297 || hs == 0x3299
                // Supplemental Arrows-B
                ||	IS_IN(hs, 0x2934, 0x2935)
                // Miscellaneous Symbols and Arrows
                ||	IS_IN(hs, 0x2B05, 0x2B07) || IS_IN(hs, 0x2B1B, 0x2B1C) || hs == 0x2B50 || hs == 0x2B55
                )
             {
                 returnValue = YES;
             }
         }
         
#undef IS_IN
     }];
    
    return returnValue;
}
#endif // FILTER_EMOJIS_IOS_KEYBOARD

