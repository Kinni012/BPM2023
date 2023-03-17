using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using Svg;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.Visualization
{
    public static class TreeVisualizer
    {
        public static Bitmap CreateBitmapFromTree(ProcessTree.ProcessTree tree, int i)
        {

            StringBuilder sb = new StringBuilder();
            Dictionary<Guid, string> dictionary = tree.GetNodes();


            Dictionary<Guid, string> nodesById = new Dictionary<Guid, string>();

            sb.Append("digraph G {" + Environment.NewLine);
            sb.Append("node [shape=record];" + Environment.NewLine);
            int counter = 0;

            foreach (KeyValuePair<Guid, string> pair in dictionary)
            {
                sb.Append($"node{counter} [label = \"{pair.Value}\"];" + Environment.NewLine);
                nodesById.Add(pair.Key, $"node{counter}");
                counter++;
            }


            foreach (KeyValuePair<Guid, List<Guid>> pair in tree.GetConnections())
            {
                foreach (Guid guid in pair.Value)
                {
                    sb.Append($"{nodesById[pair.Key]} -> {nodesById[guid]}; " + Environment.NewLine);
                }
            }


            sb.Append("}");
            //get the full location of the assembly with DaoTests in it
            string fullPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            bool exists = Directory.Exists(fullPath + "\\graphviz");

            if (!exists)
                Directory.CreateDirectory(fullPath + "\\graphviz");
            fullPath += "\\graphviz";

            string inputFile = fullPath + $"\\input{i}.dot";


            File.WriteAllText(inputFile, sb.ToString());
            RunGraphviz(fullPath, i);


            //TODO, not working yet
            SvgDocument svgDoc = SvgDocument.Open<SvgDocument>(fullPath + $"\\out{i}.svg", null);


            Bitmap bitmap = svgDoc.Draw();

            counter++;
            return bitmap;

        }
        public static void RunGraphviz(string fullPath, int i)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.WorkingDirectory = fullPath;
            startInfo.FileName = "dot.exe";
            startInfo.Arguments = $"-Tsvg input{i}.dot -o out{i}.svg";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            process.Dispose();

            //Do some fun stuff here...
        }

    }
}
