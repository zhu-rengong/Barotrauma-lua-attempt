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

			return type;
		}

		public static void GenerateDocs(Type type)
		{
			var sb = new StringBuilder();

			var members = type.GetMembers();

			foreach(var member in members)
			{
				Console.WriteLine("'{0}' is a {1}", member.Name, member.MemberType);
				
				if (member.MemberType == MemberTypes.Method)
				{
					var method = (MethodInfo)member;

					if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
						continue;

					sb.Append($"--- {method.Name}\n");
					sb.Append($"-- @realm shared\n");

					var paramNames = "";

					var parameters = method.GetParameters();
					for(var i=0; i < parameters.Length; i++)
					{
						var parameter = parameters[i];

						if(i == parameters.Length - 1)
							paramNames = paramNames + parameter.Name;
						else
							paramNames = paramNames + parameter.Name + ", ";

						sb.Append($"-- @tparam {ConvertTypeName(parameter.ParameterType.Name)} {parameter.Name}\n");
					}

					if (method.ReturnType != typeof(void))
					{
						sb.Append($"-- @treturn {ConvertTypeName(method.ReturnType.Name)}\n");
					}

					if (method.IsStatic)
						sb.Append($"function {type.Name}.{method.Name}({paramNames})");
					else
						sb.Append($"function {method.Name}({paramNames})");
					sb.Append(" end\n");

					sb.Append("\n");
				}

				if (member.MemberType == MemberTypes.Field)
				{
					var field = (FieldInfo)member;

					sb.Append($"---\n");
					sb.Append($"-- ");

					var name = field.Name;

					var returnName = ConvertTypeName(field.FieldType.Name);

					if (field.IsStatic)
						name = type.Name + "." + field.Name;

					sb.Append(name);
					sb.Append($", Field of type {returnName}\n");
					sb.Append($"-- @realm shared\n");
					sb.Append($"-- @{returnName} {name}\n");

					sb.Append("\n");
				}

				if (member.MemberType == MemberTypes.Property)
				{
					var property = (PropertyInfo)member;

					sb.Append($"---\n");
					sb.Append($"-- ");

					var name = property.Name;

					var returnName = ConvertTypeName(property.PropertyType.Name);

					if (property.GetGetMethod().IsStatic)
						name = type.Name + "." + property.Name;

					sb.Append(name);
					sb.Append($", Field of type {returnName}\n");
					sb.Append($"-- @realm shared\n");
					sb.Append($"-- @{returnName} {name}\n");

					sb.Append("\n");
				}
			}

			File.WriteAllText("luadocs.lua", sb.ToString());
		}
	}
}