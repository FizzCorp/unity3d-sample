﻿//
//  LocalizationService.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using System.Collections.Generic;
using Fizz.Common.Json;
using UnityEngine;

namespace Fizz.UI.Core {
    public class LocalizationService : IServiceLocalization {
        private string m_path = "FizzConfigration/";
        private string m_language = string.Empty;
        private Dictionary<string, string> m_resources = null;

        public LocalizationService () {
            InitLangugageResources ();
        }

        private void InitLangugageResources () {
            m_language = Utils.GetSystemLanguage ();
            TextAsset textAssets = Resources.Load (m_path + m_language) as TextAsset;
            if (textAssets != null && !string.IsNullOrEmpty (textAssets.text)) {
                JSONClass jsonClass = Utils.GetJsonClass (textAssets.text);
                m_resources = new Dictionary<string, string> ();

                if (jsonClass != null && jsonClass.Count != 0) {
                    foreach (KeyValuePair<string, JSONNode> entry in jsonClass) {
                        m_resources.Add (entry.Key, entry.Value);
                    }
                } else {
                    DefaultLangugaeResources ();
                }
            } else {
                DefaultLangugaeResources ();
            }
        }

        public override string GetText (string id) {
            if (m_resources.ContainsKey (id)) {
                return m_resources[id];
            }
            return id;
        }

        public override string Language {
            get {
                return m_language;
            }
            set {
                m_language = value;
            }
        }

        public override string this [string id] {
            get {
                return GetText (id);
            }
        }

