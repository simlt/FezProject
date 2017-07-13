using System;
using Gadgeteer.Modules.GHIElectronics;
using GHI.Glide;
using Microsoft.SPOT;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace GadgeteerApp
{
    public partial class Program
    {
        private Game game;
        private GT.Picture picture;
        private interfaccia finestra;

        enum InterfaceState
        {
            INTRO,
            ITEM,
            STREAM,
            ACQUIRE,
            CONFIRMED,
        }
        private InterfaceState interState;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            // Setup network
            var network = new Network();
            network.Setup(ethernet);

            // Create new WebServiceClient
            var client = new WebServiceClient("192.168.10.1:8080", network);

            // Create (and start) a Game
            game = new Game(client);

            // Initialize interface
            finestra = new interfaccia();

            button.ButtonPressed += green_button;
            button2.ButtonPressed += red_button;
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
            updateInterfaceWithState(InterfaceState.INTRO);
        }
        void green_button(GTM.GHIElectronics.Button sender, GTM.GHIElectronics.Button.ButtonState state)
        {
            if (state == Button.ButtonState.Released)
                return;
            Debug.Print("hai premuto il tasto verde");

            if (interState == InterfaceState.CONFIRMED)
            {
                interState = InterfaceState.ITEM;
            }
            else
            {
                interState++;
            }
            Debug.Print("sono nel caso " + interState + " di window");
            updateInterface();
        }

        private void updateInterface()
        {
            updateInterfaceWithState(interState);
        }

        private void updateInterfaceWithState(InterfaceState state)
        {
            interState = state;
            camera.PictureCaptured -= camera_PictureCaptured;
            camera.BitmapStreamed -= camera_StreamCamera;
            switch (interState)
            {
                case InterfaceState.INTRO:
                    finestra.intro();
                    break;
                case InterfaceState.ITEM:
                    finestra.item(game.CurrentItem);
                    break;
                case InterfaceState.STREAM:
                    //camera.BitmapStreamed(new Bitmap(camera.CurrentPictureResolution.Width,camera.CurrentPictureResolution.Height));
                    stream_img();
                    break;
                case InterfaceState.ACQUIRE:
                    takePicture();
                    break;
                case InterfaceState.CONFIRMED:
                    game.submitImage(picture);
                    break;
            }
        }

        void red_button(Gadgeteer.Modules.GHIElectronics.Button sender, Gadgeteer.Modules.GHIElectronics.Button.ButtonState state)
        {
            // Cancel last action
            if (interState == InterfaceState.ACQUIRE)
            {
                updateInterfaceWithState(InterfaceState.STREAM);
            }
            else
            {
                updateInterfaceWithState(InterfaceState.INTRO);
            }
        }
        void takePicture()
        {
            camera.StopStreaming();
            camera.PictureCaptured += camera_PictureCaptured;
            camera.TakePicture();
        }
        void camera_PictureCaptured(Camera sender, GT.Picture picture)
        {
            this.picture = picture;
            finestra.confirm_img(picture);
        }
        void stream_img()
        {
            camera.StopStreaming();
            camera.BitmapStreamed += camera_StreamCamera;
            camera.StartStreaming();
        }
        void camera_StreamCamera(Camera sender, Bitmap e)
        {
            finestra.acquisition(e);
        }
    }
    public class interfaccia
    {
        private GHI.Glide.Display.Window introduction, itempage, getting_picture, check_image;
        public interfaccia()
        {
            // Load the window resources at interface creation
            introduction = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.introduzione_iniziale));
            itempage = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.gestione_gioco));
            getting_picture = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.acquisizione_immagine));
            check_image = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.conferma_immagine));
        }
        public void intro()
        {
            Glide.MainWindow = introduction;
        }
        public void item(Item currentItem)
        {
            var text = (GHI.Glide.UI.TextBlock)itempage.GetChildByName("instancegestione");
            if (currentItem != null)
                text.Text = currentItem.Name;
            else
                text.Text = "ITEM_NOT_FOUND";

            Glide.MainWindow = itempage;
        }
        public void acquisition(Bitmap bmp)
        {
            //GHI.Glide.UI.ProgressBar w_for_pick = (GHI.Glide.UI.ProgressBar)getting_picture.GetChildByName("pr_b");
            //w_for_pick.Value=10;
            var streamed_img = (GHI.Glide.UI.Image)getting_picture.GetChildByName("quadrato_img");
            streamed_img.Bitmap = bmp;
            Glide.MainWindow = getting_picture;
        }
        public void confirm_img(GT.Picture picture)
        {
            var img = (GHI.Glide.UI.Image)check_image.GetChildByName("img_visualize");
            img.Bitmap = picture.MakeBitmap();
            Glide.MainWindow = check_image;
        }
    }
}