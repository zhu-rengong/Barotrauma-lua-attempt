using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Barotrauma
{
    public class LuaCsTimer
    {
        public static double Time => Timing.TotalTime;
        public static double GetTime() => Time;

        private class TimerComparer : IComparer<TimedAction>
        {
            public int Compare(TimedAction timedAction1, TimedAction timedAction2)
            {
                if (timedAction1 == null || timedAction2 == null)
                    return 0;
                return -Math.Sign(timedAction2.executionTime - timedAction1.executionTime);
            }
        }

        private class TimedAction
        {
            public LuaCsAction action
            {
                get;
                private set;
            }

            private int delayMs;

            public double executionTime
            {
                get;
                private set;
            }
            
            public TimedAction(LuaCsAction action, int delayMs)
            {
                this.action = action;
                this.delayMs = delayMs;
                executionTime = Time + (delayMs / 1000f);
            }
        }
        
        private List<TimedAction> timedActions = new List<TimedAction>();

        private void AddTimer(TimedAction timedAction)
        {
            int insertionPoint = timedActions.BinarySearch(timedAction, new TimerComparer());
            
            if (insertionPoint < 0)
            {
                insertionPoint = ~insertionPoint;
            }
            
            timedActions.Insert(insertionPoint, timedAction);
        }

        public void Update()
        {
            List<TimedAction> timedActionsToRemove = new List<TimedAction>();
            for (int i = 0; i < timedActions.Count; i++)
            {
                TimedAction timedAction = timedActions[0];
                if (Time >= timedAction.executionTime)
                {
                    timedAction.action();
                    timedActionsToRemove.Add(timedAction);
                }
            }
            
            foreach (TimedAction timedAction in timedActionsToRemove)
            {
                timedActions.Remove(timedAction);
            }
        }

        public void Clear()
        {
            timedActions = new List<TimedAction>();
        }

        public void Wait(LuaCsAction action, int millisecondDelay)
        {
            TimedAction timedAction = new TimedAction(action, millisecondDelay);
            AddTimer(timedAction);
        }
    }
}