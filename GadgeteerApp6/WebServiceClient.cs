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
        private Network.NetworkState NetworkState { get; set; }

        // TODO consider using a Queue instead of an ArrayList
        private ArrayList requestCache;
        private delegate object ResponseHandler(HttpWebResponse resp);
        public delegate void ContentHandler(object obj);
        public delegate void ItemsHandler(ArrayList item);

        private struct RequestEntry
        {
            internal HttpWebRequest req;
            internal byte[] body;
            internal ResponseHandler handler;
            internal ContentHandler cHandler;
        }

        public WebServiceClient(string address, Network network)
        {
            baseuri = "http://" + address + "/api/";
            network.NetworkStateChange += Network_NetworkStateChange;
            requestCache = new ArrayList();
        }

        // TODO DEBUG CODE, REMOVE ME
        private void Timer_Tick(GT.Timer timer)
        {
            Network_NetworkStateChange((Network)null, Network.NetworkState.Up);
        }

        private void Network_NetworkStateChange(Network sender, Network.NetworkState status)
        {
            NetworkState = status;
            if (NetworkState == Network.NetworkState.Up)
            {
                sendRequests();
            }
        }

        private void pushRequest(HttpWebRequest req, byte[] body, ResponseHandler handler, ContentHandler cHandler)
        {
            requestCache.Add(new RequestEntry { req = req, body = body, handler = handler, cHandler = cHandler });
            sendRequests();
        }

        private void sendRequests()
        {
            int requestIndex;
            try
            {
                if (NetworkState == Network.NetworkState.Up)
                {
                    Debug.Print("Network UP: handling queued requests");
                    // Iterate in reverse order so we can delete elements safely while iterating
                    for (requestIndex = requestCache.Count - 1; requestIndex >= 0; --requestIndex)
                    {
                        RequestEntry entry = (RequestEntry)requestCache[requestIndex];
                        object content;
                        // Fill body if present (implicitly starts a connection)
                        if (entry.body != null)
                        {
                            // GetRequestStream: Submits a request with HTTP headers to the server, and returns a Stream object to use to write request data.
                            using (Stream stream = entry.req.GetRequestStream())
                            {
                                stream.Write(entry.body, 0, entry.body.Length);
                            }
                        }
                        using (var resp = entry.req.GetResponse() as HttpWebResponse)
                        {
                            content = entry.handler(resp);
                        }
                        // Handle the content if it is possible
                        if (entry.cHandler != null && content != null)
                            entry.cHandler(content);

                        requestCache.RemoveAt(requestIndex);
                        Debug.Print("RequestEntry id: " + requestIndex + " was handled correctly");
                    }
                }
            }
            catch (WebException e)
            {
                // TODO should probably remove the request 
                Debug.Print("Request failed while network was up.\nProbably due to timeout or server not being reachable\n" + e.Message);
            }
        }

        // Get the list of items from the Web Service
        public void getItems(ContentHandler itemsHandler)
        {
            try
            {
                Uri uri = new Uri(baseuri + "Items/");
                var itemreq = WebRequest.Create(uri) as HttpWebRequest;
                itemreq.Method = "GET";
                itemreq.Accept = "application/xml";

                Debug.Print("Sending items GET request...");
                pushRequest(itemreq, null, getItemsResponseHandler, itemsHandler);
            }
            catch (WebException)
            {
                Debug.Print("Items Request timed out");
            }

            // TODO DEBUG CODE, REMOVE ME
            Network_NetworkStateChange((Network)null, Network.NetworkState.Down);
            var timer = new Gadgeteer.Timer(20 * 1000);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private ArrayList getItemsResponseHandler(HttpWebResponse response)
        {
            ArrayList items = new ArrayList();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Debug.Print("Items Request succeeded");
                try
                {
                    using (XmlReader xml = XmlReader.Create(response.GetResponseStream()))
                    {
                        while (xml.ReadToFollowing("Item"))
                        {
                            xml.Read(); // Item
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
                catch (XmlException e)
                {
                    Debug.Print("XML parsing failed with exception:\n" + e.Message);
                }
            }
            else
            {
                //Show a helpful error message
                Debug.Print("Items Request failed with status code " + response.StatusCode + ": " + response.StatusDescription);
            }

            // Handle content with delegate passed from user
            return items;
        }

        // Send an image to be labeled by the WebService
        public void submitImage(int itemId, GT.Picture image, ContentHandler pictureHandler)
        {
            if (image.Encoding != Gadgeteer.Picture.PictureEncoding.BMP)
                throw new ArgumentException("Unsupported image encoding used for submit");

            HttpWebRequest req = null;
            byte[] imagedata = image.PictureData;

            try
            {
                Uri uri = new Uri(baseuri + "Items/" + itemId + "/Submit/");
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
            }
            // TODO rivedere gestione eccezioni!
            catch (WebException e)
            {
                Debug.Print("Image POST request failed with exception:\n" + e.Message);
                return;
            }
            Debug.Print("Image POST request prepared...");
            pushRequest(req, imagedata, submitImageResponseHandler, pictureHandler);
        }

        private ImageSubmission submitImageResponseHandler(HttpWebResponse response)
        {
            ImageSubmission imagesub = null;
            try
            {
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Debug.Print("Image POST succeeded");
                    using (Stream stream = response.GetResponseStream())
                    {
                        imagesub = new ImageSubmission(stream);

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
            return imagesub;
        }
    }
}
