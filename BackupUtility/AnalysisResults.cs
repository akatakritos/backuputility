using System;
using System.Collections.Generic;
using System.Text;

namespace BackupUtility
{
    public class AnalysisResults
    {
        private string baseFromDir;
        private string baseToDir;
        //private Dictionary<string, string> mDictionary;
        private List<string> originals;
        private List<string> backups;

        private int currFile = 0;
        public int CurrentFileIndex
        {
            get { return currFile; }
            set { currFile = value; }
        }

        public AnalysisResults(string originalDir, string backupDir)
        {
            baseFromDir = originalDir;
            baseToDir = backupDir;
            originals = new List<string>();
            backups = new List<string>();
        }

        public void Add(string relativeFileName)
        {
            //mDictionary.Add(baseFromDir + relativeFileName, 
            //    baseToDir + relativeFileName);
            originals.Add(baseFromDir + relativeFileName);
            backups.Add(baseToDir + relativeFileName);
        }

        public string getOriginalFile(int i)
        {
            return originals[i];
        }

        public string getRelativeFile(int i)
        {
            return originals[i].Remove(0, baseFromDir.Length);
        }

        public string getBackupFile(int i)
        {
            return backups[i];
        }

        public int Count
        {
            get { return originals.Count; }
        }

        public void Print()
        {
            for (int i = 0; i < originals.Count; i++)
            {
                Console.WriteLine(originals[i] + " -> " + backups[i]);
            }
        }
    }
}
