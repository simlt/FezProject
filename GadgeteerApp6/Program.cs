using Gadgeteer.Modules.GHIElectronics;
//using GHI.Glide.UI;
using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using GHI.Glide;
using GHI.Glide.Display;
using GHI.Glide.UI;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;

namespace GadgeteerApp
{
    public partial class Program
    {
        public int st;
        public int start;
        public int scelta;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            st = 1;
            scelta = 1;
            start = 0;
            Debug.Print("verifica0");
            camera.PictureCaptured += new Camera.PictureCapturedEventHandler(camera_PictureCaptured);
            button.ButtonPressed += green_button;
            button.ButtonPressed += red_button;
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
            interfaccia i = new interfaccia();
            i.intro();
            //i.da_definire();
            //interfaccia4 iv = new interfaccia4();
        }
        void green_button(GTM.GHIElectronics.Button sender, GTM.GHIElectronics.Button.ButtonState state)
        {
            Debug.Print("hai premuto il tasto verde");
            switch (scelta)
            {
                case 2:
                    st = 3;
                    break;
                case 3:
                    st = 4;
                    break;
                case 4:
                    st = 5;
                    //SelectionChangedEventHandler.Remove(button_ButtonPressed, pulsante_1);
                    //button.ButtonPressed += new Gadgeteer.Modules.GHIElectronics.Button.ButtonEventHandler(green_button);
                    break;
                case 5:

                    break;
                default:
                        st = 2;
                    break;
            }
            window();
        }
        void window()
        {
            Debug.Print("hai richiamato window");   
            interfaccia finestra = new interfaccia();
            scelta = st;
            switch (st)
            {
                case 2:
                    Debug.Print("sono nel caso 2 di window");   
                    finestra.da_definire();
                    break;
                case 3:      // da finire
                    Debug.Print("sono nel caso 3 di window");
                    GT.Picture roba;

                    //camera.BitmapStreamed(new Bitmap(camera.CurrentPictureResolution.Width,camera.CurrentPictureResolution.Height));
                                                          
                        
                    //finestra.acquisition();
                    break;
                case 4:
                    Debug.Print("sono nel caso 4 di window");   
                    camera.TakePicture();
                    //SelectionChangedEventHandler.Remove(button_ButtonPressed, pulsante_1);
                    //button.ButtonPressed += new Gadgeteer.Modules.GHIElectronics.Button.ButtonEventHandler(green_button);
                    break;
                case 5:
                    break;
                default:
                    finestra.intro();
                    scelta = 2;
                    break;
            }
        }

        void red_button(Gadgeteer.Modules.GHIElectronics.Button sender, Gadgeteer.Modules.GHIElectronics.Button.ButtonState state)
        {
            camera.TakePicture();
            st = 1;
        }
        void camera_PictureCaptured(Camera sender, GT.Picture immagine)
        {
            //displayT35.SimpleGraphics.DisplayImage(immagine, 5, 5);
            Bitmap bmp = immagine.MakeBitmap();
            interfaccia iv = new interfaccia();
            //bmp.RotateImage(180, 0, 0, iv.confirm_img(bmp);
            iv.confirm_img(bmp);
            //button.ButtonPressed -= button_ButtonPressed;
            //button.ButtonPressed += new Gadgeteer.Modules.GHIElectronics.Button.ButtonEventHandler(pulsante_1);
        }
        void stream_img()
        {
            Bitmap  img = new Bitmap(250, 191);  ////// da finire
            camera.StartStreaming();
            interfaccia iii = new interfaccia();
           // camera.StartStreaming(stream_img);
        }
    }
    public class interfaccia
    {
        public void intro()
        {
            GHI.Glide.Display.Window introduction1 = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.introduzione_iniziale));
            Glide.MainWindow = introduction1;
        }
        public void da_definire()
        {
            GHI.Glide.Display.Window pagina2 = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.gestione_gioco));
            Glide.MainWindow = pagina2;
        }
        public void acquisition()
        {
            GHI.Glide.Display.Window getting_picture = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.acquisizione_immagine));
            GHI.Glide.UI.ProgressBar w_for_pick = (GHI.Glide.UI.ProgressBar)getting_picture.GetChildByName("pr_b");
            w_for_pick.Value=10;
            GHI.Glide.UI.Image streamed_img = (GHI.Glide.UI.Image)getting_picture.GetChildByName("quadrato_img");
            Glide.MainWindow = getting_picture;
        }
        public void confirm_img(Bitmap a)
        {
            GHI.Glide.Display.Window check_image = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.conferma_immagine));
            //GlideTouch.Initialize();
            GHI.Glide.UI.Image img = (GHI.Glide.UI.Image)check_image.GetChildByName("img_visualize");
            Debug.Print("ciaooooooooooooooooooooooooooooooooooooooooooooooo");
            img.Bitmap = a;
            Glide.MainWindow = check_image;
        }
    }

}

    /*
    }
    public class interfaccia2
    {
        
    }
    public class interfaccia3
    {
        public void acquisition()
        {
            GHI.Glide.Display.Window get_picture = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.acquisizione_immagine));
            Glide.MainWindow = get_picture;
        }
    }
    public class interfaccia4
    {
        public void confirm_img(Bitmap a){
            GHI.Glide.Display.Window check_image = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.conferma_immagine));
            //GlideTouch.Initialize();
            GHI.Glide.UI.Image img = (GHI.Glide.UI.Image)check_image.GetChildByName("img_visualize");
            Debug.Print("ciaooooooooooooooooooooooooooooooooooooooooooooooo");
            img.Bitmap = a;
            Glide.MainWindow = check_image;
        }      
    }    */



/*

            switch (stato)
                {
                default:
                        //inizio.intro();
                        stato = 1;
                        button.ButtonPressed += new Gadgeteer.Modules.GHIElectronics.Button.ButtonEventHandler(pulsante_1);
                        break;
                case 3:
                       
                        break;
                case 4:
                        //SelectionChangedEventHandler.Remove(button_ButtonPressed, pulsante_1);
                        button.ButtonPressed += new Gadgeteer.Modules.GHIElectronics.Button.ButtonEventHandler(button_ButtonPressed);
                        break;
                }*/
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

            // Setup network
            var network = new Network();
            network.Setup(ethernet);

            // Create new WebServiceClient
            client = new WebServiceClient("192.168.10.1:8080", network);

            // Create (and start) a Game
            game = new Game(client);


            // Test submit for "watch" item
            game.submitImage(immagine);

/*    void green_button(Gadgeteer.Modules.GHIElectronics.Button sender, Gadgeteer.Modules.GHIElectronics.Button.ButtonState state)
    {
            if (state == Button.ButtonState.Pressed)
            {
                if (camera.CameraReady)
                {
                    Debug.Print("Picture taken.");
        camera.TakePicture();
        interfaccia i = new interfaccia();
        //Glide.MainWindow = i.in

    }*/