using Gadgeteer.Networking;
using Microsoft.SPOT;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Xml;
using GT = Gadgeteer;

namespace GadgeteerApp
{
    // NOTE: The webservice should handle the disconnection of the network and warn the Game somehow
    class WebServiceClient
    {
        private string baseuri;
        private Network network;

        /*private ArrayList cachedRequest;
        public delegate void ResponseHandler(Object arg1);
        public delegate void ItemResponseHandler(ArrayList items);*/

        public WebServiceClient(string address, Network network)
        {
            baseuri = "http://" + address + "/api/";
            this.network = network;
            //cachedRequest = new ArrayList();
        }

        private bool isNetworkUp()
        {
            return network.GetNetworkStatus();
        }

        // Get the list of items from the Web Service
        public ArrayList getItems()
        {
            ArrayList list = null;
            try
            {
                list = sendItemRequestSync();
            }
            catch (WebException)
            {
                Debug.Print("Items Request timed out");
            }
            return list;
        }

        private ArrayList sendItemRequestSync()
        {
            Uri uri = new Uri(baseuri + "Items/");
            ArrayList items = new ArrayList();

            var itemreq = WebRequest.Create(uri) as HttpWebRequest;
            itemreq.Method = "GET";
            itemreq.Accept = "application/xml";

            Debug.Print("Sending items GET request...");
            using (var response = itemreq.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Debug.Print("Items Request succeeded");
                    try
                    {
                        using (XmlReader xml = XmlReader.Create(response.GetResponseStream()))
                        {
                            while (xml.ReadToFollowing("Item"))
                            {
                                xml.Read(); // consume item CHECK if this works
                                xml.ReadStartElement("ItemID");
                                int itemID = int.Parse(xml.ReadString());
                                xml.ReadEndElement();
                                xml.ReadStartElement("Name");
                                string name = xml.ReadString();
                                xml.ReadEndElement();
                                xml.ReadStartElement("Points");
                                int points = int.Parse(xml.ReadString());
                                xml.ReadEndElement();
                                xml.ReadEndElement(); // Item
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
                    Debug.Print("Items Request failed with status code " + response.StatusCode + ": " + response.StatusDescription);
                }
            }
            return items;
        }

        // Send an image to be labeled by the WebService
        public void submitImage(int itemId, GT.Picture image)
        {
            if (image.Encoding != Gadgeteer.Picture.PictureEncoding.BMP)
                throw new ArgumentException("Unsupported image encoding used for submit");

            HttpWebRequest req = null;

            try
            {
                Uri uri = new Uri(baseuri + "Items/" + itemId + "/Submit/");
                byte[] imagedata = image.PictureData;
                /*var imagereq = WebRequest.CreateHttp(uri.Uri);
                POSTContent content = POSTContent.CreateBinaryBasedContent(image.PictureData);
                var imagereq = HttpHelper.CreateHttpPostRequest(uri.AbsoluteUri, content, "image/bmp");
                imagereq.ResponseReceived += Imagereq_ResponseReceived;
                imagereq.SendRequest();*/

                req = HttpWebRequest.Create(uri) as HttpWebRequest;
                //req.Timeout = Timeout.Infinite;
                req.Method = "POST";
                req.Accept = "application/xml";
                req.ContentType = "image/bmp";
                req.ContentLength = imagedata.Length;
                //req.SendChunked = true;
                using (Stream stream = req.GetRequestStream())
                {
                    stream.Write(imagedata, 0, imagedata.Length);
                }
            }
            // TODO rivedere gestione eccezioni!
            catch (Exception e)
            {
                Debug.Print("Image POST request failed with exception:\n" + e.Message);
                return;
            }
            Debug.Print("Image POST request sent...");


            // Send request and wait for response (blocking)
            using (var response = req.GetResponse() as HttpWebResponse)
            {
                try
                {
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        Debug.Print("Image POST succeeded");
                        using (Stream stream = response.GetResponseStream())
                        {
                            var imagesub = new ImageSubmission(stream);

                            // TODO use image result (i.e. Go to next image and parse points)
                        }
                    }
                    else
                    {
                        //Show a helpful error message
                        Debug.Print("Image POST failed with status code " + response.StatusCode);
                        Debug.Print("Response text: " + response.StatusDescription);
                    }
                }
                // TODO rivedere gestione eccezioni!
                catch (Exception e)
                {
                    Debug.Print("Image response parsing failed with exception:\n" + e.Message);
                }
            }
        }

        /*private void sendItemRequest()
        {
            Uri uri = new Uri(baseuri + "Items/");

            var itemreq = Gadgeteer.Networking.WebClient.GetFromWeb(uri.ToString());
            itemreq.Accepts = "application/xml";
            itemreq.ResponseReceived += Itemreq_ResponseReceived;
            itemreq.SendRequest();
            Debug.Print("Items GET request sent...");
        }*/

        /*private void Itemreq_ResponseReceived(HttpRequest sender, HttpResponse response)
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
                            xml.Read(); // consume item CHECK if this works
                            xml.ReadStartElement("ItemID");
                            int itemID = int.Parse(xml.ReadString());
                            xml.ReadEndElement();
                            xml.ReadStartElement("Name");
                            string name = xml.ReadString();
                            xml.ReadEndElement();
                            xml.ReadStartElement("Points");
                            int points = int.Parse(xml.ReadString());
                            xml.ReadEndElement();
                            xml.ReadEndElement(); // Item
                            var item = new Item() { ItemID = itemID, Name = name, Points = points };
                            //items.Add(item);
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
        }*/

        /*private void Imagereq_ResponseReceived(HttpRequest sender, HttpResponse response)
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
        }*/
    }
}
