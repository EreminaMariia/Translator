using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TranslatorWPF
{
    public class SubtitleManager
    {
        private readonly string _path;
        private string _subPath;
        private readonly string _toolNixPath;
        private List<string> _fileInfo;
        private List<Subtitle> _subtitles;
        private int _id;
        private readonly Translator _translator;

        public SubtitleManager(string path)
        {
            _path = path;
            _toolNixPath = @"..\..\..\..\MKVToolNix";
            _subtitles = new List<Subtitle>();
            _fileInfo = new List<string>();
            _translator = new Translator();
            _id = 0;  
        }

        public List<Subtitle> TranslateSubtitles(int start, int end)
        {
            for (int i =0; i<_subtitles.Count();i++)
            {
                if(i >=start && i<=end)
                _subtitles[i].Translated = _translator.EngToRus(_subtitles[i].Text);
            }

            return _subtitles;
        }
        public List<Subtitle> GetSubtitles()
        {
            List<string> lines = new List<string>();
            var extention = Path.GetExtension(_path);

            if (extention is null || extention == string.Empty)
            {
                return _subtitles;
            }

            if (extention == ".mkv")
            {
                lines = MKVExtract();
            }

            else if (extention == ".ass" || extention == ".srt")
            {
                _subPath = _path;
                lines = GetSubtitles(_path);
            }

            for (int i = 0; i < lines.Count; i++)
            {
                //if (!lines[i].StartsWith("Dialogue:"))
                //{
                //    _fileInfo.Add(lines[i]);
                //}
                //else
                //{
                    _subtitles.Add(MakeSubFromString(lines[i]));
                //}
            }
            return _subtitles;
        }

        private Subtitle MakeSubFromString(string line)
        {

            Regex formatRex = new Regex(@"Dialogue: \w*,\d+:\d+:\d+.\d+,\d+:\d+:\d+.\d+,(.*?),(.*?),\w*,\w*,\w*,(.*?),");
            Regex bracketsRex = new Regex(@"\{(.*?)\}");
            Match match = formatRex.Match(line);

            string temp = formatRex.Replace(line, "");
            temp = bracketsRex.Replace(temp, "");
            temp = temp.Replace("\\N", " ");
            //string translated = _translator.EngToRus(temp);
            _id++;
            string tempInfo = match.Value;
            //Regex infoRex = new Regex(@"Dialogue: \w*,\d+:\d+:\d+.\d+,\d+:\d+:\d+.\d+,");
            //string info = infoRex.Replace(tempInfo, "");
            string[] tempArr = match.Value.Split(',');

            return new Subtitle { Text = temp, Info = tempInfo, Translated = "-", Start = TimeSpan.Parse(tempArr[1]), Finish = TimeSpan.Parse(tempArr[2]) };
        }

        private List<string> MKVExtract()
        {
            var mkvInfo = GetMkvInfo();
            ExtractSubtitles(mkvInfo);
            return GetSubtitles(_subPath);
        }

        private string GetMkvInfo()
        {
            var mkvInfo = string.Empty;
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;

            using (Process p = new Process())
            {

                p.StartInfo = info;
                p.Start();

                using (StreamWriter sw = p.StandardInput)
                {
                    if (sw.BaseStream.CanWrite)
                    {
                        sw.WriteLine("cd " + _toolNixPath);
                        sw.WriteLine("mkvmerge -i " + "\"" + _path + "\"");
                    }
                }
                mkvInfo = p.StandardOutput.ReadToEnd();
            }

            return mkvInfo;
        }

        private void ExtractSubtitles(string mkvInfo)
        {
            Regex infoRex = new Regex(@"\d+: subtitles");
            Match match = infoRex.Match(mkvInfo);
            string pathNumber = match.Value.Split(':')[0];

            using (Process s = new Process())
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "cmd.exe";
                info.RedirectStandardInput = true;
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
                s.StartInfo = info;
                s.Start();

                _subPath = Path.GetFileNameWithoutExtension(_path) + ".ass";

                using (StreamWriter sw = s.StandardInput)
                {
                    if (sw.BaseStream.CanWrite)
                    {
                        var extractLine = $"mkvextract \"{_path}\" tracks {pathNumber}:\"{_subPath}\"";
                        sw.WriteLine("cd " + _toolNixPath);
                        sw.WriteLine(extractLine);
                    }
                }
                s.WaitForExit();
            }
        }

        private List<string> GetSubtitles(string filename)
        {
            var path = Path.Combine(_toolNixPath, filename);
            var lines = File.ReadAllLines(path);
            _fileInfo.AddRange(lines.Where(x => !x.StartsWith("Dialogue")).ToList());
            return lines.Where(x => x.StartsWith("Dialogue")).ToList();
        }

        public List<Subtitle> ShiftTime(TimeSpan shift, bool forward)
        {
            foreach (Subtitle s in _subtitles)
            {
                if (forward)
                {
                    s.Start += shift;
                    s.Finish += shift;
                }
                else
                {
                    s.Start -= shift;
                    s.Finish -= shift;
                }
            }
            return _subtitles;
        }

        public List<Subtitle> EditTranslated(int id, string translatedText)
        {
            Subtitle sub = _subtitles[id];
            sub.Translated = translatedText;
            return _subtitles;
        }

        public void SaveSubtitles()
        {
            string writePath = Path.GetDirectoryName(_path)+ "\\" + Path.GetFileNameWithoutExtension(_path) + ".ass";
            using (StreamWriter sw = new StreamWriter(writePath, false, System.Text.Encoding.Default))
            {
                foreach (string f in _fileInfo)
                {
                    sw.WriteLine(f);
                }
                foreach (Subtitle s in _subtitles)
                {
                    string[] tempArr = s.Info.Split(',');
                    sw.Write(tempArr[0]+",");
                    sw.Write(s.Start.ToString() + ",");
                    sw.Write(s.Finish.ToString() + ",");
                    for (int i = 3; i < tempArr.Length; i++)
                    {
                            sw.Write(tempArr[i]);                       
                        if (i != tempArr.Length - 1)
                        {
                            sw.Write(',');
                        }
                    }
                    sw.WriteLine(s.Translated);
                }
            }
            string nixPath = _toolNixPath + Path.GetFileNameWithoutExtension(_path) + ".ass";
            if (File.Exists(nixPath))
            {
                File.Delete(nixPath);
            }
        }
    }
}
