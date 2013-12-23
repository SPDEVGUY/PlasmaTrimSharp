using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlasmaSharpDriver;
using System.Threading;

namespace PlasmaTrimSharp
{
    class Program
    {
        //http://www.thephotonfactory.com/forum/viewtopic.php?f=5&t=104&p=169#p168
        //
        static void Main(string[] args)
        {
            var trims = PlasmaTrimController.GetPlasmatrims();
            if (trims.Count > 0)
            {
                using (var trim0 = trims[0])
                {
                    if (trim0.Open())
                    {
                        StopAnimating(trim0);
                        ColourChangeExample(trim0);
                        SetColourExample(trim0);
                        BlackoutExample(trim0);

                        StartAnimating(trim0);

                    }
                    else
                    {
                        Console.WriteLine("Failed to open.");
                    }
                }
            }
            else
            {
                Console.WriteLine("No plasmatrims detected.");
            }

            Console.WriteLine("done.");
            Console.ReadLine();
        }
        

        static void StopAnimating(PlasmaTrimController trim)
        {
            Console.WriteLine("Stop playing: {0}", trim.StopPlayingSequence()); 

            Console.ReadLine();
        }
        static void StartAnimating(PlasmaTrimController trim)
        {
            Console.WriteLine("Start playing: {0}", trim.StartPlayingSequence()); 

            Console.ReadLine();
        }
        static void ColourChangeExample(PlasmaTrimController trim)
        {
            Console.WriteLine("Set RGB4, transition from green to red.");

            var state = new PlasmaTrimState
            {   Brightness = 255  };

            for (byte i = 0; i < 255; i++)
            {
                state.R4 = i;
                state.G4 = (byte)(255 - i);
                trim.SetImmediateState(state);
                Thread.Sleep(2); //wait 2 ms before advancing colour
            }

            Console.ReadLine();
        }

        static void SetColourExample(PlasmaTrimController trim)
        {
            var state = new PlasmaTrimState {Brightness = 170, B4 = 255};

            Console.WriteLine("Set RGB4, to BLUE: {0}", 
                trim.SetImmediateState(state)
                ); 
            Console.ReadLine();
        }

        static void BlackoutExample(PlasmaTrimController trim)
        {
            var blackState = new PlasmaTrimState() { Brightness = 170}; //Retaining brightness as it affects sequence playback..
            Console.WriteLine("Blackout: {0}", trim.SetImmediateState(blackState)); Console.ReadLine();
        }
    }
}
