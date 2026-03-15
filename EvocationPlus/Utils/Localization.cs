using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityModManagerNet;

namespace EvocationPlus.Utils
{
    public static class Localization
    {
        private static readonly Dictionary<string, string> Strings =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static bool TryGet(string key, out string value)
        {
            return Strings.TryGetValue(key, out value);
        }

        public static void LoadFromResx(UnityModManager.ModEntry mod)
        {
            Strings.Clear();

            var resxPath = Path.Combine(mod.Path, "Data", "Localization", "EvocationPlus.resx");
            try
            {
                var doc = new XmlDocument();
                doc.Load(resxPath);

                // Standard resx shape: <root><data name="KEY" ...><value>TEXT</value></data>...</root>
                var dataNodes = doc.SelectNodes("/root/data");
                if (dataNodes != null)
                    foreach (XmlNode data in dataNodes)
                    {
                        var nameAttr = data.Attributes?["name"];
                        if (nameAttr == null) continue;

                        var key = nameAttr.Value;
                        if (string.IsNullOrEmpty(key)) continue;

                        // Find the <value> child (ignore comments / other nodes)
                        var valueNode = data.SelectSingleNode("value");
                        var value = valueNode?.InnerText;

                        if (value != null)
                            Strings[key] = value;
                    }
            }
            catch (Exception ex)
            {
                TryWrite(Path.Combine(mod.Path, "resx_probe_exception.txt"), ex.ToString());

                TryWrite(Path.Combine(mod.Path, "localization_loaded.txt"), "Exception: " + ex);
                mod.Logger.Log("EvocationPlus: LoadFromResx exception: " + ex);
            }
        }


        private static void TryWrite(string path, string text)
        {
            try
            {
                File.WriteAllText(path, text);
            }
            catch
            {
                Main.Mod.Logger.Log("Method TryWrite Failed in Localization.cs");
            }
        }
    }
}