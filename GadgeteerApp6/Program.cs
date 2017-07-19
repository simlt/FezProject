using System;
using Gadgeteer.Modules.GHIElectronics;
using Gadgeteer.SocketInterfaces;
using GHI.Glide;
using Microsoft.SPOT;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GHI.Pins;
using Microsoft.SPOT.Hardware;
using static Gadgeteer.Modules.GHIElectronics.Button;

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
            END,
        }
        private InterfaceState interState;
        private DigitalOutput ledOut;
        private InterruptPort greenbuttonHW;
        private OutputPort greenLED;

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

            // Initialize breakout board for Lighting
            ledOut = breakout.CreateDigitalOutput(GT.Socket.Pin.Six, false);

            // Initialize interface
            finestra = new interfaccia();
            //button.ButtonPressed += green_button;
            greenbuttonHW = new InterruptPort(FEZSpider.Socket4.Pin3, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);
            greenLED = new OutputPort(FEZSpider.Socket4.Pin4, false);
            greenbuttonHW.OnInterrupt += ButtonHW_OnInterrupt;
            button2.ButtonPressed += red_button;
            game.PictureVerified += finestra.PictureVerified;
            game.GameLoaded += (s) => finestra.item(game.CurrentItem, interState == InterfaceState.ITEM);
            game.GameEnded += () => updateState(InterfaceState.END);
            updateState(InterfaceState.INTRO);
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }

        private void ButtonHW_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            ButtonState state = (data2 == 0) ? ButtonState.Pressed : ButtonState.Released;
            if (state == ButtonState.Pressed)
            {
                // turn button led on
                greenLED.Write(true);
                var timer = new GT.Timer(500, GT.Timer.BehaviorType.RunOnce);
                timer.Tick += (t) => greenLED.Write(false);
                timer.Start();
            }
            green_button(null, state);
            if (greenbuttonHW != null)
            {
                greenbuttonHW.ClearInterrupt();
            }
        }

        void red_button(Gadgeteer.Modules.GHIElectronics.Button sender, Gadgeteer.Modules.GHIElectronics.Button.ButtonState state)
        {
            if (state == GTM.GHIElectronics.Button.ButtonState.Released)
                return;

            Debug.Print("hai premuto il tasto rosso");
            // turn button led on
            sender.TurnLedOn();
            var timer = new GT.Timer(500, GT.Timer.BehaviorType.RunOnce);
            timer.Tick += (t) => sender.TurnLedOff();
            timer.Start();

            if (finestra.msgBoxWindow != null)
            {
                finestra.HideMsgBox();
                return;
            }

            // Cancel last action
            switch (interState)
            {
                case InterfaceState.ACQUIRE:
                    updateState(InterfaceState.STREAM);
                    break;
                case InterfaceState.ITEM:
                    updateState(InterfaceState.END);
                    break;
                case InterfaceState.END:
                    break;
                default:
                    updateState(InterfaceState.ITEM);
                    break;
            }
        }
        void green_button(GTM.GHIElectronics.Button sender, GTM.GHIElectronics.Button.ButtonState state)
        {
            if (state == GTM.GHIElectronics.Button.ButtonState.Released)
                return;

            Debug.Print("hai premuto il tasto verde");

            if (finestra.msgBoxWindow != null)
            {
                finestra.HideMsgBox();
                return;
            }

            switch (interState)
            {
                case InterfaceState.CONFIRMED:
                    if (picture == null)
                        return;
                    break;
                case InterfaceState.END:
                    game.start();
                    interState = InterfaceState.INTRO;
                    break;
                default:
                    ++interState;
                    break;
            }
            updateState(interState);
        }

        private void updateState(InterfaceState newState)
        {
            var oldState = interState;
            interState = newState;
            if (newState != InterfaceState.ACQUIRE)
            {
                camera.PictureCaptured -= camera_PictureCaptured;
            }
            if (newState != InterfaceState.STREAM)
            {
                camera.BitmapStreamed -= camera_StreamCamera;
                try
                {
                    camera.StopStreaming();
                }
                catch { }
            }
            bool ledOn = false;
            switch (newState)
            {
                case InterfaceState.INTRO:
                    finestra.intro();
                    break;
                case InterfaceState.ITEM:
                    finestra.item(game.CurrentItem);
                    break;
                case InterfaceState.STREAM:
                    ledOn = true;
                    camera.BitmapStreamed += camera_StreamCamera;
                    if (camera.CameraReady)
                    {
                        try
                        {
                            //camera.CurrentPictureResolution = Camera.PictureResolution.Resolution176x144;
                            camera.StartStreaming();
                            finestra.acquisition();
                        }
                        catch (ArgumentException) { }
                    }
                    break;
                case InterfaceState.ACQUIRE:
                    camera.PictureCaptured += camera_PictureCaptured;
                    //camera.CurrentPictureResolution = Camera.PictureResolution.Resolution320x240;
                    try
                    {
                        camera.TakePicture();
                    }
                    catch (ArgumentException) { }
                    break;
                case InterfaceState.CONFIRMED:
                    interState = InterfaceState.ITEM;
                    game.submitImage(picture);
                    game.nextItem();
                    finestra.item(game.CurrentItem);
                    break;
                case InterfaceState.END:
                    finestra.endGame(game.CurrentPoints, game.TotalPoints, game.hasGameEnded);
                    break;
            }
            ledOut.Write(ledOn);
        }
        void camera_PictureCaptured(Camera sender, GT.Picture picture)
        {
            this.picture = picture;
            finestra.confirm_img(picture);
        }
        void camera_StreamCamera(Camera sender, Bitmap e)
        {
            if (interState != InterfaceState.STREAM)
                return;
            finestra.acquisitionUpdate(e);
        }
    }
    public class interfaccia
    {
        private GHI.Glide.Display.Window introduction, itempage, getting_picture, check_image, conclusion;
        public GHI.Glide.UI.Image streamImage;
        private GHI.Glide.UI.MessageBox msgBox;
        public GHI.Glide.Display.Window msgBoxWindow { get; set; }

        public interfaccia()
        {
            // Load the window resources at interface creation
            introduction = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.introduzione_iniziale));
            itempage = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.gestione_gioco));
            getting_picture = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.acquisizione_immagine));
            streamImage = (GHI.Glide.UI.Image)getting_picture.GetChildByName("quadrato_img");
            check_image = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.conferma_immagine));
            conclusion = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.conclusion));
            msgBox = new GHI.Glide.UI.MessageBox("msgBox", 255, 0, 0, (int)(Glide.LCD.Width * 0.80), 0);
        }
        public void intro()
        {
            Glide.MainWindow = introduction;
        }
        public void item(Item currentItem, bool refresh = true)
        {
            var text = (GHI.Glide.UI.TextBlock)itempage.GetChildByName("item");
            if (currentItem != null)
            {
                text.Text = currentItem.Name;
                var score = (GHI.Glide.UI.TextBlock)itempage.GetChildByName("score");
                score.Text = currentItem.Points.ToString() + " points";
            }
            else
            {
                text.Text = "Game Loading...";
            }
            if (refresh)
            {
                Glide.MainWindow = itempage;
            }
        }
        public void acquisition()
        {
            Glide.MainWindow = getting_picture;
        }
        public void acquisitionUpdate(Bitmap bmp)
        {
            streamImage.Bitmap = bmp;
            getting_picture.Invalidate();
        }
        public void confirm_img(GT.Picture picture)
        {
            var img = (GHI.Glide.UI.Image)check_image.GetChildByName("img_visualize");
            img.Bitmap = picture.MakeBitmap();
            Glide.MainWindow = check_image;
        }

        public void ShowMsgBox(string title, string message)
        {
            msgBoxWindow = Glide.MainWindow;
            msgBox.Title = title;
            msgBox.Message = message;

            // Determine the number of lines.
            int width = msgBox.Width - 20;
            GHI.Glide.Geom.Size size = FontManager.GetSize(msgBox.MessageFont, message);
            int numLines = (int)System.Math.Ceiling((double)size.Width / width);

            // Set the Height based on the message size.
            int realHeight = msgBox.TitlebarHeight + 10 + (numLines * msgBox.MessageFont.Height) + 15 + 32;
            if (realHeight != msgBox.Height)
                msgBox.Height = realHeight;

            msgBox.X = (Glide.LCD.Width - msgBox.Width) / 2;
            msgBox.Y = (Glide.LCD.Height - msgBox.Height) / 2;
            msgBoxWindow.AddChild(msgBox);
            msgBoxWindow.Invalidate();
        }

        public void HideMsgBox()
        {
            if (msgBoxWindow != null)
            {
                msgBoxWindow.RemoveChild(msgBox);
                msgBoxWindow.Invalidate();
                msgBoxWindow = null;
            }
        }

        internal void PictureVerified(PictureVerifiedEventArgs args)
        {
            string msg = args.Result ?
                args.Name + " found for " + args.Points + " points!" :
                args.Name + " was NOT found!";

            ShowMsgBox("Result", msg);
        }

        internal void endGame(int score, int totalScore, bool allItems)
        {
            var text = (GHI.Glide.UI.TextBlock)conclusion.GetChildByName("score");
            text.Text = score.ToString() + "/" + totalScore.ToString();
            var message = (GHI.Glide.UI.TextBlock)conclusion.GetChildByName("message");
            if (allItems)
            {
                message.Text = "Good job! You found all the objects.";
            }
            else
            {
                message.Text = "Oh... was it too hard? Try again and maybe next time you'll win!";
            }
            Glide.MainWindow = conclusion;
        }
    }
}