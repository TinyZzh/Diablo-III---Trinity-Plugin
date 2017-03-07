using System;
using System.Threading.Tasks;
using QuestTools.ProfileTags;
using QuestTools.ProfileTags.Movement;
using Zeta.Bot;
using Zeta.Bot.Profile.Common;
using Zeta.Game;
using Zeta.Game.Internals;

namespace QuestTools
{
    public class GameUI
    {
        private const ulong mercenaryOKHash = 1591817666218338490;
        private const ulong conversationSkipHash = 0x942F41B6B5346714;
        private const ulong talkToInteractButton1Hash = 0x8EB3A93FB1E49EB8;
        private const ulong confirmTimedDungeonOKHash = 0xF9E7B8A635A4F725;
        private const ulong genericOKHash = 0x891D21408238D18E;
        private const ulong partyLeaderBossAcceptHash = 0x69B3F61C0F8490B0;
        private const ulong partyFollowerBossAcceptHash = 0xF495983BA9BE450F;

        //private static UIElement _confirmTimedDungeonOK;
        //public static UIElement ConfirmTimedDungeonOK { get { try { return _confirmTimedDungeonOK ?? (_confirmTimedDungeonOK = UIElement.FromHash(confirmTimedDungeonOKHash)); } catch { return null; } } }
        public static UIElement ConfirmTimedDungeonOK
        {
            get
            {
                try { return UIElement.FromHash(confirmTimedDungeonOKHash); }
                catch { return null; }
            }
        }

        //private static UIElement _mercenaryOKButton;
        //public static UIElement MercenaryOKButton { get { try { return _mercenaryOKButton ?? (_mercenaryOKButton = UIElement.FromHash(mercenaryOKHash)); } catch { return null; } } }
        public static UIElement MercenaryOKButton
        {
            get
            {
                try { return UIElement.FromHash(mercenaryOKHash); }
                catch { return null; }
            }
        }

        //private static UIElement _conversationSkipButton;
        //public static UIElement ConversationSkipButton { get { try { return _conversationSkipButton ?? (_conversationSkipButton = UIElement.FromHash(conversationSkipHash)); } catch { return null; } } }
        public static UIElement ConversationSkipButton
        {
            get
            {
                try { return UIElement.FromHash(conversationSkipHash); }
                catch { return null; }
            }
        }

        //private static UIElement _partyLeaderBossAccept;
        //public static UIElement PartyLeaderBossAccept { get { try { return _partyLeaderBossAccept ?? (_partyLeaderBossAccept = UIElement.FromHash(partyLeaderBossAcceptHash)); } catch { return null; } } }
        public static UIElement PartyLeaderBossAccept
        {
            get
            {
                try { return UIElement.FromHash(partyLeaderBossAcceptHash); }
                catch { return null; }
            }
        }

        //private static UIElement _partyFollowerBossAccept;
        //public static UIElement PartyFollowerBossAccept { get { try { return _partyFollowerBossAccept ?? (_partyFollowerBossAccept = UIElement.FromHash(partyFollowerBossAcceptHash)); } catch { return null; } } }
        public static UIElement PartyFollowerBossAccept
        {
            get
            {
                try { return UIElement.FromHash(0xF495983BA9BE450F); }
                catch { return null; }
            }
        }

        //private static UIElement _genericOK;
        //public static UIElement GenericOK { get { try { return _genericOK ?? (_genericOK = UIElement.FromHash(genericOKHash)); } catch { return null; } } }
        public static UIElement GenericOK
        {
            get
            {
                try { return UIElement.FromHash(0x891D21408238D18E); }
                catch { return null; }
            }
        }

        //private static UIElement _talktoInteractButton1;
        //public static UIElement TalktoInteractButton1 { get { try { return _talktoInteractButton1 ?? (_talktoInteractButton1 = UIElement.FromHash(talkToInteractButton1Hash)); } catch { return null; } } }
        public static UIElement TalktoInteractButton1
        {
            get
            {
                try { return UIElement.FromHash(talkToInteractButton1Hash); }
                catch { return null; }
            }
        }

