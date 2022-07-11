using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Utilities;

namespace SiProjectAnalyzer.Service
{
    internal class Network
    {
        public static void CountNetWorkElement(FileInfo xmlInfo)
        {
            var xml = XElement.Load(xmlInfo.FullName);
            if (!xml.TryGetTargetElemnts("NetworkSource", out var networks)) return;
            
            var wires = new List<Wire>();
            foreach (var network in networks)
            {
                var parts = new List<Part>();
                if (!network.TryGetTargetElemnts("Part", out var partsXele)) continue;
                foreach (var p in partsXele)
                {
                    if (!p.TryGetAttributeValue("Name", out var partName)) continue;
                    if (!p.TryGetAttributeValue("UId", out var partUid)) continue;
                    parts.Add(new Part(partName, int.Parse(partUid)));
                }

                if (!network.TryGetTargetElemnts("Wire", out var wiresXele)) continue;
                foreach (var w in wiresXele)
                {
                    var isRoot = w.TryGetTargetElement("Powerrail", out var _);

                    if (!w.TryGetAttributeValue("UId", out var wireUid)) continue;

                    if (!w.TryGetTargetElemnts("NameCon", out var partsNameCon)) continue;
                    var wireParts = new List<Part>();
                    foreach (var na in partsNameCon)
                    {
                        if (!na.TryGetAttributeValue("Name", out var conName)) continue;
                        if (!na.TryGetAttributeValue("UId", out var uidConName)) continue;
                        wireParts.Add(new Part(conName, int.Parse(uidConName)));
                    }
                    wires.Add(new Wire(isRoot,int.Parse(wireUid), wireParts));
                }
                var start = wires.Where(w => w.IsRoot);
                var connect = wires.Where(c => !c.IsRoot);
                foreach(var w in wires.Where(w => w.IsRoot))
                {
                    var uid = w.Parts.Select(p => p.Uid);
                }
            }
        }


        public class Part
        {
            public string? Name { get; }
            public int Uid { get; }
            public Part(string name, int uid)
            {
                Name = name;
                Uid = uid;
            }
        }

        public class Wire
        {
            public int Uid { get; }
            public bool IsRoot { get; }
            public IReadOnlyList<Part> Parts { get; }
            public Wire(bool isRoot, int uid, IReadOnlyList<Part> parts)
            {
                Uid = uid;
                IsRoot = isRoot;
                Parts = parts;
            }
        }
    }
}
