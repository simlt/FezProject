using System;
using Microsoft.SPOT;
using System.Collections;

namespace GadgeteerApp
{
    class Game
    {
        private Hashtable items;
        private WebServiceClient client;

        public int CurrentPoints { get; private set; }
        public int TotalPoints { get; private set; }
        public int GameID { get; private set; }
        private delegate void GameLoadedHandler(GameSession session);
        private event GameLoadedHandler GameLoaded;

        private IEnumerator itemEnum;
        public Item CurrentItem { get; private set; }

        public Game(WebServiceClient client)
        {
            this.client = client;
            items = new Hashtable();
            GameLoaded += OnGameLoad;

            // Request new Game
            client.createGame(g => GameLoaded(g));
            
            // Fill item list
            //client.getItems(itemsHandler);
        }

        private void OnGameLoad(GameSession session)
        {
            GameID = session.GameID;
            loadItems(session.items);
            Debug.Print("Game session successfully started with id: " + GameID);
        }

        private void loadItems(ArrayList itemList)
        {
            /*
             * TODO itemList is already in random order, so we use its enumerator to enum each item.
             * We only memorize the Hashtable items here so we *may* lose the random generated order.
             */
            CurrentPoints = 0;
            if (itemList.Count > 0)
            {
                foreach (Item item in itemList)
                {
                    items.Add(item.ItemID, item);
                    TotalPoints += item.Points;
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
                CurrentPoints += item.Points;
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

            client.submitImage(GameID, item.ItemID, picture, verifyImage);
        }
    }
}
