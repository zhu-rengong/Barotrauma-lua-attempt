using System;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;

namespace Barotrauma
{
    internal enum LuaCsMessageOrigin
    {
        LuaCs,
        Unknown,
        LuaMod,
        CSharpMod,
    }

    internal delegate void LuaCsMessageLogger(string prefix, object o);
    internal delegate void LuaCsExceptionHandler(Exception ex, LuaCsMessageOrigin origin);

    // TODO: make it a separate class
    partial class LuaCsSetup
    {

#if SERVER
        private const string LOG_PREFIX = "SV";
#else
        private const string LOG_PREFIX = "CL";
#endif
        public static void PrintLuaError(object o) => PrintErrorBase($"[{LOG_PREFIX} LUA ERROR] ", o, "nil");
        public static void PrintCsError(object o) => PrintErrorBase($"[{LOG_PREFIX} CS ERROR] ", o, "Null");
        public static void PrintGenericError(object o) => PrintErrorBase($"[{LOG_PREFIX} ERROR] ", o, "Null");


        private static void PrintErrorBase(string prefix, object message, string empty)
        {
            message ??= empty;
            var str = message.ToString();

            for (int i = 0; i < str.Length; i += 1024)
            {
                var subStr = str.Substring(i, Math.Min(1024, str.Length - i));

                var errorMsg = subStr;
                if (i == 0) errorMsg = prefix + errorMsg;

                DebugConsole.ThrowError(errorMsg);

#if SERVER
                if (GameMain.Server != null)
                {
                    foreach (var c in GameMain.Server.ConnectedClients)
                    {
                        GameMain.Server.SendDirectChatMessage(ChatMessage.Create("", errorMsg, ChatMessageType.Console, null, textColor: Color.Red), c);
                    }

                    GameServer.Log(errorMsg, ServerLog.MessageType.Error);
                }
#endif
            }
        }

        // TODO: deprecate this (in an effort to get rid of as much global state as possible)
        public void PrintError(object o, LuaCsMessageOrigin origin)
        {
            switch (origin)
            {
                case LuaCsMessageOrigin.LuaCs:
                    PrintGenericError(o);
                    break;
                case LuaCsMessageOrigin.LuaMod:
                    PrintLuaError(o);
                    break;
                case LuaCsMessageOrigin.CSharpMod:
                    PrintCsError(o);
                    break;
            }
        }

        private void DefaultExceptionHandler(Exception ex, LuaCsMessageOrigin origin)
        {
            switch (ex)
            {
                case NetRuntimeException netRuntimeException:
                    if (netRuntimeException.DecoratedMessage == null)
                    {
                        PrintError(netRuntimeException, origin);
                    }
                    else
                    {
                        // FIXME: netRuntimeException.ToString() doesn't print the InnerException's stack trace...
                        PrintError($"{netRuntimeException.DecoratedMessage}: {netRuntimeException}", origin);
                    }
                    break;
                case InterpreterException interpreterException:
                    if (interpreterException.DecoratedMessage == null)
                    {
                        PrintError(interpreterException, origin);
                    }
                    else
                    {
                        PrintError(interpreterException.DecoratedMessage, origin);
                    }
                    break;
                default:
                    var msg = ex.StackTrace != null
                        ? ex.ToString()
                        : $"{ex}\n{Environment.StackTrace}";
                    PrintError(msg, origin);
                    break;
            }
        }

        private static void DefaultMessageLogger(string prefix, object o)
        {
            var message = o.ToString();
            for (int i = 0; i < message.Length; i += 1024)
            {
                var subStr = message.Substring(i, Math.Min(1024, message.Length - i));

#if SERVER
                if (GameMain.Server != null)
                {
                    foreach (var c in GameMain.Server.ConnectedClients)
                    {
                        GameMain.Server.SendDirectChatMessage(ChatMessage.Create("", subStr, ChatMessageType.Console, null, textColor: Color.MediumPurple), c);
                    }

#if !DEBUG
                    GameServer.Log(prefix + subStr, ServerLog.MessageType.ServerMessage);
#endif
                }
#endif
            }

#if SERVER
            DebugConsole.NewMessage(message.ToString(), Color.MediumPurple);
#else
            DebugConsole.NewMessage(message.ToString(), Color.Purple);
#endif
        }

        private void PrintMessageBase(string prefix, object message, string empty) => MessageLogger?.Invoke(prefix, message ?? empty);
        internal void PrintMessage(object message) => PrintMessageBase("[LuaCs] ", message, "nil");

        // TODO: deprecate this (in an effort to get rid of as much global state as possible)
        public static void PrintCsMessage(object message) => GameMain.LuaCs.PrintMessage(message);

    }
}
