using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.PetriNet;
using Svg;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.Visualization
{
    public static class WorkflowNetVisualizer
    {
        public static Bitmap CreateBitmapFromPetriNet(PetriNet.PetriNet net, int i)
        {
            StringBuilder sb = new StringBuilder();


            sb.Append("digraph G {" + Environment.NewLine);
            sb.Append("  node [shape=record];" + Environment.NewLine);

            sb.Append("  Start [shape=circle];" + Environment.NewLine);
            sb.Append("  End [shape=circle];" + Environment.NewLine);

            foreach (Place place in net.NormalNodes)
            {
                string placeName = place.Name.Replace(" ", "");
                int index = placeName.IndexOf("Human");
                if (index != -1)
                    placeName = placeName.Substring(0, index);
                sb.Append($"  node_{placeName} [label=\"{placeName}\"];" + Environment.NewLine);
            }

            for (int j = 0; j < net.Transitions.Count; j++)
            {
                sb.Append($"  node_{j} [label=\"\", width=0.3, shape=circle];" + Environment.NewLine);
            }
            for (int j = 0; j < net.Transitions.Count; j++)
            {
                Transition t = net.Transitions[j];
                if (t.To.Count == 1 && t.From.Count ==1)
                {
                    string startNode = t.From.First().Name.Replace(" ", "");
                    int index = startNode.IndexOf("Human");
                    if (index != -1)
                        startNode = startNode.Substring(0, index);

                    if (startNode != "Start" && startNode != "End")
                        startNode = "node_" + startNode;
                    string endNode = t.To.First().Name.Replace(" ", "");
                    int index2 = endNode.IndexOf("Human");
                    if (index2 != -1)
                        endNode = endNode.Substring(0, index2);

                    if (endNode != "End" && endNode != "Start")
                        endNode = "node_" + endNode;

                    sb.Append("  " + startNode + " -> " + $"node_{j}" + ";" + Environment.NewLine);
                    sb.Append("  " + $"node_{j}" + " -> " + endNode + ";" + Environment.NewLine);
                }
                else
                {
                    //TODO
                    //Transitions with multiple start/end nodes
                }
            }
            sb.Append("}");

            //get the full location of the assembly with DaoTests in it
            string fullPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            bool exists = Directory.Exists(fullPath + "\\graphviz");

            if (!exists)
                Directory.CreateDirectory(fullPath + "\\graphviz");
            fullPath += "\\graphviz";

            string inputFile = fullPath + $"\\input_petri_net{i}.dot";

            File.WriteAllText(inputFile, sb.ToString());
            RunGraphviz(fullPath, i);

            //TODO, not working yet
            SvgDocument svgDoc = SvgDocument.Open<SvgDocument>(fullPath + $"\\output_petri_net{i}.svg", null);


            Bitmap bitmap = svgDoc.Draw();

            return bitmap;
        }
        public static void RunGraphviz(string fullPath, int i)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.WorkingDirectory = fullPath;
            startInfo.FileName = "dot.exe";
            startInfo.Arguments = $"-Tsvg input_petri_net{i}.dot -o output_petri_net{i}.svg";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            process.Dispose();

            //Do some fun stuff here...
        }
    }
}
