using System;
using System.Collections.Generic;
using System.Text;
using MoonSharp.Interpreter;
using Microsoft.Xna.Framework;
using Barotrauma.Networking;
using System.Threading.Tasks;
using Barotrauma.Items.Components;
using System.IO;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;

namespace Barotrauma
{

	public static class LuaDocs
	{

		public static string ConvertTypeName(string type)
		{
			switch (type)
			{
				case "Boolean":
					return "bool";
				case "String":
					return "string";
				case "int":
					return "number";
				case "Single":
					return "number";
				case "Double":
					return "number";
				case "float":
					return "number";
				case "UInt16":
					return "number";
				case "UInt32":
					return "number";
				case "UInt64":
					return "number";
				case "Int32":
					return "number";
				case "List`1":
					return "table";
				case "Dictionary`2":
					return "table";
			}

			if (type.StartsWith("Action"))
				return "function";

			if (type.StartsWith("Func"))
				return "function";

			if (type.StartsWith("IEnumerable"))
				return "Enumerable";

			return type;
		}

		public static string EscapeName(string n)
		{
			if (n == "end")
				return "endparam";

			return n;
		}

		public static void GenerateDocsAll()
		{
			GenerateDocs(typeof(Character), "Character.lua");
			GenerateDocs(typeof(CharacterInfo), "CharacterInfo.lua");
			GenerateDocs(typeof(CharacterHealth), "CharacterHealth.lua");
			GenerateDocs(typeof(AnimController), "AnimController.lua");
			GenerateDocs(typeof(Client), "Client.lua");
			GenerateDocs(typeof(Entity), "Entity.lua");
			GenerateDocs(typeof(EntitySpawner), "Entity.Spawner.lua", "Entity.Spawner");
			GenerateDocs(typeof(Item), "Item.lua");
			GenerateDocs(typeof(ItemPrefab), "ItemPrefab.lua");
			GenerateDocs(typeof(Submarine), "Submarine.lua");
			GenerateDocs(typeof(SubmarineInfo), "SubmarineInfo.lua");
			GenerateDocs(typeof(Job), "Job.lua");
			GenerateDocs(typeof(JobPrefab), "JobPrefab.lua");
			GenerateDocs(typeof(GameSession), "GameSession.lua", "Game.GameSession");
			GenerateDocs(typeof(NetLobbyScreen), "NetLobbyScreen.lua", "Game.NetLobbyScreen");
			GenerateDocs(typeof(GameScreen), "GameScreen.lua", "Game.GameScreen");
			GenerateDocs(typeof(FarseerPhysics.Dynamics.World), "World.lua", "Game.World");
			GenerateDocs(typeof(Inventory), "Inventory.lua", "Inventory");
			GenerateDocs(typeof(ItemInventory), "ItemInventory.lua", "ItemInventory");
			GenerateDocs(typeof(CharacterInventory), "CharacterInventory.lua", "CharacterInventory");
			GenerateDocs(typeof(Hull), "Hull.lua", "Hull");
			GenerateDocs(typeof(Level), "Level.lua", "Level");
			GenerateDocs(typeof(Affliction), "Affliction.lua", "Affliction");
			GenerateDocs(typeof(AfflictionPrefab), "AfflictionPrefab.lua", "AfflictionPrefab");
			GenerateDocs(typeof(WayPoint), "WayPoint.lua", "WayPoint");
			GenerateDocs(typeof(ServerSettings), "ServerSettings.lua", "Game.ServerSettings");
		}

		public static void GenerateDocs(Type type, string name, string categoryName = null)
		{
			GenerateDocs(type, "../../../../docs/baseluadocs/" + name, "../../../../docs/lua/generated/" + name, categoryName);
		}

		public static void GenerateDocs(Type type, string baselua, string fileresult, string categoryName = null)
		{
			var sb = new StringBuilder();

			if (categoryName == null)
				categoryName = type.Name;

			var baseluatext = "";

			if (!File.Exists(baselua))
			{
				const string EMPTY_TABLE = "{}";

				baseluatext = @$"-- luacheck: ignore 111

--[[--
{type.FullName}
]]
-- @code {categoryName}
-- @pragma nostrip
local {type.Name} = {EMPTY_TABLE}";

				File.WriteAllText(baselua, baseluatext);
			}
			else
				baseluatext = File.ReadAllText(baselua);

			HashSet<string> removed = new HashSet<string>();
			
			foreach(var line in baseluatext.Split('\n'))
			{
				if(line.Contains("-- @remove "))
				{
					var replaced = line.Replace("-- @remove ", "").Replace("\r", "");
					removed.Add(replaced);
				}
			}
			
			sb.Append(baseluatext + "\n\n");

			var members = type.GetMembers();

			foreach(var member in members)
			{
				Console.WriteLine("'{0}' is a {1}", member.Name, member.MemberType);

				if (member.MemberType == MemberTypes.Method)
				{
					var method = (MethodInfo)member;

					if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
						continue;

					var lsb = new StringBuilder();

					lsb.Append($"--- {method.Name}\n");
					lsb.Append($"-- @realm shared\n");

					var paramNames = "";

					var parameters = method.GetParameters();
					for(var i=0; i < parameters.Length; i++)
					{
						var parameter = parameters[i];

						if(i == parameters.Length - 1)
							paramNames = paramNames + EscapeName(parameter.Name);
						else
							paramNames = paramNames + EscapeName(parameter.Name) + ", ";

						lsb.Append($"-- @tparam {ConvertTypeName(parameter.ParameterType.Name)} {EscapeName(parameter.Name)}\n");
					}

					if (method.ReturnType != typeof(void))
					{
						lsb.Append($"-- @treturn {ConvertTypeName(method.ReturnType.Name)}\n");
					}

					string functionDecoration;

					if (method.IsStatic)
						functionDecoration = $"function {type.Name}.{method.Name}({paramNames}) end";
					else
						functionDecoration = $"function {method.Name}({paramNames}) end";

					if (removed.Contains(functionDecoration))
					{
						continue;
					}

					lsb.Append(functionDecoration);

					lsb.Append("\n\n");
					sb.Append(lsb);
				}

				if (member.MemberType == MemberTypes.Field)
				{
					var lsb = new StringBuilder();

					var field = (FieldInfo)member;

					lsb.Append($"---\n");
					lsb.Append($"-- ");

					var name = EscapeName(field.Name);

					var returnName = ConvertTypeName(field.FieldType.Name);

					if (field.IsStatic)
						name = type.Name + "." + field.Name;

					if (removed.Contains(name))
						continue;

					lsb.Append(name);
					lsb.Append($", Field of type {returnName}\n");
					lsb.Append($"-- @realm shared\n");
					lsb.Append($"-- @{returnName} {name}\n");

					lsb.Append("\n");
					sb.Append(lsb);
				}

				if (member.MemberType == MemberTypes.Property)
				{
					var lsb = new StringBuilder();

					var property = (PropertyInfo)member;

					lsb.Append($"---\n");
					lsb.Append($"-- ");

					var name = EscapeName(property.Name);

					var returnName = ConvertTypeName(property.PropertyType.Name);

					if (property.GetGetMethod().IsStatic)
						name = type.Name + "." + property.Name;

					if (removed.Contains(name))
						continue;

					lsb.Append(name);
					lsb.Append($", Field of type {returnName}\n");
					lsb.Append($"-- @realm shared\n");
					lsb.Append($"-- @{returnName} {name}\n");

					lsb.Append("\n");
					sb.Append(lsb);
				}
			}

			File.WriteAllText(fileresult, sb.ToString());
		}
	}
}