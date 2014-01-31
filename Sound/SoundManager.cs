using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using System.IO;

namespace RacingGame.Sound
{
    /// <summary>
    /// Singleton used to handle the sound and music in the game
    /// Author: David Sundelius
    /// </summary>
    
    class SoundManager
    {
        private static SoundManager instance = new SoundManager();

        private AudioEngine audioEngine;
        private WaveBank waveBank;
        private SoundBank soundBank;

        private SoundManager()
        {
            try
            {
                string dir = "Content\\Sound";
                audioEngine = new AudioEngine(Path.Combine(dir, "GameAudio.xgs"));
                waveBank = new WaveBank(audioEngine, Path.Combine(dir, "Wave Bank.xwb"));
                soundBank = new SoundBank(audioEngine, Path.Combine(dir, "Sound Bank.xsb"));
            }
            catch (Exception e)
            {
                Sys.Logger.getInstance().print("SoundManager threw exception during initialization" + e.Message);
            }
        }

        public static SoundManager getInstance() {
            return instance;
        }

        public void playSound(string soundName)
        {
            soundBank.PlayCue(soundName);
        }

        public void stopAll()
        {
            AudioCategory c = audioEngine.GetCategory("Default");
            c.Stop(AudioStopOptions.AsAuthored);  
        }
    }
}
