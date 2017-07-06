using System;
using Microsoft.SPOT;
using System.Collections;

namespace GadgeteerApp
{
    class Game : Application
    {
        // ArrayList<Item>
        private Hashtable items;
        private WebServiceClient client;
        private int points;
        private IEnumerator itemEnum;

        public Item CurrentItem
        {
            get; private set;
        }

        public Game(WebServiceClient client)
        {
            items = new Hashtable();
            this.client = client;

            // Fill item list
            client.getItems(itemsHandler);
            // itemResponse(items);
            // test submitImage
            submitImage(new Gadgeteer.Picture(DebugEMU.testImage, Gadgeteer.Picture.PictureEncoding.BMP));
        }

        private void itemsHandler(object obj)
        {
            ArrayList itemList = obj as ArrayList;
            if (itemList.Count > 0)
            {
                foreach (Item item in itemList)
                {
                    items.Add(item.ItemID, item);
                }
                // Enumerator becomes invalid after the table is modified!
                itemEnum = items.GetEnumerator();
                // Set first item
                nextItem();
            }
            else
            {
                Debug.Print("Loaded an empty item list for game.");
                return;
            }
        }

        private void nextItem()
        {
            bool rescanFromBeginning = true;
            while (true)
            {
                while (itemEnum.MoveNext())
                {
                    var entry = itemEnum.Current as DictionaryEntry;
                    Item item = entry.Value as Item;
                    if (!item.Found)
                    {
                        CurrentItem = item;
                        return;
                    }
                }
                // Reset to beginning and scan again
                if (rescanFromBeginning)
                {
                    rescanFromBeginning = false;
                    itemEnum.Reset();
                }
                break;
            }
            // If we end without getting a next item, we have completed the list!
            Debug.Print("Game ended!");
            // TODO handle game ending
        }

        private void verifyImage(ImageSubmission image)
        {
            Item item = items[image.ItemID] as Item;
            if (image.VerificationResult)
            {
                // if image was OK
                points += item.Points;
                item.Found = true;
                nextItem();
                Debug.Print("Image for item: " + item.Name + " matched successfully");
            }
            else
            {
                Debug.Print("Image for item: " + item.Name + " did not match");
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
                verifyImage(image);
            });
        }
    }
}
