using Gadgeteer.Modules.GHIElectronics;
//using GHI.Glide.UI;
using Microsoft.SPOT;
using GT = Gadgeteer;

namespace GadgeteerApp
{
    public partial class Program
    {
        WebServiceClient client = null;
        // Window window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.Window));

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/
            button.ButtonPressed += new Button.ButtonEventHandler(button_ButtonPressed);
            camera.PictureCaptured += new Camera.PictureCapturedEventHandler(camera_PictureCaptured);

            // Setup network
            var network = new Network();
            network.Setup(ethernet);

            // Create new WebServiceClient
            client = new WebServiceClient("192.168.10.1:8080", network);

            // Create (and start) a Game
            var game = new Game(client);

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }

        void camera_PictureCaptured(Camera sender, GT.Picture immagine)
        {
            displayT35.SimpleGraphics.DisplayImage(immagine, 0, 0);
            // Test submit for "watch" item
            client.submitImage(4, immagine);
        }

        void button_ButtonPressed(Button sender, Button.ButtonState state)
        {
            if (state == Button.ButtonState.Pressed)
            {
                if (camera.CameraReady)
                {
                    camera.TakePicture();
                    Debug.Print("Picture taken.");
                }
                else
                {
                    Debug.Print("Camera not ready.");
                }
            }
        }
    }
}
