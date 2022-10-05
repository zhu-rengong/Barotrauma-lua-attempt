namespace Barotrauma
{
    class CheckOrderAction : BinaryOptionAction
    {
        [Serialize("", IsPropertySaveable.Yes)]
        public Identifier TargetTag { get; set; }

        [Serialize("", IsPropertySaveable.Yes)]
        public Identifier OrderIdentifier { get; set; }

        [Serialize("", IsPropertySaveable.Yes)]
        public Identifier OrderOption { get; set; }

        [Serialize("", IsPropertySaveable.Yes)]
        public Identifier OrderTargetTag { get; set; }

        public CheckOrderAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element) { }

        protected override bool? DetermineSuccess()
        {
            Character targetCharacter = null;
            if (!TargetTag.IsEmpty)
            {
                foreach (var t in ParentEvent.GetTargets(TargetTag))
                {
                    if (t is Character c)
                    {
                        targetCharacter = c;
                        break;
                    }
                }
            }
            if (targetCharacter == null)
            {
                DebugConsole.LogError($"CheckConditionalAction error: {GetEventName()} uses a CheckOrderAction but no valid target character was found for tag \"{TargetTag}\"! This will cause the check to automatically fail.");
                return false;
            }
            var currentOrderInfo = targetCharacter.GetCurrentOrderWithTopPriority();
            if (currentOrderInfo?.Identifier == OrderIdentifier)
            {
                if (!OrderTargetTag.IsEmpty)
                {
                    if (currentOrderInfo.TargetEntity is not Item targetItem || !targetItem.HasTag(OrderTargetTag)) { return false; }
                }
                return OrderOption.IsEmpty || currentOrderInfo?.Option == OrderOption;                
            }
            return false;
        }

        private string GetEventName()
        {
            return ParentEvent?.Prefab?.Identifier is { IsEmpty: false } identifier ? $"the event \"{identifier}\"" : "an unknown event";
        }
    }
}