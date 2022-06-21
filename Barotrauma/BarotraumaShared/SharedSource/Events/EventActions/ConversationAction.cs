using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Barotrauma
{
    partial class ConversationAction : EventAction
    {

        public enum DialogTypes
        {
            Regular,
            Small,
            Mission
        }
        
        const float InterruptDistance = 300.0f;

        /// <summary>
        /// Other events can't trigger conversations if some other event has triggered one within this time.
        /// Intended to prevent multiple events from triggering conversations at the same time.
        /// </summary>
        const float BlockOtherConversationsDuration = 5.0f;

        [Serialize("", IsPropertySaveable.Yes)]
        public string Text { get; set; }

        [Serialize(0, IsPropertySaveable.Yes)]
        public int DefaultOption { get; set; }

        [Serialize("", IsPropertySaveable.Yes)]
        public Identifier SpeakerTag { get; set; }

        [Serialize("", IsPropertySaveable.Yes)]
        public Identifier TargetTag { get; set; }

        [Serialize(true, IsPropertySaveable.Yes)]
        public bool WaitForInteraction { get; set; }

        [Serialize("", IsPropertySaveable.Yes, "Tag to assign to whoever invokes the conversation")]
        public Identifier InvokerTag { get; set; }

        [Serialize(false, IsPropertySaveable.Yes)]
        public bool FadeToBlack { get; set; }

        [Serialize(true, IsPropertySaveable.Yes, "Should the event end if the conversations is interrupted (e.g. if the speaker dies or falls unconscious mid-conversation). Defaults to true.")]
        public bool EndEventIfInterrupted { get; set; }

        [Serialize("", IsPropertySaveable.Yes)]
        public string EventSprite { get; set; }
        
        [Serialize(DialogTypes.Regular, IsPropertySaveable.Yes)]
        public DialogTypes DialogType { get; set; }

        [Serialize(false, IsPropertySaveable.Yes)]
        public bool ContinueConversation { get; set; }

        private Character speaker;

        private AIObjective prevIdleObjective, prevGotoObjective;

        public List<SubactionGroup> Options { get; private set; }

        public SubactionGroup Interrupted { get; private set; }

        private static UInt16 actionCount;

        //an identifier the server uses to identify which ConversationAction a client is responding to
        public readonly UInt16 Identifier;

        private int selectedOption = -1;
        private bool dialogOpened = false;

        private double lastActiveTime;

        private bool interrupt;

        public ConversationAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
        {
            actionCount++;
            Identifier = actionCount;
            Options = new List<SubactionGroup>();
            foreach (var elem in element.Elements())
            {
                if (elem.Name.LocalName.Equals("option", StringComparison.InvariantCultureIgnoreCase))
                {
                    Options.Add(new SubactionGroup(ParentEvent, elem));
                }
                else if (elem.Name.LocalName.Equals("interrupt", StringComparison.InvariantCultureIgnoreCase))
                {
                    Interrupted = new SubactionGroup(ParentEvent, elem);
                }
            }
        }

        public override IEnumerable<EventAction> GetSubActions()
        {
            return Options.SelectMany(group => group.Actions);
        }

        public override bool IsFinished(ref string goTo)
        {
            if (interrupt)
            {
                if (dialogOpened)
                {
#if CLIENT
                    dialogBox?.Close();
                    GUIMessageBox.MessageBoxes.ForEachMod(mb => 
                    { 
                        if (mb.UserData as string == "ConversationAction")
                        {
                            (mb as GUIMessageBox)?.Close();
                        }
                    });
#else
                    foreach (Client c in GameMain.Server.ConnectedClients)
                    {
                        if (c.InGame && c.Character != null) { ServerWrite(speaker, c); }
                    }
#endif
                    ResetSpeaker();
                    dialogOpened = false;
                }

                if (Interrupted == null)
                {
                    if (EndEventIfInterrupted) { goTo = "_end"; }
                    return true;
                }
                else
                {
                    return Interrupted.IsFinished(ref goTo);
                }
            }

            if (selectedOption >= 0)
            {
                if (!Options.Any() || Options[selectedOption].IsFinished(ref goTo))
                {
                    ResetSpeaker();
                    return true;
                }
            }
            return false;
        }

        public override void Reset()
        {
            Options.ForEach(a => a.Reset());
            ResetSpeaker();
            selectedOption = -1;
            interrupt = false;
            dialogOpened = false;
            speaker = null;
        }

        public override bool SetGoToTarget(string goTo)
        {
            selectedOption = -1;
            for (int i = 0; i < Options.Count; i++)
            {
                if (Options[i].SetGoToTarget(goTo))
                {
                    selectedOption = i;
                    interrupt = false;
                    dialogOpened = true;
                    return true;
                }
            }
            return false;
        }

        private void ResetSpeaker()
        {
            if (speaker == null) { return; }
            speaker.CampaignInteractionType = CampaignMode.InteractionType.None;
            speaker.ActiveConversation = null;
            speaker.SetCustomInteract(null, null);
#if SERVER
            GameMain.NetworkMember.CreateEntityEvent(speaker, new Character.AssignCampaignInteractionEventData());
#endif
            var humanAI = speaker.AIController as HumanAIController;
            if (humanAI != null && !speaker.IsDead && !speaker.Removed)
            {
                humanAI.ClearForcedOrder();
                if (prevIdleObjective != null) { humanAI.ObjectiveManager.AddObjective(prevIdleObjective); }
                if (prevGotoObjective != null) { humanAI.ObjectiveManager.AddObjective(prevGotoObjective); }
                humanAI.ObjectiveManager.SortObjectives();
            }
        }

        private int[] GetEndingOptions()
        {
            List<int> endings = Options.Where(group => !group.Actions.Any() || group.EndConversation).Select(group => Options.IndexOf(group)).ToList();
            if (!ContinueConversation) { endings.Add(-1); }
            return endings.ToArray();
        }

        public override void Update(float deltaTime)
        {
            lastActiveTime = Timing.TotalTime;
            if (interrupt)
            {
                Interrupted?.Update(deltaTime);
            }
            else if (selectedOption < 0)
            {
                if (dialogOpened)
                {
#if CLIENT
                    if (GUIMessageBox.MessageBoxes.Any(mb => mb.UserData as string == "ConversationAction"))
                    {
                        Character.DisableControls = true;
                    }
                    else
                    {
                        Reset();
                    }
#endif
                    if (ShouldInterrupt())
                    {
                        ResetSpeaker();
                        interrupt = true;
                    }
                    return;
                }

                if (!SpeakerTag.IsEmpty)
                {
                    if (speaker != null && !speaker.Removed && speaker.CampaignInteractionType == CampaignMode.InteractionType.Talk && speaker.ActiveConversation?.ParentEvent != this.ParentEvent) { return; }
                    speaker = ParentEvent.GetTargets(SpeakerTag).FirstOrDefault(e => e is Character) as Character;
                    if (speaker == null || speaker.Removed)
                    {
                        return;
                    }
                    //some conversation already assigned to the speaker, wait for it to be removed
                    if (speaker.CampaignInteractionType == CampaignMode.InteractionType.Talk && speaker.ActiveConversation?.ParentEvent != this.ParentEvent)
                    {
                        return;
                    }
                    else if (!WaitForInteraction)
                    {
                        TryStartConversation(speaker);
                    }
                    else if (speaker.ActiveConversation != this)
                    {
                        speaker.CampaignInteractionType = CampaignMode.InteractionType.Talk;
                        speaker.ActiveConversation = this;
#if CLIENT
                        speaker.SetCustomInteract(
                            TryStartConversation, 
                            TextManager.GetWithVariable("CampaignInteraction.Talk", "[key]", GameSettings.CurrentConfig.KeyMap.KeyBindText(InputType.Use)));
#else
                        speaker.SetCustomInteract( 
                            TryStartConversation, 
                            TextManager.Get("CampaignInteraction.Talk"));
                        GameMain.NetworkMember.CreateEntityEvent(speaker, new Character.AssignCampaignInteractionEventData());   
#endif
                    }
                    return;
                }
                else
                {
                    TryStartConversation(null);
                }
            }
            else
            {
                if (ShouldInterrupt())
                {
                    ResetSpeaker();
                    interrupt = true;
                }
                else if (Options.Any())
                {
                    Options[selectedOption].Update(deltaTime);
                }
            }
        }

        private bool ShouldInterrupt()
        {
            IEnumerable<Entity> targets = Enumerable.Empty<Entity>();
            if (!TargetTag.IsEmpty)
            {
                targets = ParentEvent.GetTargets(TargetTag).Where(e => IsValidTarget(e));
                if (!targets.Any()) { return true; }
            }

            if (speaker != null)
            {
                if (!TargetTag.IsEmpty)
                {
                    if (targets.All(t => Vector2.DistanceSquared(t.WorldPosition, speaker.WorldPosition) > InterruptDistance * InterruptDistance)) { return true; }
                }
                if (speaker.AIController is HumanAIController humanAI && !humanAI.AllowCampaignInteraction())
                {
                    return true;                    
                }
                return speaker.Removed || speaker.IsDead || speaker.IsIncapacitated;
            }

            return false;
        }

        private bool IsValidTarget(Entity e)
        {
            bool isValid = e is Character character && !character.Removed && !character.IsDead && !character.IsIncapacitated &&
                (e == Character.Controlled || character.IsRemotePlayer);
#if SERVER
            if (!dialogOpened)
            {
                UpdateIgnoredClients();
                isValid &= !ignoredClients.Keys.Any(c => c.Character == e);
            }
#elif CLIENT
            bool block = GUI.InputBlockingMenuOpen && !dialogOpened;
            isValid &= (e != Character.Controlled || !block);
#endif
            return isValid;
        }

        private void TryStartConversation(Character speaker, Character targetCharacter = null)
        {
            IEnumerable<Entity> targets = Enumerable.Empty<Entity>();
            if (!TargetTag.IsEmpty)
            {
                targets = ParentEvent.GetTargets(TargetTag).Where(e => IsValidTarget(e));
                if (!targets.Any() || IsBlockedByAnotherConversation(targets)) { return; }
            }

            if (speaker?.AIController is HumanAIController humanAI)
            {
                prevIdleObjective = humanAI.ObjectiveManager.GetObjective<AIObjectiveIdle>();
                prevGotoObjective = humanAI.ObjectiveManager.GetObjective<AIObjectiveGoTo>();
                humanAI.SetForcedOrder(
                    new Order(OrderPrefab.Prefabs["wait"], Barotrauma.Identifier.Empty, null, orderGiver: null));
                if (targets.Any()) 
                {
                    Entity closestTarget = null;
                    float closestDist = float.MaxValue;
                    foreach (Entity entity in targets)
                    {
                        float dist = Vector2.DistanceSquared(entity.WorldPosition, speaker.WorldPosition);
                        if (dist < closestDist)
                        {
                            closestTarget = entity;
                            closestDist = dist;
                        }
                    }
                    if (closestTarget != null)
                    {
                        humanAI.FaceTarget(closestTarget);
                    }
                }
            }

            if (targetCharacter != null && !InvokerTag.IsEmpty)
            {
                ParentEvent.AddTarget(InvokerTag, targetCharacter);
            }

            ShowDialog(speaker, targetCharacter);

            dialogOpened = true;
            if (speaker != null)
            {
                speaker.CampaignInteractionType = CampaignMode.InteractionType.None;
                speaker.SetCustomInteract(null, null);
#if SERVER
                GameMain.NetworkMember.CreateEntityEvent(speaker, new Character.AssignCampaignInteractionEventData());
#endif
            }
        }

        partial void ShowDialog(Character speaker, Character targetCharacter);

        public override string ToDebugString()
        {
            if (!interrupt)
            {
                return $"{ToolBox.GetDebugSymbol(selectedOption > -1, selectedOption < 0 && dialogOpened)} {nameof(ConversationAction)} -> (Selected option: {selectedOption.ColorizeObject()})";
            }
            else
            {
                return $"{ToolBox.GetDebugSymbol(true, selectedOption < 0 && dialogOpened)} {nameof(ConversationAction)} -> (Interrupted)";
            }
        }
    }
}