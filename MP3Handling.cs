using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio;

namespace MusicPlayerAttempt
{
    class MP3Handling
    {

        public IWavePlayer waveOutDevice;
        public WaveStream mainOuputStream;
        public WaveChannel32 volumeStream;

        public string selectedSong = "";

        private WaveStream CreateInputStream(string fileName)
        {
            WaveChannel32 inputstream;
            if (fileName.EndsWith(".mp3"))
            {
                WaveStream mp3Reader = new Mp3FileReader(fileName);
                inputstream = new WaveChannel32(mp3Reader);
            }
            else
            {
                throw new InvalidOperationException("Unsupported extension");
            }
            volumeStream = inputstream;
            return volumeStream;
        }

        public void Setup()
        {
            waveOutDevice = new WaveOut();
            mainOuputStream = CreateInputStream(selectedSong);
        }

        public void initialize()
        {
            waveOutDevice.Init(mainOuputStream);
            waveOutDevice.Play();
        }

        public void Play()
        {
            Setup();
            initialize();
        }

        public void PauseIt()
        {
            waveOutDevice.Pause();
        }

        public void OnResume()
        {
            waveOutDevice.Play();
        }

        public void StopPlayback()
        {
            waveOutDevice.Stop();
        }

        public void backToStart()
        {
            CloseWaveOut();
            Play();
        }

        public void CloseWaveOut()
        {
            waveOutDevice.Stop();
            volumeStream.Close();
            mainOuputStream.Close();
        }

        public void visulisation()
        {
            
        }

    }
}
