using System;
using Microsoft.SPOT;
using System.Collections;

namespace GadgeteerApp
{
    class Game : Application
    {
        // ArrayList<Item>
        private ArrayList items;
        private ArrayList itemsOK;
        private WebServiceClient client;
        private int points;

        public Item CurrentItem
        {
            get; private set;
        }

        public Game(WebServiceClient client)
        {
            items = new ArrayList();
            itemsOK = new ArrayList();
            this.client = client;

            // Fill item list
            client.getItems(itemsHandler);
            // itemResponse(items);
            submitImage(new Gadgeteer.Picture(DebugEMU.testImage, Gadgeteer.Picture.PictureEncoding.BMP));
        }

        private void itemsHandler(object obj)
        {
            ArrayList items = obj as ArrayList;
            this.items = items;
            if (items.Count > 0)
                CurrentItem = items[0] as Item;
            else
            {
                Debug.Print("Loaded an empty item list for game.");
                return;
            }
        }

        public void submitImage(Gadgeteer.Picture picture)
        {
            Item item = CurrentItem;
            if (item == null)
            {
                Debug.Print("No CurrentImage is set for submitImage");
                return;
            }

            client.submitImage(item.ItemID, picture, (object obj) =>
            {
                ImageSubmission image = obj as ImageSubmission;
                // if image was OK
                if (image.VerificationResult)
                {
                    points += item.Points;
                    items.Remove(item);
                    itemsOK.Add(item);
                }
                else
                {
                    Debug.Print("Image for item: " + item.Name + " did not match");
                }
            });
        }
    }
}
