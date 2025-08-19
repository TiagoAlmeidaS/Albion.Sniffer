using System.Collections.Concurrent;
using System.Xml.Serialization;

namespace AlbionOnlineSniffer.Core.Models.Dependencies.Template
{
    [XmlRoot(ElementName = "layer")]
    public class Layer
    {
        [XmlAttribute(AttributeName = "id")] public string Id { get; set; }

        [XmlAttribute(AttributeName = "name")] public string Name { get; set; }

        [XmlElement(ElementName = "tile")] public List<Tile> Tiles { get; set; }

        [XmlElement(ElementName = "compoundtile")]
        public List<CompoundTile> CompoundTiles { get; set; }

        public Layer()
        {
            Id = string.Empty;
            Name = string.Empty;
            Tiles = new List<Tile>();
            CompoundTiles = new List<CompoundTile>();
        }
    }

    [XmlRoot(ElementName = "tile")]
    public class Tile
    {
        [XmlAttribute(AttributeName = "name")] public string Name { get; set; }

        [XmlAttribute(AttributeName = "pos")] public string Pos { get; set; }

        [XmlAttribute(AttributeName = "roty")] public string RotY { get; set; }

        [XmlAttribute(AttributeName = "scale")]
        public string Scale { get; set; }

        [XmlAttribute(AttributeName = "rot")] public string Rot { get; set; }

        public Tile()
        {
            Name = string.Empty;
            Pos = string.Empty;
            RotY = string.Empty;
            Scale = string.Empty;
            Rot = string.Empty;
        }
    }

    [XmlRoot(ElementName = "compoundtile")]
    public class CompoundTile
    {
        [XmlAttribute(AttributeName = "name")] public string Name { get; set; }

        [XmlAttribute(AttributeName = "pos")] public string Pos { get; set; }

        [XmlAttribute(AttributeName = "roty")] public string RotY { get; set; }

        public CompoundTile()
        {
            Name = string.Empty;
            Pos = string.Empty;
            RotY = string.Empty;
        }
    }

    [XmlRoot(ElementName = "layergroup")]
    public class LayerGroup
    {
        [XmlElement(ElementName = "layer")] public List<Layer> Layers { get; set; }

        [XmlAttribute(AttributeName = "name")] public string Name { get; set; }

        public LayerGroup()
        {
            Layers = new List<Layer>();
            Name = string.Empty;
        }
    }

    [XmlRoot(ElementName = "tiles")]
    public class TilesRoot
    {
        [XmlElement(ElementName = "layergroup")]
        public List<LayerGroup> LayerGroups { get; set; }

        [XmlElement(ElementName = "tile")] public List<Tile> Tiles { get; set; }

        public TilesRoot()
        {
            LayerGroups = new List<LayerGroup>();
            Tiles = new List<Tile>();
        }
    }

    [XmlRoot(ElementName = "template")]
    public class Template
    {
        [XmlElement(ElementName = "tiles")] public TilesRoot TilesRoot { get; set; }

        public Template()
        {
            TilesRoot = new TilesRoot();
        }
    }

    public static class TemplateData
    {
        private static readonly ConcurrentDictionary<string, Template> Templates 
            = new ConcurrentDictionary<string, Template>();

        public static Template GetTemplate(string path, string name)
        {
            if (!name.EndsWith(".template.xml"))
            {
                name += ".template.xml";
            }
            
            var key = $"{path}/{name}";
            
            if (Templates.TryGetValue(key, out var template))
            {
                return template;
            }
            
            try
            {
                // Usar Directory.GetCurrentDirectory() em vez de Pathfinder.mainFolder
                var basePath = Directory.GetCurrentDirectory();
                var templatePath = Path.Combine(basePath, $"ao-bin-dumps/templates/{path}/{name}");
                
                template = XmlTools.Deserialize<Template>(templatePath);
                Templates.TryAdd(key, template);
                return template;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Can't read template data {path}/{name}: " + e.Message);
            }

            return new Template();
        }
    }
}