        private void DefaultLangugaeResources () {
            /* fixformat ignore:start */
			m_resources = new Dictionary<string,string> () {
				{ "Contact_Remove","Remove" },
				{ "Contact_Unblock","Unblock" },
				{ "Friends_RequestReceived","has sent you a friend request" },
				{ "Friends_TitleAddFriends","Add Contacts" },
				{ "Friends_TitleAll","All" },
				{ "Friends_TitleFavourites","Favorites" },
				{ "Friends_TitleFollowers","Followers" },
				{ "Friends_TitleFollowing","Following" },
				{ "Friends_TitleFriendRequests","Requests" },
				{ "Friends_TitleFriends","Friends" },
				{ "Friends_TitleRequestReceived","Friend Requests Received" },
				{ "Friends_TitleRequestSent","Friend Requests Sent" },
				{ "General_BlockedByUser","You have been blocked by this user!" },
				{ "General_BlockedThisUser","You have blocked this user!" },
				{ "General_Cancel","Cancel" },
				{ "General_Create","Create" },
				{ "General_Done","Done" },
				{ "General_Error","Error" },
				{ "General_PrivilegedActionError","Only privileged users can perform this action." },
				{ "General_InfoUpdateFailure","Information failed to update" },
				{ "General_InfoUpdateSuccess","Information updated successfully" },
				{ "General_InternetUnavailable","Internet not Available!" },
				{ "General_InvalidName","Name is not valid" },
				{ "General_LengthRangeMsg","Length should be {0} to {1}" },
				{ "General_LimitRangeMsg","Limit should be {0} to {1}" },
				{ "General_Loading","Loading..." },
				{ "General_No","No" },
				{ "General_Note","Note" },
				{ "General_Ok","Ok" },
				{ "General_Password","Password" },
				{ "General_SearchPlaceholder","Search…" },
				{ "General_SelectAll","Select All" },
				{ "General_ServerErrorMessage","Something went wrong on server" },
				{ "General_UnSelectAll","Unselect All" },
				{ "General_Yes","Yes" },
				{ "Group_ActionClosed","Closed" },
				{ "Group_ActionDelete","Delete" },
				{ "Group_ActionJoin","Join" },
				{ "Group_ActionLeave","Leave" },
				{ "Group_ActionRequest","Request" },
				{ "Group_AddToGroup","Add to group" },
				{ "Group_ChooseType","Choose group type" },
				{ "Group_Deleted","Group has been deleted" },
				{ "Group_DeleteGroup","Delete Group" },
				{ "Group_DeleteQuestion","Are you sure you want to delete this group?" },
				{ "Group_DescJoinGroup","Search and join groups" },
				{ "Group_DescJoinTransientGroup","Search and join transient rooms" },
				{ "Group_EnablePassword","Enable Password" },
				{ "Group_EnterName","Enter group name" },
				{ "Group_EnterLimit","Enter limit" },
				{ "Group_InvitationReceivedGeneral","You have been invited to this group." },
				{ "Group_InvitationReceivedPrivateChat","has requested to have a private chat with you." },
				{ "Group_InvitationSent","has been invited to this group." },
				{ "Group_InvitationsReceived","Invitations Received" },
				{ "Group_InvitationsSent","Invitations Sent" },
				{ "Group_JoinFailure","Unable to join group!" },
				{ "Group_JoinRequestReceived","has requested to join this group." },
				{ "Group_JoinRequests","Join Requests" },
				{ "Group_JoinRequestSent","Join request sent" },
				{ "Group_LeaveGroup","Leave Group" },
				{ "Group_LeaveQuestion","Are you sure you want to leave this group?" },
				{ "Group_MaxMemberLimit","Max member limit" },
				{ "Group_ModRejectMsg","The admin has prohibited you from joining this group." },
				{ "Group_MutedMsg","You have been muted in this group" },
				{ "Group_NoName","No Name" },
				{ "Group_NotMemberError","You can't send messages to this group because you're no longer a participant." },
				{ "Group_PermissionError","You can't send messages to this group because you're not permitted." },
				{ "Group_RequestedToJoin","You requested to join this group." },
				{ "Group_TitleAddFriends","Add Friends" },
				{ "Group_TitleAll","All" },
				{ "Group_TitleCreate","Create Group" },
				{ "Group_TitleJoinGroup","Join Group" },
				{ "Group_TitleJoinTransientGroup", "Join Transient Room"},
				{ "Group_TitleMembers","Members" },
				{ "Group_TitleOwned","Owned" },
				{ "Group_TitlePassword","Group Password" },
				{ "Group_TitleRequests","Requests" },
				{ "Group_TitleSettings","Group Settings" },
				{ "Group_TypePrivateClosed","Private Closed" },
				{ "Group_TypePrivateInvite","Private Invite only" },
				{ "Group_TypePublic","Public" },
				{ "Group_TypeSecret","Secret" },
				{ "Group_YouGotKicked","You have been kicked" },
				{ "Group_YouLeft","You Left" },
				{ "MemberAction_Kick","Kick" },
				{ "MemberAction_MakeListener","Make Listener" },
				{ "MemberAction_MakeMember","Make Member" },
				{ "MemberAction_MakeModerator","Make Moderator" },
				{ "MemberAction_MakeSuperModerator","Make Super Moderator" },
				{ "MemberAction_Mute","Mute" },
				{ "MemberAction_Unmute","Unmute" },
				{ "Message_PlaceHolderTypeMsg","Write a message..." },
				{ "Message_TitleAll","All" },
				{ "Message_TitleGroup","Group" },
				{ "Message_TitleNewMessage","New Message" },
				{ "Message_TitlePrivate","Private" },
				{ "TransientRoom_Title", "Transient Rooms" },
				{ "TransientRoom_DialogTitle", "Transient Room"},
				{ "TransientRoom_DialogMessage", "Enter name"},
				{ "Presence_Away","Away" },
				{ "Presence_Busy","Busy" },
				{ "Presence_Offline","Offline" },
				{ "Presence_Online","Online" },
				{ "Privacy_Everyone","Everyone" },
				{ "Privacy_Friends","Friends" },
				{ "Privacy_None","None" },
				{ "Request_ApproveIgnoreMsg","Approve or ignore requests" },
				{ "Request_CancelFriendReqQuestion","Are you sure you want to cancel sent friend request?" },
				{ "Request_FriendRejectedMsg","This user has prohibited you from sending friend request." },
				{ "Request_FriendRequests","Friend Request" },
				{ "Role_Admin","Admin" },
				{ "Role_Owner","Owner" },
				{ "Role_Pending","Pending" },
				{ "Settings_AddToBlockList","Add to block list" },
				{ "Settings_AutoTranslation","Auto Translation" },
				{ "Settings_BlockList","Block List" },
				{ "Settings_ConfirmPassword","Confirm Password" },
				{ "Settings_EnterPassword","Enter Password" },
				{ "Settings_NickPlaceholder","Enter your Nick" },
				{ "Settings_NickUpdateFailure","Unable to update Nick!" },
				{ "Settings_PasswordsMismatch","Passwords don't match" },
				{ "Settings_Privacy","Privacy" },
				{ "Settings_Register","Register" },
				{ "Settings_SignIn","Sign In" },
				{ "Settings_Status","Status" },
				{ "Settings_TitleBlockUsers","Block Users" },
				{ "Settings_TitleSettings","Settings" },
				{ "Settings_WrongPassword","Wrong Password" },
				{ "TabItem_Friends","Friends" },
				{ "TabItem_GameChat","Game Chat" },
				{ "TabItem_Groups","Groups" },
				{ "TabItem_Messages","Messages" },
				{ "TabItem_Notifications","Notifications" },
				{ "Translation_Translate","Translate" },
				{ "Translation_Translating","Translating" },
				{ "Translation_TranslationFailed","Translation Failed!" },
				{ "General_SocketConnecting","Connecting..." },
				{ "General_SocketConnected", "Connected" },
				{ "General_InternalIDInput","Enter Valid Internal User Id" },
				{ "Settings_EnterNick","Enter nick..." },
				{ "Settings_EnterMood","Enter mood..." },
				{ "Search_Title","Search User" },
				{ "Profile_UnFriendMessage","Are you sure you want to unfriend?" },
				{ "Profile_UnFriendSuccess","UnFriend User Success" },
				{ "Profile_UnFriendFailure","UnFriend User Failure" },
				{ "Profile_FriendRequestPending","Friend request is pending" },
				{ "Profile_FriendRequestSuccess","Friend Request Success" },
				{ "Profile_FriendRequestFailure","Friend Request Failure" },
				{ "Profile_UnblockSuccess","User has been unblocked successfully" },
				{ "Profile_UnblockFailure","Unblock User Failure" },
				{ "Profile_BlockUserMessage","Are you sure you want to block?" }, 
				{ "Profile_BlockFriendMessage", "Are you sure you want to block? \n(User will be removed from your friend list too)"},
				{ "Profile_BlockSuccess","Block User Success" },
				{ "Profile_BlockFailure","Block User Failure" },
				{ "Profile_UnFavouriteFriendSuccess","User is removed from your Favorite list" },
				{ "Profile_UnFavouriteFriendFailure","Remove Favourite User Failure" },
				{ "Profile_FavouriteFriendSuccess","User is added to your Favorite list" },
				{ "Profile_FavouriteFriendFailure","Add Favourite User Failure" },
				{ "Group_LeaveFailure","Leave Error" },
				{ "Group_DeleteFailture","Delete Error" },
				{ "Requests_AcceptFriendRequestSuccess","Accept Friend Request Success" },
				{ "Requests_AcceptFriendRequestFailure","Accept Friend Request Failure" },
				{ "Requests_AcceptRoomInviteSuccess","Group joined successfully" },
				{ "Requests_AcceptRoomInviteFailure","Unable to process invite accept request" },
				{ "Requests_AcceptRoomRequestSuccess","Accept Room Request Success" },
				{ "Requests_AcceptRoomRequestFailure","Unable to process accept room request" },
				{ "Requests_RejectFriendRequestSuccess","Reject Friend Request Success" },
				{ "Requests_RejectFriendRequestFailture","Reject Friend Request Failure" },
				{ "Requests_RejectRoomInviteSuccess","Group join invite rejected" },
				{ "Requests_RejectRoomInviteFailure","Unable to process invite reject request" },
				{ "Requests_RejectRoomRequestSuccess","Reject Room Request Success" },
				{ "Requests_RejectRoomRequestFailure","Unable to process reject room request" },
				{ "Requests_CancelFriendRequestSuccess","Cancel Friend Request Success" },
				{ "Requests_CancelFriendRequestFailure","Cancel Friend Request Failure" },
				{ "Requests_CancelJoinRoomRequestSuccess","Cancel Join Room Request Success" },
				{ "Requests_CancelJoinRoomRequestFailure","Cancel Join Room Request Failure" },
				{ "Requests_CancelRoomInviteSuccess","Cancel Room Invite Success" },
				{ "Requests_CancelRoomInviteFailure","Cancel Room Invite Failure" },
				{ "Requests_RejectedFriendRequestMessage","{0} has rejected friend request" },
				{ "Requests_RequestWithdrawnMessage","Request for {0} withdrawn" },
				{ "Requests_RequestRejectedByOwnerMessage","Request to join {0} rejected by owner" },
				{ "Request_JoinedMessage","Joined {0}!" },
				{ "Settings_NickLimit","Nick should be greater than {0}" },
				{ "Settings_UnblockSuccess","Unblock User Success" },
				{ "Settings_UnblockFailture","Unblock User Failure" },
				{ "Settings_BlockSuccess","User has been blocked successfully" },
				{ "Settings_BlockFailure","Block User Failure" },
				{ "ChatNotification_AdminRejectedRequest","Admin rejected join request of {0}" },
				{ "ChatNotification_AdminRejectedMyRequest","Admin rejected your join request" },
				{ "ChatNotification_AppKickedUser","Admin kicked {0}" },
				{ "ChatNotification_AppKickedMe","Admin kicked you" },
				{ "ChatNotification_AppRemovedUser","Admin removed {0}" },
				{ "ChatNotification_AppRemovedMe","Admin removed you" },
				{ "ChatNotification_CancelRoomInviteByUser","{0} canceled invitation" },
				{ "ChatNotification_CancelRoomInviteByMe","You canceled invitation" },
				{ "ChatNotification_JoinRequestByUser","{0} requested to join" },
				{ "ChatNotification_JoinRequestByMe","You requested to join" },
				{ "ChatNotification_JoinRoomByUser","{0} joined" },
				{ "ChatNotification_JoinRoomByMe","You joined" },
				{ "ChatNotification_AdminKickedUser","Admin kicked {0}" },
				{ "ChatNotification_AdminKickedMe","Admin kicked you" },
				{ "ChatNotification_LeaveRoomByUser","{0} left" },
				{ "ChatNotification_LeaveRoomByMe","You left" },
				{ "Chat_PermissionError","You can't send messages to this chat because you're not permitted." },
				{ "Message_TitleFriends","Friends" },
				{ "Message_TitleOthers","Others" },
				{ "Settings_MoodLimit","Mood length should be less than {0}." },
				{ "Friends_TitleRequestSentFailed","Friend Requests Sent Failed" },
				{ "Friends_CanNotAddAnonymousUser","Unregistered User cannot be added as Friend" },
				{ "Profile_CanNotAddAnonymousUser","Unregistered User cannot be added as Friend" },
				{ "Group_ActionJoining","Joining" },
				{ "Group_ActionRequesting","Requesting" },
				{ "Group_ActionJoined","Joined" },
				{ "Group_ActionRequested","Requested" },
				{ "Group_MaxMemberLimitReached","Max member limit reached" },
				{ "Group_NameLimit", "Name should be greater than {0}"},
				{ "Settings_BlockUserMessage","Are you sure you want to block?" },
				{ "Settings_BlockFriendMessage", "Are you sure you want to block? \n(User will be removed from your friend list too)" },
				{ "Search_NoResultsFound","No Results Found" },
				{ "CreateRoom_AddMember","Add Member" },
				{ "GroupSettings_Requests","Requests" },
				{ "GroupSettings_UserLimit","User Limit" },
				{ "Friends_TitleRequestSentSuccessfully","Friend request sent successfully" },
				{ "Role_Member","Member" },
				{ "Title_BlockedUsers","Blocked Users" },
				{ "PasswordView_ChangeSuccessfully","Password has been changed successfully" },
				{ "Group_GroupJoined","Group joined successfully" },
				{ "Group_GroupCreatedSuccessfully","Group created successfully" },
				{ "Group_PasswordInvalid","Password is Incorrect" },
				{ "Search_TitleAddToGroup", "Add to Group" },
				{ "Search_TitleAddToBlockList", "Add to Block List" },
				//Error
				{ "CreatePrivateChatRoom_RoomTypeDisabled", "CreatePrivateChatRoom_RoomTypeDisabled"},
				{ "CreatePrivateChatRoom_CreateGroupRoomLimitExceeded", "CreatePrivateChatRoom_CreateGroupRoomLimitExceeded"},
				{ "CreatePrivateChatRoom_UserIsBlocked", "You have blocked this user"},
				{ "CreatePrivateChatRoom_SelfChatNotAllowed", "Self chat not allowed" },
				{ "CreatePrivateChatRoom_UserIsBlocker", "User has blocked you"},
				{ "SendFriendRequest_Success", "Friend request sent "},
				{ "SendFriendRequest_RelationshipAlreadyExists", "User has already sent you a friend request"},
				{ "SendFriendRequest_CanNotAddAnonymousUser", "Cannot add an unregistered user"},
				{ "SendFriendRequest_RelationshipAlreadyRejected", "Cannot send request. User rejected your previous request."},
				{ "SendFriendRequest_MaxRelationshipLimitReached", "Maximum limit reached."},
				{ "SendFriendRequest_UserIsBlocker", "Unable to send friend request" },
				{ "SendFriendRequest_UserIsBlocked", "User is blocked"},
				{ "UpdateProfileMood_Success", "Mood updated" },
				{ "UpdateProfileMood_Failure", "Failed to process. Please try again later." },
				{ "UpdateProfileNick_Success", "Nick updated" },
				{ "UpdateProfileNick_Failure", "Failed to process. Please try again later." },
				{ "UpdateProfileAvatar_Success", "Avatar updated" },
				{ "UpdateProfileAvatar_Failure", "Failed to process. Please try again later." },
				{ "UpdateAvatr_ErrorCodeRoomAvatarInvalid", "Avatar no longer exists." },
				{ "AcceptFriendRequest_Success", "Request accepted" },
				{"AcceptFriendRequest_Failure", "Failed to process. Please try again later."},
				{"AcceptJoinRoomInvite_Success", "Invite accepted"},
				{"AcceptPendingJoinRoomRequest_Success", "Request accepted"},
				{"AcceptPendingJoinRoomRequest_Failure", "Failed to process. Please try again later."},
				{"RejectJoinRoomInvite_Success", "Invite rejected"},
				{"RejectJoinRoomInvite_Failure", "Failed to process. Please try again later."},
				{"RejectFriendRequest_Success", "Request rejected"},
				{"RejectFriendRequest_Failure", "Failed to process. Please try again later."},
				{"RejectPendingJoinRoomRequest_Success", "Request rejected"},
				{"RejectPendingJoinRoomRequest_Failure", "Failed to process. Please try again later."},
				{"CancelFriendRequest_Success", "Request revoked"},
				{"CancelFriendRequest_Failure", "Failed to process. Please try again later."},
				{"CancelJoinRoomRequest_Success", "Request revoked"},
				{"CancelJoinRoomRequest_Failure", "Failed to process. Please try again later."},
				{"CancelRoomInviteRequest_Success", "Invite revoked"},
				{"CancelRoomInviteRequest_Failure", "Failed to process. Please try again later."},
				{"RelationShipChange_RejectedByOthers","Request Rejected by {0}"},
				{"JoinRequest_AdminCanceledJoinRequest", "Invite revoked by Admin"},
				{"JoinRequest_JoinRequestRejected", "Request rejected by Admin"},
				{"JoinRequest_StatusJoined", "Request accepted by Admin"},
				{"UpdateRoomPrivacySettings_Failure", "Failed to process. Please try again later."},
				{"UpdateRoomPrivacySettings_Success", "Group privacy settings updated"},
				{"UpdateRoomName_Failure", "Failed to process. Please try again later."},
				{"UpdateRoomName_Success", "Group name updated"},
				{"UpdateRoomUserLimit_Success", "Group user limit updated"},
				{"UpdateRoomUserLimit_Failure", "Failed to process. Please try again later."},
				{"LeaveRoom_Success", "Group left successfully"},
				{"LeaveTransientRoom_Success", "Transient room left successfully"},
				{"LeaveRoom_Failure", "Failed to process. Please try again later."},
				{"DeleteRoom_Success", "Group deleted successfully"},
				{"DeleteRoom_Failure", "Failed to process. Please try again later."},
				{"UpdateRoomAvatar_Success", "Group avatar updated"},
				{"UpdateRoomAvatar_Failure", "Failed to process. Please try again later."},
				{"UpdateRoomPassword_Success", "Group password updated"},
				{"UpdateRoomPassword_Failure", "Failed to process. Please try again later."},
				{"ChatRoomCreation_CreateGroupRoomLimitExceeded", "Max. group creation limit reached."},
				{"ChatRoomCreation_Success", "Group created successfully"},
				{"ChatRoomCreation_RoomTypeDisabled", "ChatRoomCreation_RoomTypeDisabled"},
				{"ChatRoomCreation_AnonymousUser", "ChatRoomCreation_AnonymousUser"},
				{"BlockUserRequest_Success", "Blocked successfully"},
				{"BlockUserRequest_Failure", "Failed to process. Please try again later."},
				{"BlockUserRequest_MaxRelationshipLimitReached", "Maximum block user limit reached."},
				{"UnBlockUserRequest_Success", "Unblocked successfully"},
				{"UnBlockUserRequest_Failure", "Failed to process. Please try again later."},
				{"JoinRoomRequest_Success", "Room joined successfully"},
				{"JoinTransientRoom_Success", "Transient Room joined successfully"},
				{"JoinRoomRequest_RoomNotFound", "Cannot join. Group does not exist anymore."},
				{"JoinRoomRequest_RoomMaxUserLimit", "Group is full"},
				{"JoinRoomReques_RequestSentSuccess", "Request sent"},
				{"JoinRoomRequest_RoomJoinNotAllowed", "JoinRoomRequest_RoomJoinNotAllowed"},
				{"JoinRoomRequest_RoomJoinInvalidPassword", "Invalid password."},
				{"UnFriendUser_Success", "Removed from friend list"},
				{"UnFriendUser_Failure", "Failed to process. Please try again later."},
				{"RemoveFavouriteFriend_Failure", "Failed to process. Please try again later."},
				{"RemoveFavouriteFriend_Success", "Removed from Favorite"},
				{"AddFavouriteFriend_Success", "Added to Favorite"},
				{"AddFavouriteFriend_AlreadyFavourite", "AddFavouriteFriend_AlreadyFavourite"},
				{"AddFavouriteFriend_Failure", "Failed to process. Please try again later."},
				{"BlockUserRequest_AlreadyBlocked", "BlockUserRequest_AlreadyBlocked"},
				{"SendFriendRequest_AlreadyFriends", "SendFriendRequest_AlreadyFriends"},
				{"KickUser_Success", "User has been kicked"},
				{"KickUser_CannotLeaveOrKickFromGlobalRoom", "KickUser_CannotLeaveOrKickFromGlobalRoom"},
				{"KickUser_CannotLeaveOrKickFromOneXOneRoom", "KickUser_CannotLeaveOrKickFromOneXOneRoom"},
				{"KickUser_PermissionDenied", "KickUser_PermissionDenied"},
				{"SendRoomInvite_Success", "Invite sent successfully"},
				{"SendRoomInvite_UserIsBlocker", "Unable to send invite."},
				{"SendRoomInvite_UserHasRequestedToJoinRoom", "User has requested to join room"},
				{"SendRoomInvite_UserRejectedInvitationBefore", "Cannot send invite. User rejected your previous invitation."},
				{"SendFriendRequest_AlreadySentFriendRequest", "Already sent friend request"},
				{"JoinRoomRequest_KickedFromRoom", "You got kicked"},
				{"JoinRoomRequest_RoomJoinRequestDeclined", "Cannot send request. Admin has rejected your previous join request."},
				{"GroupMembers_Title", "Group Members"},
				{"SearchFriend_AddFriend", "Add Friend"},
				{"SearchFriend_Adding", "Adding"},
				{"SearchFriend_Unfriend", "Unfriend"},
				{"SearchFriend_Removing", "Removing"},
				{"SearchFriend_Accept", "Accept"},
				{"SearchFriend_Reject", "Reject"},
				{"SearchFriend_Cancel", "Cancel"},
				{"SearchFriend_Accepting", "Accepting"},
				{"SearchFriend_Rejecting", "Rejecting"},
				{"SearchFriend_Canceling", "Canceling"},
				{"FIZZErrorCode_ErrorCodeInvalidAppid", "App id is not valid"},
				{"FIZZErrorCode_ErrorCodeAppNotFound", "App not found"},
				{"FIZZErrorCode_ErrorCodeInvalidAppCredentials", "Invalid app cradentials"},
				{"FIZZErrorCode_ErrorCodeInternalIdNotProvided", "Internal id not provided"},
				{"FIZZErrorCode_ErrorCodeGameUserNotFound", "Game user not found"},
				{"FIZZErrorCode_ErrorCodeFizzUserNotFound", "Fizz user not found"},
				{"FIZZErrorCode_ErrorCodeFizzIdDuplicates", "Fizz id duplicates"},
				{"FIZZErrorCode_ErrorCodeGameUserIdDuplicates", "Game user id duplicates"},
				{"FIZZErrorCode_ErrorCodeUserIsBanned", "User is banned"},
				{"FIZZErrorCode_ErrorCodeDecodingToken", "Invalid token"},
				{"FIZZErrorCode_ErrorCodeInvalidToken", "Invalid token"},
				{"FIZZErrorCode_ErrorCodeTokenExpired", "Token expired"},
				{"FIZZErrorCode_ErrorCodeTokenMissing", "Token missing"},
				{"FIZZErrorCode_ErrorCodeUnauthorizedAccessToken", "Token unautherized access"},
				{"FIZZErrorCode_ErrorCodeDbError", "Database error"},
				{"FIZZErrorCode_ErrorCodeSocketNoUserBound", "No user is bound to socket"},
				{"FIZZErrorCode_ErrorCodeMissingParameter", "Api parameters are missing"},
				{"FIZZErrorCode_ErrorCodeInvalidParameterFormat", "Invalid parameter format"},
				{"FIZZErrorCode_ErrorCodeInvalidResponse", "Invalid response"},
				{"FIZZErrorCode_ErrorCodeDiscontinuedApiVersion", "Discontinued api version"},
				{"FIZZErrorCode_ErrorCodeServerMaintenance", "Service Unavailable"},
				{"FIZZErrorCode_ErrorCodeNoAck", "Unable to process. Please try again!"},
				{"FIZZErrorCode_ErrorCodeUnKnown", "Unknown error"},
				{"FIZZErrorCode_ErrorConnectionFailure", "FIZZ Connection Failed"},
				{"FIZZErrorCode_ErrorRoomAlreadyJoined", "Room already joined"},
				{"FIZZErrorCode_ErrorTransientRoomJoinLimitReached", "Max limit reached for transient room"},
				{"FIZZStateReason_Banned", "You have been banned"}
			};
			/* fixformat ignore:end */
        }
    }
}