using System.Xml;

namespace GadgeteerApp
{
    internal class Item
    {
        public int ItemID { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public bool Found { get; set; }

        internal Item(XmlReader xml)
        {
            Found = false;
            xml.ReadStartElement("Item"); // Item
            xml.ReadStartElement("ItemID");
            ItemID = int.Parse(xml.ReadString());
            xml.ReadEndElement();
            xml.ReadStartElement("Name");
            Name = xml.ReadString();
            xml.ReadEndElement();
            xml.ReadStartElement("Points");
            Points = int.Parse(xml.ReadString());
            xml.ReadEndElement();
            xml.ReadEndElement(); // Item
        }
    }
}