using System;
using Microsoft.SPOT;
using System.Collections;

namespace GadgeteerApp
{
    class Game
    {
        // ArrayList<Item>
        private ArrayList items;
        private WebServiceClient client;

        public Item CurrentItem
        {
            get; private set;
        }

        public Game(WebServiceClient client)
        {
            this.client = client;

            // Fill item list
            items = client.getItems();
        }

        void itemResponse(ArrayList items)
        {
            if (items.Count > 0)
                CurrentItem = items[0] as Item;
            else
            {
                Debug.Print("Loaded an empty item list for game.");
                return;
            }
        }
    }
}
