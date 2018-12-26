//
//  FIZZKeyboard.h
//  Unity-iPhone
//
//  Created by Hassan Ali on 4/6/17.
//
//

#ifndef FIZZKeyboard_h
#define FIZZKeyboard_h

#import "PHFComposeBarView.h"
#pragma once

extern "C" typedef void (*SendMessageCallback)(const char *message);
extern "C" typedef void (*StickerCallback)();

typedef struct
{
    const char* text;
    const char* placeholder;
    
    UIKeyboardType				keyboardType;
    UITextAutocorrectionType	autocorrectionType;
    UIKeyboardAppearance		appearance;
    
    BOOL multiline;
    BOOL secure;
}
KeyboardShowParam;


@interface FIZZKeyboardDelegate : NSObject <UITextFieldDelegate, UITextViewDelegate, PHFComposeBarViewDelegate>
{
}
- (BOOL)textFieldShouldReturn:(UITextField*)textField;
- (void)textInputDone:(id)sender;
- (void)textInputCancel:(id)sender;
- (void)keyboardDidShow:(NSNotification*)notification;
- (void)keyboardWillHide:(NSNotification*)notification;

// on older devices initial keyboard creation might be slow, so it is good to init in on initial loading.
// on the other hand, if you dont use keyboard (or use it rarely), you can avoid having all related stuff in memory:
//     keyboard will be created on demand anyway (in Instance method)
+ (void)Initialize;
+ (FIZZKeyboardDelegate*)Instance;

- (id)init;
- (void)setKeyboardParams:(KeyboardShowParam)param;
- (void)show;
- (void)hide;
- (void)positionInput:(CGRect)keyboardRect x:(float)x y:(float)y;
- (void)shouldHideInput:(BOOL)hide;

+ (void)StartReorientation;
+ (void)FinishReorientation;

- (CGRect)queryArea;
- (NSString*)getText;
- (void)setText:(NSString*)newText;

@property (readonly, nonatomic, getter=queryArea)				CGRect		area;
@property (readonly, nonatomic)									BOOL		active;
@property (readonly, nonatomic)									BOOL		done;
@property (readonly, nonatomic)									BOOL		canceled;
@property (retain, nonatomic, getter=getText, setter=setText:)	NSString*	text;

@property SendMessageCallback sendMessageCallback;
@property StickerCallback stickerCallback;

@end


#endif /* FIZZKeyboard_h */
