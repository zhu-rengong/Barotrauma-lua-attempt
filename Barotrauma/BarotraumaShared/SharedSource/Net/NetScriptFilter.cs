using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

partial class NetScript
{
    public class NetScriptFilter
    {
        private const bool useWhitelist = false;

        private static string[] classesPermited = new string[] {};
        private static string[] classesProhibited = new string[] { };
        public static bool IsClassAllowed(string usingName)
        {
            if (useWhitelist && !classesPermited.Any(u => u.Equals(usingName))) return false;
            if (classesProhibited.Any(u => u.Equals(usingName))) return false;
            return true;
        }

        public static string FilterSyntaxTree(CSharpSyntaxTree tree)
        {
            if (tree == null) throw new ArgumentNullException("Syntax tree must not be null.");
            { // Disallow top-level statements
                var nodeCheck = tree.GetRoot().DescendantNodes();

                var tlStatements = nodeCheck.Where(n => n is GlobalStatementSyntax).ToList();
                if (tlStatements.Count > 0)
                {
                    string errStr = "Cmopilation Error:";
                    foreach (var tls in tlStatements) tls.GetDiagnostics().ToList().ForEach(d => errStr += "\n" + d.ToString());
                    return errStr;
                }
            }

            var compRoot = tree.GetCompilationUnitRoot();
            var refDirs = compRoot.GetReferenceDirectives().ToList();
            Console.WriteLine($"Reference Directives [{refDirs.Count}]:");
            refDirs.ForEach(d => Console.WriteLine(d.ToFullString()));

            List<string> allUsedTypes = new List<string>();
            { // Find all used types
            }

            List<string> allResolvedTypes = new List<string>();
            { // Resolve all types
            }

            { // Check all used types
            }

            if (!Directory.Exists("./SyntaxTrees")) Directory.CreateDirectory("./SyntaxTrees");
            string fileName = "./SyntaxTrees/" + tree.FilePath.Replace("/", "--") + ".txt";
            if (File.Exists(fileName)) File.Delete(fileName);
            var fileWriter = File.CreateText(fileName);

            var nodes = new Queue<(SyntaxNode, int)>();
            nodes.Enqueue((tree.GetRoot(), 0));
            while (nodes.Count > 0)
            {
                var nodeElem = nodes.Dequeue();
                var node = nodeElem.Item1;
                var indent = nodeElem.Item2;

                node.ChildNodes().ToList().ForEach(n => {
                    if (n.ChildNodes().Count() > 0) nodes.Enqueue((n, indent + 1));
                    if (!(
                        n is MemberAccessExpressionSyntax ||
                        n is UsingDirectiveSyntax ||
                        n is BaseTypeSyntax ||
                        n is TypeSyntax
                    )) return;
                    //Console.WriteLine(new String(' ', indent * 2) + n.GetType().Name + "  |  " + n.GetText()?.ToString() ?? "null");
                    fileWriter.WriteLine(new String(' ', indent * 2) + n.GetType().Name + "  |  " + n.GetText()?.ToString() ?? "null");
                });
                node.DescendantNodes().ToList().ForEach(n => {
                    if (n.DescendantNodes().Count() > 0 && !nodes.Contains((n, indent + 1))) nodes.Enqueue((n, indent + 1));
                    if (!(
                        n is MemberAccessExpressionSyntax ||
                        n is UsingDirectiveSyntax ||
                        n is BaseTypeSyntax ||
                        n is TypeSyntax
                    )) return;
                    //Console.WriteLine(new String(' ', indent * 2) + n.GetType().Name + "  |  " + n.GetText()?.ToString() ?? "null");
                    fileWriter.WriteLine(new String(' ', indent * 2) + n.GetType().Name + "  |  " + n.GetText()?.ToString() ?? "null");
                });
            }
            fileWriter.Close();
            return null; 
        }
    }
}