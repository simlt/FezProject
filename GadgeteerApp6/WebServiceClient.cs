using Gadgeteer.Networking;
using Microsoft.SPOT;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml;
using GT = Gadgeteer;

namespace GadgeteerApp
{
    // NOTE: The webservice should handle the disconnection of the network and warn the Game somehow
    class WebServiceClient
    {
        private string baseuri;
        private delegate object ResponseHandler(HttpWebResponse resp);
        public delegate void ContentHandler(object obj);
        public delegate void ItemsHandler(ArrayList item);

        // TODO consider using a Queue instead of an ArrayList
        private ArrayList m_requestCache = new ArrayList();
        private Thread m_requestThread;
        private Network.NetworkState m_networkState;
        private AutoResetEvent m_requestARE = new AutoResetEvent(false);

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
            m_requestThread = new Thread(new ThreadStart(processRequests));
            m_requestThread.Start();
        }

        ~WebServiceClient()
        {
            m_requestThread.Abort();
        }

        private void pushRequest(HttpWebRequest req, byte[] body, ResponseHandler handler, ContentHandler cHandler)
        {
            lock (m_requestCache)
            {
                m_requestCache.Add(new RequestEntry { req = req, body = body, handler = handler, cHandler = cHandler });
                m_requestARE.Set();
            }
        }

        private void processRequests()
        {
            while (true)
            {
                Monitor.Enter(m_requestCache);
                try
                {
                    // TODO Questo non funziona, controllare il thread che esegue context
                    while ((m_requestCache.Count > 0) && (m_networkState == Network.NetworkState.Up))
                    {
                        Debug.Print("Network UP: handling queued requests");
                        int requestIndex;
                        // Iterate in reverse order so we can delete elements safely while iterating
                        for (requestIndex = m_requestCache.Count - 1; requestIndex >= 0; --requestIndex)
                        {
                            RequestEntry entry = (RequestEntry)m_requestCache[requestIndex];
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
                            // Handle the content if it is available
                            if (entry.cHandler != null && content != null)
                                entry.cHandler(content);

                            m_requestCache.RemoveAt(requestIndex);
                            Debug.Print("RequestEntry id: " + requestIndex + " was handled correctly");
                        }
                    }
                }
                catch (WebException e)
                {
                    // TODO should probably remove the request 
                    Debug.Print("Request failed while network was up.\nProbably due to timeout or server not being reachable\n" + e.Message);
                }
                finally
                {
                    Monitor.Exit(m_requestCache);
                    // Wait for a new request to be sent
                    m_requestARE.WaitOne();
                } 
            }
        }

        // TODO DEBUG CODE, REMOVE ME
        private void Timer_Tick(GT.Timer timer)
        {
            Network_NetworkStateChange(null, Network.NetworkState.Up);
        }

        private void Network_NetworkStateChange(Network sender, Network.NetworkState status)
        {
            // Lock on m_requestCache. NetworkState is ONLY used in processRequests()
            lock(m_requestCache)
            {
                m_networkState = status;
                string text = m_networkState == Network.NetworkState.Up ? "up" : "down";
                Debug.Print("Network state change: " + text);
                if ((m_networkState == Network.NetworkState.Up) && m_requestCache.Count > 0)
                    m_requestARE.Set();
            }
        }

        // Get the list of items from the Web Service
        private void getItems(ContentHandler itemsHandler)
        {
            try
            {
                Uri uri = new Uri(baseuri + "Items/");
                var itemreq = WebRequest.Create(uri) as HttpWebRequest;
                itemreq.Method = "GET";
                itemreq.Accept = "application/xml";

                pushRequest(itemreq, null, getItemsResponseHandler, itemsHandler);
                Debug.Print("Items GET request prepared...");
            }
            catch (WebException)
            {
                Debug.Print("Items Request timed out");
            }
        }

        private ArrayList getItemsResponseHandler(HttpWebResponse response)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Debug.Print("Items Request success");
                try
                {
                    ArrayList items = new ArrayList();
                    using (XmlReader xml = XmlReader.Create(response.GetResponseStream()))
                    {
                        while (xml.ReadToFollowing("Item"))
                        {
                            var item = new Item(xml);
                            items.Add(item);
                        }
                    }
                    return items;
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
            return null;
        }

        // Send an image to be labeled by the WebService
        public void submitImage(int gameId, int itemId, GT.Picture image, ContentHandler pictureHandler)
        {
            if (image.Encoding != Gadgeteer.Picture.PictureEncoding.BMP)
                throw new ArgumentException("Unsupported image encoding used for submit");


            try
            {
                byte[] imagedata = image.PictureData;
                Uri uri = new Uri(baseuri + "Games/" + gameId + "/Items/" + itemId);
                var req = WebRequest.Create(uri) as HttpWebRequest;
                //req.Timeout = Timeout.Infinite;
                req.Method = "POST";
                req.Accept = "application/xml";
                req.ContentType = "image/bmp";
                req.ContentLength = imagedata.Length;
                //req.SendChunked = true;
                pushRequest(req, imagedata, submitImageResponseHandler, pictureHandler);
                Debug.Print("Image POST request prepared...");
            }
            // TODO rivedere gestione eccezioni!
            catch (WebException e)
            {
                Debug.Print("Image POST request failed with exception:\n" + e.Message);
                return;
            }
        }

        private ImageSubmission submitImageResponseHandler(HttpWebResponse response)
        {
            try
            {
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Debug.Print("Image POST success");
                    using (Stream stream = response.GetResponseStream())
                    {
                        return new ImageSubmission(stream);
                    }
                }
                else
                {
                    //Show a helpful error message
                    Debug.Print("Image POST failed with status code " + response.StatusCode);
                    Debug.Print("Response text: " + response.StatusDescription);
                }
            }
            catch (Exception e)
            {
                Debug.Print("Image response parsing failed with exception:\n" + e.Message);
            }
            return null;
        }

        public void createGame(ContentHandler gameRequestHandler)
        {
            try
            {
                Uri uri = new Uri(baseuri + "Games/");
                var req = WebRequest.Create(uri) as HttpWebRequest;
                req.Method = "POST";
                req.Accept = "application/xml";
                req.ContentLength = 0;
                pushRequest(req, null, createGameResponseHandler, gameRequestHandler);
                Debug.Print("Game POST request prepared...");
            }
            catch (WebException e)
            {
                Debug.Print("Game POST request failed with exception:\n" + e.Message);
                return;
            }

            // TODO DEBUG CODE, REMOVE ME
            Network_NetworkStateChange(null, Network.NetworkState.Down);
            var timer = new GT.Timer(20 * 1000);
            timer.Tick += Timer_Tick;
            timer.Behavior = GT.Timer.BehaviorType.RunOnce;
            timer.Start();
        }

        private GameSession createGameResponseHandler(HttpWebResponse response)
        {
            try
            {
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Debug.Print("Game POST success");
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (var xml = XmlReader.Create(stream))
                        {
                            var gamereq = new GameSession(xml);
                            return gamereq;
                        }
                    }
                }
                else
                {
                    //Show a helpful error message
                    Debug.Print("Image POST failed with status code " + response.StatusCode);
                    Debug.Print("Response text: " + response.StatusDescription);
                }
            }
            catch (Exception e)
            {
                Debug.Print("Game response parsing failed with exception:\n" + e.Message);
            }
            return null;
        }
    }
}
