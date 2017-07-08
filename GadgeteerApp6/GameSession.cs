using System;
using Microsoft.SPOT;
using System.Collections;
using System.Xml;

namespace GadgeteerApp
{
    // GameRequest struct
    class GameSession
    {
        internal int GameID;
        internal ArrayList items; // ArrayList<Item>

        public GameSession(XmlReader xml)
        {
            items = new ArrayList();
            /* Example XML
                <Game>
                  <GameID>5</GameID>
                  <Items>
                    <Item>
                      <ItemID>1</ItemID><Name>Shoe</Name><Points>50</Points>
                    </Item>
                    <Item>
                      <ItemID>2</ItemID><Name>PC</Name><Points>150</Points>
                    </Item>
                  </Items>
                </Game>
            */
            xml.ReadStartElement("Game");
            xml.ReadStartElement("GameID");
            GameID = int.Parse(xml.ReadString());
            xml.ReadEndElement();
            xml.ReadStartElement("Items");
            while (xml.NodeType == XmlNodeType.Element && xml.Name == "Item")
            {
                items.Add(new Item(xml));
            }
            xml.ReadEndElement(); // Items
            xml.ReadEndElement(); // Game
        }
    }
}
