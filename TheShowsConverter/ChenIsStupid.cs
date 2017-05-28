using NReco.VideoConverter;
using System;
﻿using System.Collections.Generic;
﻿using System.IO;
﻿using System.Linq;
﻿using System.Windows.Forms;

namespace TheShowsConverter
{
    public partial class ChenIsStupid : Form
    {
        private const string LOG_PATH = "c:\\logs\\theShowsConverter.log";
        private readonly string _cid;

        public ChenIsStupid()
        {
            InitializeComponent();

            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            Hide();

            _cid = Guid.NewGuid().ToString();
            //TODO: meinhh |28-05-2017|- process will fail if two files run at the same time. logs file names should include start time or _cid

            using (StreamWriter writter = File.AppendText(LOG_PATH))
            {
                try
                {
                    Log(w, "Program started");
                    var filesToConvert= GetFilesToConvert(writter);

                    ConvertFiles(writter, filesToConvert);
                }
                catch (Exception ex)
                {
                    Log(w, $"FAILURE! ex: {ex}");
                }
                finally
                {
                    Close();
                }
            }
        }
        
       private IEnumerable<string> GetFilesToConvert(StreamWriter writer)
        {
            try
            {
                IEnumerable<string> files;
                var args = Environment.GetCommandLineArgs();
                //TODO: meinhh |28-05-2017|- what is args[0]?
                string location = args[1];
                FileAttributes attr = File.GetAttributes(location);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    files = Directory.EnumerateFiles(location, "*.*", SearchOption.AllDirectories)
                    .Where(s => s.EndsWith(".mkv") || s.EndsWith(".avi"));
                }
                else
                {
                    files = new[] {location};
                }

                Log(writer, $"Params recived. directory: {location}");

                return files;
            }
            catch (Exception ex)
            {
                throw new Exception("Getting params failed.", ex);
            }
        }     

        private void ConvertFiles(StreamWriter w, IEnumerable<string> files)
        {        

            var converter = new FFMpegConverter();

            foreach (var fileName in files)
            {
                ConvertFile(w, converter, fileName);
            }
        }

        private void ConvertFile(StreamWriter w, FFMpegConverter converter,  string mkvPath)
        {
            try
            {
                var mp4Path = Path.ChangeExtension(mkvPath, "mp4");

                converter.ConvertMedia(mkvPath, mp4Path, Format.mp4);

                //File.Delete(mkvPath);
            }
            catch (Exception ex)
            {
                Log(w, $"FAILURE! exception occured while converting the file {mkvPath}. ex: {ex}");
            }
        }

        private void Log(StreamWriter w, string message)
        {
            w.WriteLine($"{_cid} | {DateTime.Now.ToString()} | {message}");
        }
    }
}
