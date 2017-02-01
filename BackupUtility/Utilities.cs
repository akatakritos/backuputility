using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BackupUtility
{
    public class Utilities
    {
        public static AnalysisResults Analyze(string dirFrom, string dirTo)
        {
            AnalysisResults ar = new AnalysisResults(dirFrom,dirTo);
            foreach (string file in
                Directory.GetFiles(dirFrom, "*.*", SearchOption.AllDirectories))
            {
                string relativeFile = file.Remove(0, dirFrom.Length);
                string backupFile = dirTo + relativeFile;
                if (!File.Exists(backupFile))
                    ar.Add(relativeFile);
                else if (File.GetLastWriteTime(file) > File.GetLastWriteTime(backupFile))
                    ar.Add(relativeFile);
            }

            return ar;
        }

        public static void Backup(AnalysisResults ar)
        {
            for (int i = 0; i < ar.Count; i++)
            {
                backupOne(ar, i);
                ar.CurrentFileIndex = i;
            }
        }

        public static void BackupNext(AnalysisResults ar)
        {
            if (ar.CurrentFileIndex >= ar.Count)
                throw new ArgumentOutOfRangeException("All files have been backed up!");

            backupOne(ar, ar.CurrentFileIndex);            
            ar.CurrentFileIndex++;
        }

        private static void backupOne(AnalysisResults ar, int i)
        {
            if (!Directory.Exists(Path.GetDirectoryName(ar.getBackupFile(i))))
                Directory.CreateDirectory(Path.GetDirectoryName(ar.getBackupFile(i)));
            File.Copy(ar.getOriginalFile(i), ar.getBackupFile(i), true);
        }
    }
}
