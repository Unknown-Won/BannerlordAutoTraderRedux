using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace AutoTrader
{
    /// <summary>
    /// Saved settings profiles: one file per profile in Configs\AutoTraderProfiles\[name].xml.
    /// A profile uses the same setting names and values as AutoTraderConfig.xml.
    /// </summary>
    public static class AutoTraderProfiles
    {
        public const int MaxNameLength = 40;

        private static readonly PlatformDirectoryPath profilesPath = new PlatformDirectoryPath(EngineFilePaths.ConfigsPath.Type, EngineFilePaths.ConfigsPath.Path + "/AutoTraderProfiles/");

        /// <summary>
        /// Removes characters that can't be in a file name. Returns an empty string if nothing usable is left.
        /// </summary>
        public static string CleanName(string name)
        {
            if (name == null)
                return "";

            char[] invalid = System.IO.Path.GetInvalidFileNameChars();
            StringBuilder sb = new StringBuilder();
            foreach (char c in name)
            {
                if (Array.IndexOf(invalid, c) < 0)
                    sb.Append(c);
            }

            string cleaned = sb.ToString().Trim().TrimEnd('.');
            if (cleaned.Length > MaxNameLength)
                cleaned = cleaned.Substring(0, MaxNameLength).Trim();
            return cleaned;
        }

        public static List<string> GetProfileNames()
        {
            List<string> names = new List<string>();
            try
            {
                foreach (PlatformFilePath file in FileHelper.GetFiles(profilesPath, "*.xml", SearchOption.TopDirectoryOnly))
                    names.Add(file.GetFileNameWithoutExtension());
            }
            catch (Exception)
            {
                // The folder doesn't exist until the first profile is saved
            }

            return names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static bool Exists(string name)
        {
            return GetProfileNames().Any(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase));
        }

        public static void Save(string name, IEnumerable<KeyValuePair<string, string>> settings)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.IndentChars = "  ";
            xmlWriterSettings.NewLineChars = "\r\n";
            xmlWriterSettings.NewLineHandling = NewLineHandling.Replace;
            StringBuilder sb = new StringBuilder();

            using (XmlWriter writer = XmlWriter.Create(sb, xmlWriterSettings))
            {
                writer.WriteStartElement("profile");
                foreach (KeyValuePair<string, string> setting in settings)
                    writer.WriteElementString(setting.Key, setting.Value);
                writer.WriteEndElement();
            }

            FileHelper.SaveFileString(GetPath(name), sb.ToString());
        }

        /// <summary>
        /// Reads a profile's settings by name. Returns null if the file is missing or isn't valid XML.
        /// </summary>
        public static Dictionary<string, string> Load(string name)
        {
            PlatformFilePath path = GetPath(name);
            if (!FileHelper.FileExists(path))
                return null;

            try
            {
                Dictionary<string, string> settings = new Dictionary<string, string>();
                XmlDocument document = new XmlDocument();
                document.LoadXml(FileHelper.GetFileContentString(path));
                if (document.DocumentElement == null)
                    return null;

                foreach (XmlNode node in document.DocumentElement.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                        settings[node.Name] = node.InnerText.Trim();
                }
                return settings;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void Delete(string name)
        {
            PlatformFilePath path = GetPath(name);
            if (FileHelper.FileExists(path))
                FileHelper.DeleteFile(path);
        }

        private static PlatformFilePath GetPath(string name)
        {
            return new PlatformFilePath(profilesPath, name + ".xml");
        }
    }
}
