using System.IO;
using System.Xml;

namespace GadgeteerApp
{
    internal class ImageSubmission
    {
        public int ImageID { get; set; }
        public int ItemID { get; set; }
        public bool VerificationResult { get; set; }

        internal ImageSubmission(Stream xmlstream)
        {
            using (XmlReader xml = XmlReader.Create(xmlstream))
            {
                /* Read a single ImageSubmission (Example)
                <ImageSubmission>
                    <ImageID>15</ImageID>
                    <ItemID>1</ItemID>
                    <Labels>
                        <d2p1:string>label</string>
                    </Labels>
                    <VerificationResult>false</VerificationResult>
                </ImageSubmission>
                */
                xml.ReadStartElement("ImageSubmission");
                xml.ReadStartElement("ImageID");
                ImageID = int.Parse(xml.ReadString());
                xml.ReadEndElement();
                xml.ReadStartElement("ItemID");
                ItemID = int.Parse(xml.ReadString());
                xml.ReadEndElement();
                /*xml.ReadStartElement("Labels");
                // Labels....
                xml.ReadEndElement();
                xml.ReadStartElement("VerificationResult");*/
                xml.ReadToFollowing("VerificationResult");
                //xml.ReadStartElement("VerificationResult");
                VerificationResult = xml.ReadString() == "true";
                xml.ReadEndElement();
                xml.ReadEndElement(); // ImageSubmission
            }
        }
    }
}