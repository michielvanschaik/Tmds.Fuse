using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Tmds.Fuse;

namespace ConfigFiles
{
    public class ConfigFile
    {
        protected string _name;
        public string Name { get { return _name; } }
        protected byte[] _path = Encoding.UTF8.GetBytes("");
        public byte[] Path { get { return _path; } }

        protected string _sourcePath;

        protected byte[] _content = Encoding.UTF8.GetBytes("");
        public byte[] Content { get { return _content; } }

        public int Size { get { return _content.Length; } }

        public ConfigFile(string name, string sourcePath)
        {
            _name = name;
            _sourcePath = sourcePath;

            _path = Encoding.UTF8.GetBytes("/" + _name);

            if (File.Exists(_sourcePath))
            {
                string sourceContent = File.ReadAllText(_sourcePath);
                string transformedContent = Transform(sourceContent);
                _content = Encoding.UTF8.GetBytes(transformedContent);
            }
        }

        protected string Transform(string source)
        {
            string transformed = source;

            List<string> secrets = new();

            string pattern = @"\[\[(\w+)\:(\w+)\]\]";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = r.Match(transformed);
            while (match.Success)
            {
                Group type = match.Groups[1];
                switch (type.Value)
                {
                    case "Secret":
                        Group tag = match.Groups[2];
                        secrets.Add(tag.Value);
                        break;
                }
                match = match.NextMatch();
            }

            Console.WriteLine($"Found {secrets.Count} secrets: {String.Join(", ", secrets)}");

            // TODO: Get Secrets using Secret Manager

            transformed = transformed.Replace("[[Secret:username]]", "edgeuser");
            transformed = transformed.Replace("[[Secret:password]]", "edgepassword");

            return transformed;
        }
    }
}
