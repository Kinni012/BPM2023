using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.ProcessTree
{
    public class ProcessTree : ICloneable
    {
        /// <summary>
        /// Reference to the parent of the current node
        /// Necessary to get root of the tree 
        /// </summary>
        public ProcessTree Parent { get; set; }

        /// <summary>
        /// List of all children of the current node
        /// </summary>
        public List<ProcessTree> Children { get; set; } = new List<ProcessTree>();

        /// <summary>
        /// Label of the current node in the process tree
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Unique id of the tree
        /// </summary>
        public Guid Guid { get; private set; }


        public Random.MersenneTwister Twister { get; set; }

        /// <summary>
        /// Initialize empty process tree
        /// </summary>
        public ProcessTree(string label = "")
        {
            Parent = null;
            Label = label;
            Guid = Guid.NewGuid();
            Twister = new Random.MersenneTwister();
        }

        /// <summary>
        /// Check if two process trees are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is ProcessTree tree)
            {
                string s1 = this.ToString();
                string s2 = tree.ToString();
                return s1 == s2;
            }
            return false;
        }

        /// <summary>
        /// Get hash value of a process tree
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hashCode = Label.GetHashCode();
            foreach (ProcessTree tree in Children)
            {
                hashCode += tree.GetHashCode();
            }

            return hashCode;
        }

        /// <summary>
        /// String representation of the process tree, can be used for e.g. console output 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Operators.Instance.OperatorList.Keys.Contains(Label))
            {
                sb.Append(Operators.Instance.OperatorList[Label] + "(");
                foreach (ProcessTree child in Children)
                    sb.Append(child.ToString());

                sb.Append(")");
            }
            else
                sb.Append(Label);

            return sb.ToString();
        }

        /// <summary>
        /// Create a deep clone of the process tree
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            ProcessTree result = new ProcessTree(Label);
            result.Guid = Guid;
            List<ProcessTree> childList = new List<ProcessTree>();
            foreach (ProcessTree tree in Children)
                childList.Add((ProcessTree)tree.Clone());
            foreach (ProcessTree tree in childList)
                tree.Parent = result;

            result.Children = childList;
            return result;
        }

        /// <summary>
        /// Get the root of the process tree through recursion
        /// </summary>
        /// <returns></returns>
        public ProcessTree GetRoot()
        {
            ProcessTree current = this;
            while (current.Parent != null)
                current = current.Parent;
            return current;
        }

        /// <summary>
        /// Create a ptml string of the process tree
        /// </summary>
        /// <returns></returns>
        public string ToPtmlFormat()
        {

            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);
            XmlElement ptmlElement = doc.CreateElement("ptml");
            XmlElement processTree = doc.CreateElement("processTree");
            processTree.SetAttribute("name", Guid.NewGuid().ToString());
            processTree.SetAttribute("root", Guid.ToString());

            doc.InsertAfter(ptmlElement, xmlDeclaration);
            ptmlElement.AppendChild(processTree);

            List<XmlElement> elements = GetElementsAndSubElements(doc);

            foreach (XmlElement element in elements)
            {
                processTree.AppendChild(element);
            }


            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            writer.Formatting = Formatting.Indented;
            doc.WriteContentTo(writer);
            writer.Flush();
            mStream.Flush();
            mStream.Position = 0;

            // Read MemoryStream contents into a StreamReader.
            StreamReader sReader = new StreamReader(mStream);

            // Extract the text from the StreamReader.
            string formattedXml = sReader.ReadToEnd();

            return formattedXml;
        }

        public Dictionary<Guid, string> GetNodes()
        {
            Dictionary<Guid, string> result = new Dictionary<Guid, string>();
            if (Operators.Instance.OperatorList.Keys.Contains(Label))
            {
                result.Add(Guid, Operators.Instance.OperatorList[Label].ToString());
            }
            else if (Label != "")
            {
                result.Add(Guid, Label);
            }

            foreach (ProcessTree child in Children)
            {
                Dictionary<Guid, string> chilDictionary = child.GetNodes();
                foreach (KeyValuePair<Guid, string> pair in chilDictionary)
                {
                    result.Add(pair.Key, pair.Value);
                }
            }
            return result;
        }

        public Dictionary<Guid, List<Guid>> GetConnections()
        {
            Dictionary<Guid, List<Guid>> result = new Dictionary<Guid, List<Guid>>();

            foreach (ProcessTree child in Children)
            {
                if (!result.ContainsKey(Guid))
                    result.Add(Guid, new List<Guid>());
                result[Guid].Add(child.Guid);

                Dictionary<Guid, List<Guid>> childDictionary = child.GetConnections();
                foreach (KeyValuePair<Guid, List<Guid>> pair in childDictionary)
                {
                    foreach (Guid childGuid in pair.Value)
                    {
                        if (!result.ContainsKey(pair.Key))
                            result.Add(pair.Key, new List<Guid>());
                        result[pair.Key].Add(childGuid);
                    }
                }
            }

            return result;
        }

        public List<ProcessTree> GetLeaves()
        {
            List<ProcessTree> result = new List<ProcessTree>();

            if (Children.Count == 0)
            {
                result.Add(this);
            }
            else
            {
                foreach (ProcessTree tree in Children)
                {
                    List<ProcessTree> results = tree.GetLeaves();
                    result.AddRange(results);
                }
            }
            return result;
        }
        public ProcessTree GetLeaveWithName(string name)
        {
            List<ProcessTree> result = new List<ProcessTree>();

            if (Children.Count == 0)
            {
                if (this.Label == name)
                    result.Add(this);
            }
            else
            {
                foreach (ProcessTree tree in Children)
                {
                    ProcessTree r = tree.GetLeaveWithName(name);
                    if (r != null)
                        result.Add(r);
                }
            }
            return (ProcessTree)(result.FirstOrDefault());
        }

        public ProcessTree GetRandomLeave()
        {
            List<ProcessTree> leaves = GetLeaves();
            List<ProcessTree> dic = new List<ProcessTree>();
            foreach (ProcessTree leaf in leaves)
            {
                if (leaf.Parent != null)
                {

                    if (leaf.Parent.Label == "Parallel")
                    {
                        //Symetrical operators described in the paper
                        //Only use rightmost child
                        //TODO
                    }
                    if (leaf.Parent.Label == "Xor")
                    {
                        //Symetrical operators described in the paper
                        //Only use rightmost child
                        //TODO
                    }
                }
                dic.Add(leaf);
            }

            return dic.OrderBy(x => Twister.Next()).FirstOrDefault();
        }

        public void ReplaceRandomChildWithOperator(string operatorName, string secondChild)
        {
            ProcessTree leaf = GetRandomLeave();
            string leafName = leaf.Label;


            ProcessTree child1 = new ProcessTree();
            child1.Label = leafName;
            ProcessTree child2 = new ProcessTree();
            child2.Label = secondChild;


            leaf.Children.Add(child1);
            leaf.Children.Add(child2);

            leaf.Label = operatorName;

        }

        public void ReplaceSpecificChildWithOperator(string child, string operatorName, string secondChild)
        {
            ProcessTree leaf = GetLeaveWithName(child);
            string leafName = leaf.Label;


            ProcessTree child1 = new ProcessTree();
            child1.Label = leafName;
            ProcessTree child2 = new ProcessTree();
            child2.Label = secondChild;


            leaf.Children.Add(child1);
            leaf.Children.Add(child2);

            leaf.Label = operatorName;

        }

        public List<XmlElement> GetElementsAndSubElements(XmlDocument doc)
        {
            List<XmlElement> result = new List<XmlElement>();

            //==================== Operators ===============================
            if (Operators.Instance.OperatorList.Keys.Contains(Label))
            {
                XmlElement operatorElement = doc.CreateElement(Operators.Instance.OperatorList[Label]);
                operatorElement.SetAttribute("name", "");
                operatorElement.SetAttribute("id", Guid.ToString());

                result.Add(operatorElement);
            }
            else //==================== Tasks ===============================
            if (Label != "")
            {
                XmlElement taskElement = doc.CreateElement("manualTask");
                taskElement.SetAttribute("name", Label);
                taskElement.SetAttribute("id", Guid.ToString());
                result.Add(taskElement);
            }

            //==================== Connections ===============================

            foreach (ProcessTree child in Children)
            {
                XmlElement parentsNode = doc.CreateElement("parentsNode");
                parentsNode.SetAttribute("id", Guid.NewGuid().ToString());
                parentsNode.SetAttribute("sourceId", Guid.ToString());
                parentsNode.SetAttribute("targetId", child.Guid.ToString());
                result.Add(parentsNode);
            }

            foreach (ProcessTree child in Children)
            {
                result.AddRange(child.GetElementsAndSubElements(doc));
            }

            return result;
        }

        public string RandomNodeNotInTree(List<string> activityKeys)
        {
            List<string> labelsInTree = GetLeaves().Select(x => x.Label).ToList();
            List<string> result = new List<string>();
            foreach (string activityKey in activityKeys)
            {
                if (!labelsInTree.Contains(activityKey))
                    result.Add(activityKey);
            }

            return result.OrderBy(x => Twister.NextDouble()).FirstOrDefault();
        }

        /// <summary>
        /// Pass a list of activity keys => check which keys are not in the tree yet
        /// </summary>
        /// <param name="activityKeys"></param>
        /// <returns></returns>
        public List<string> ActivityKeysNotInTree(List<string> activityKeys)
        {
            List<string> labelsInTree = GetLeaves().Select(x => x.Label).ToList();
            List<string> result = new List<string>();
            foreach (string activityKey in activityKeys)
            {
                if (!labelsInTree.Contains(activityKey))
                    result.Add(activityKey);
            }
            return result;
        }

        public List<string> GetRepresentations()
        {
            List<string> result = new List<string>();
            string s = Label;
            if (Children.Count > 2)
            {
                Console.WriteLine("Error in Processtree.GetRepresentations");
            }


            if (s == "Xor")
            {
                foreach (string representation1 in Children[0].GetRepresentations())
                {
                    foreach (string representation2 in Children[1].GetRepresentations())
                    {
                        result.Add(Operators.Instance.OperatorList[Label] + "(" + representation1 + " " +
                                   representation2 + ")");
                        result.Add(Operators.Instance.OperatorList[Label] + "(" + representation2 + " " +
                                   representation1 + ")");
                    }
                }
            }
            else if (s == "Sequence")
            {
                foreach (string representation1 in Children[0].GetRepresentations())
                {
                    foreach (string representation2 in Children[1].GetRepresentations())
                    {
                        result.Add(Operators.Instance.OperatorList[Label] + "(" + representation1 + " " +
                                   representation2 + ")");
                    }
                }
            }
            else
            {
                result.Add(Label);
            }

            return result;
        }
    }
}
