using System;
using Microsoft.SPOT;
using Gadgeteer.Networking;
using System.Collections;
using System.Xml;

namespace GadgeteerApp
{
    class WebServiceClient
    {
        private string baseuri;
        private ArrayList items;

        public WebServiceClient(string address)
        {
            items = new ArrayList();
            baseuri = "http://" + address + "/api/";
        }

        // Get the list of items from the Web Service
        public void getItems()
        {
            Uri uri = new Uri(baseuri + "Items/");

            var itemreq = Gadgeteer.Networking.WebClient.GetFromWeb(uri.ToString());
            itemreq.Accepts = "application/xml";
            itemreq.ResponseReceived += Itemreq_ResponseReceived;
            itemreq.SendRequest();
            Debug.Print("Items GET request sent...");
        }

        private void Itemreq_ResponseReceived(HttpRequest sender, HttpResponse response)
        {
            if (response.StatusCode == "200")
            {
                Debug.Print("Items Request succeeded");
                try
                {
                    using (XmlReader xml = XmlReader.Create(response.Stream))
                    {
                        while (xml.ReadToFollowing("Item"))
                        {
                            xml.ReadStartElement("ItemID");
                            int itemID = int.Parse(xml.ReadString());
                            xml.ReadStartElement("Name");
                            string name = xml.ReadString();
                            xml.ReadStartElement("Points");
                            int points = int.Parse(xml.ReadString());
                            var item = new Item() { ItemID = itemID, Name = name, Points = points };
                            items.Add(item);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Print("XML parsing failed with exception:\n" + e.Message);
                }
            }
            else
            {
                //Show a helpful error message
                Debug.Print("Items Request failed with status code " + response.StatusCode);
                Debug.Print("Response text: " + response.Text);
            }
        }

        // Get the list of items from the Web Service
        public void submitImage(int itemId, Bitmap image)
        {
            Uri uri = new Uri(baseuri + "Items/" + itemId + "/Submit/");

            //var imagereq = WebRequest.CreateHttp(uri.Uri);
            POSTContent content = POSTContent.CreateBinaryBasedContent(image.GetBitmap());
            var imagereq = HttpHelper.CreateHttpPostRequest(uri.AbsoluteUri, content, "image/png");
            imagereq.ResponseReceived += Imagereq_ResponseReceived;
            imagereq.SendRequest();
            Debug.Print("Image POST request sent...");
        }

        private void Imagereq_ResponseReceived(HttpRequest sender, HttpResponse response)
        {
            if (response.StatusCode == "201")
            {
                Debug.Print("Image POST succeeded");
                try
                {
                    using (XmlReader xml = XmlReader.Create(response.Stream))
                    {
                        while (xml.ReadToFollowing("ImageSubmission"))
                        {
                            xml.ReadStartElement("ImageID");
                            int imageID = int.Parse(xml.ReadString());
                            xml.ReadStartElement("ItemID");
                            int itemID = int.Parse(xml.ReadString());
                            xml.ReadStartElement("Labels");
                            // Labels....
                            xml.ReadStartElement("VerificationResult");
                            bool result = xml.ReadString() == "true";

                            // TODO use image result (i.e. Go to next image and parse points)
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Print("XML parsing failed with exception:\n" + e.Message);
                }
            }
            else
            {
                //Show a helpful error message
                Debug.Print("Image POST failed with status code " + response.StatusCode);
                Debug.Print("Response text: " + response.Text);
            }
        }
    }
}
