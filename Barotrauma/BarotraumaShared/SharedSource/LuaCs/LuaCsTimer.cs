using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Barotrauma
{
    public class LuaCsTimer
    {
        public static long LastUpdateTime = 0;

    	public static double Time => Timing.TotalTime;
    	
    	public static float GetUsageMemory()
    	{
    		Process proc = Process.GetCurrentProcess();
    		float memory = MathF.Round(proc.PrivateMemorySize64 / (1024 * 1024), 2);
    		proc.Dispose();

    		return memory;
    	}

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
    	
    	private static List<TimedAction> timedActions = new List<TimedAction>();

    	private static void AddTimer(TimedAction timedAction)
    	{
    		int insertionPoint = timedActions.BinarySearch(timedAction, new TimerComparer());
    		
    		if (insertionPoint < 0)
    		{
    			insertionPoint = ~insertionPoint;
    		}
    		
    		timedActions.Insert(insertionPoint, timedAction);
    	}

    	public static void Update()
    	{
    		while (timedActions.Count > 0)
    		{
    			TimedAction timedAction = timedActions[0];
    			if (Time >= timedAction.executionTime)
    			{
    				timedAction.action();
    				timedActions.RemoveAt(0);
    			}
    			else
    			{
    				break;
    			}
    		}
    	}

    	public static void Clear()
    	{
    		timedActions = new List<TimedAction>();
    	}

    	public static void Wait(LuaCsAction action, int millisecondDelay)
    	{
    		TimedAction timedAction = new TimedAction(action, millisecondDelay);
    		AddTimer(timedAction);
    	}
    }
}