        public static UIElement StashCloseButton
        {
            get
            {
                try { return UIElement.FromHash(0x5CF7230E045FF4DF); }
                catch { return null; }
            }
        }

        // Urshi Continue button
        // [22A5DBD0] Mouseover: 0x1A089FAFF3CB6576, Name: Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.rewardChoicePane.Container.Continue
        public static UIElement UrshiContinueButton
        {
            get
            {
                try { return UIElement.FromHash(0x1A089FAFF3CB6576); }
                catch { return null; }
            }
        }

        // Urshi Keystone button
        // [22A60AB0] Mouseover: 0x4BDE2D63B5C36134, Name: Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.rewardChoicePane.Container.advance_button
        public static UIElement UrshiKeystoneButton
        {
            get
            {
                try { return UIElement.FromHash(0x4BDE2D63B5C36134); }
                catch { return null; }
            }
        }

        // Urshi Gem button 
        // [22A5F900] Mouseover: 0x826E5716E8D4DD05, Name: Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.rewardChoicePane.Container.upgrade_button1
        public static UIElement UrshiGemButton
        {
            get
            {
                try { return UIElement.FromHash(0x826E5716E8D4DD05); }
                catch { return null; }
            }
        }

        public static bool IsElementVisible(UIElement element)
        {
            if (element == null)
                return false;
            if (!element.IsValid)
                return false;
            if (!element.IsVisible)
                return false;

            return true;
        }

        public static void SafeClickElement(UIElement element, string name = "", bool fireWorldTransfer = false)
        {
            if (ZetaDia.Me.IsValid)
            {
                if (IsElementVisible(element))
                {
                    if (fireWorldTransfer)
                        GameEvents.FireWorldTransferStart();

                    Logger.Log("Clicking UI element {0} ({1})", name, element.BaseAddress);
                    element.Click();
                }
            }
        }

        private static DateTime lastCheckedUIButtons = DateTime.MinValue;
        public static void SafeClickUIButtons()
        {
            SafeClickElement(ConversationSkipButton, "Conversation Button");
            SafeClickElement(PartyLeaderBossAccept, "Party Leader Boss Accept", true);
            SafeClickElement(PartyFollowerBossAccept, "Party Follower Boss Accept", true);

            if (DateTime.UtcNow.Subtract(lastCheckedUIButtons).TotalMilliseconds <= 500)
                return;

            lastCheckedUIButtons = DateTime.UtcNow;

            if (ZetaDia.Me.LoopingAnimationEndTime <= 0)
            {
                SafeClickElement(MercenaryOKButton, "Mercenary OK Button");
                SafeClickElement(GenericOK, "GenericOK");
                SafeClickElement(UIElements.ConfirmationDialogOkButton, "ConfirmationDialogOKButton", true);
                SafeClickElement(ConfirmTimedDungeonOK, "Confirm Timed Dungeon OK Button", true);
            }
        }

        /// <summary>
        /// The current profile behavior is an 'Interact' type (UseObject, MoveToActor, etc)
        /// </summary>
        public static bool CurrentProfileTagIsInteract
        {
            get
            {
                bool result = false;

                if (ProfileManager.CurrentProfileBehavior != null)
                {
                    Type behaviorType = ProfileManager.CurrentProfileBehavior.GetType();
                    if (behaviorType == typeof(UseObjectTag) || behaviorType == typeof(UsePortalTag) || behaviorType == typeof(MoveToActorTag) || behaviorType == typeof(MoveToMapMarkerTag))
                    {
                        result = true;
                    }
                }

                return result;
            }
        }

        public static bool IsPartyDialogVisible
        {
            get
            {
                return IsElementVisible(PartyFollowerBossAccept) || IsElementVisible(PartyLeaderBossAccept);
            }
        }

        public static void CloseVendorWindow()
        {
            UIElement.FromHash(0x109597E125942DA4).Click();
        }


    }
}
