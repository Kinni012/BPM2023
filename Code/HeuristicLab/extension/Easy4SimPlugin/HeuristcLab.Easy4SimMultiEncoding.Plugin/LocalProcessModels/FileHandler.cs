using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels
{
    public class FileHandler
    {
        public string FolderPath { get; set; }

        public List<string> FileList { get; set; } = new List<string>();
        public Dictionary<string, XmlDocument> XmlDocuments = new Dictionary<string, XmlDocument>();

        private Object _lock = new Object();

        public FileHandler(string folderPath)
        {
            FolderPath = folderPath;
            FileList = Directory.GetFiles(folderPath).ToList();

            XmlDocuments = new Dictionary<string, XmlDocument>();


            Parallel.ForEach(FileList, file =>
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(file);
                    lock (_lock)
                    {
                        XmlDocuments.Add(file, doc);
                    }
                }
                catch (Exception e)
                {
                    lock (_lock)
                    {
                        Console.WriteLine("Error in File: " + file);
                        Console.WriteLine(e);
                    }
                    throw;
                }
            });
        }


        public void ReReadFiles()
        {
            XmlDocuments = new Dictionary<string, XmlDocument>();

            Parallel.ForEach(FileList, file =>
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(file);
                lock (_lock)
                {
                    XmlDocuments.Add(file, doc);
                }
            });
        }






    }
}
