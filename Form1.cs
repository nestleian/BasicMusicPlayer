using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Web;
using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.IO;
using TagLib.Id3v2;


// Need to add support for adding single files to playlist***(Done)*** 
//Need to fix play previous song button and straight up play button. ***(Done)***
//Fix clear playlist button. ***(Done)***
//Fix adding more than one album to add duplicates of all previous ones. ***(Done)***
//Fix shuffle playlist button. ***(DONE!!!)***
//Fix error while clearing a playlist with no song playing.***(Done)***
//Selected index won't play while another song is playing upon hitting play button. ***(Done)***
//Try to implement album art ***(Done)!***
//Now need to make album art change depending on song. Add album art path to playlistitems?***(Done)***
//Implement last.fm API to grab album art that isn't on the computer.
//Look for bugs.

namespace MusicPlayerAttempt
{
    public partial class Form1 : Form
    {
        MP3Handling mp3Handler = new MP3Handling();
        List<PlaylistItem> playlist = new List<PlaylistItem>();

        string paused = "";
        List<string> songPathsInFolder = new List<string>();
        List<string> albumArtInFolder = new List<string>();

        int currentTrack;
        int currentlyPlayingTrackNumber;

        int artStart = 0;
        int track;
        int seconds;
        int minutes;

        string apiKey = "23506d22c8a556c16abaac591652d113";

        public Form1()
        {
            InitializeComponent();
        }

        private void songSelectButton_Click(object sender, EventArgs e)
        {
            if (songSelectDialog.ShowDialog() == DialogResult.OK)
            {
                addAlbumArtToSongInfo(false);
            }
        }

        private void folderBrowseButton_Click(object sender, EventArgs e)
        {
            if (folderMusicBrowser.ShowDialog() == DialogResult.OK)
            {
                songPathsInFolder = Directory.GetFiles(folderMusicBrowser.SelectedPath, "*.mp3").ToList<string>();
                albumArtInFolder = Directory.GetFiles(folderMusicBrowser.SelectedPath, "*.jpg").ToList<string>();
                AddFolderToPlaylist();
                addAlbumArtToSongInfo(true);
                songPathsInFolder.Clear();
            }

        }

        private void playButton_Click(object sender, EventArgs e)
        {
            if (mp3Handler.selectedSong == "" && playlistBox.SelectedIndex == -1)
            {
                MessageBox.Show("You need to select a song first, idiot.");
            }
            else
            {
                if (paused == "")
                {
                    makeLabelsVisible();
                    if (playlistBox.SelectedIndex != -1)
                    {
                        playSelectedSong();
                        secTimer.Start();
                        getTrackNumber();
                    }
                    else
                    {
                        mp3Handler.Play();
                        secTimer.Start();
                        getTrackNumber();
                    }
                    setAlbumArtInPictureBox();
                }
                else
                {
                    mp3Handler.OnResume();
                    paused = "";
                    secTimer.Start();
                    getTrackNumber();
                }
            }
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            mp3Handler.PauseIt();
            paused = "Paused";
            secTimer.Stop();
        }

        private void skipButton_Click(object sender, EventArgs e)
        {
            if (playlist.Count > 0 && currentlyPlayingTrackNumber < playlist.Count)
            {
                playNextSongInPlaylist();
            }
            getTrackNumber();
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            if (minutes == 0 && seconds <= 3 && track > 0)
            {
                playPreviousSongInPlaylist();
            }
            else
            {
                resetTimer();
                mp3Handler.backToStart();
            }
            getTrackNumber();
        }

        private void stopPlaybackButton_Click(object sender, EventArgs e)
        {
            mp3Handler.StopPlayback();
            secTimer.Stop();
            seconds = 0;
            minutes = 0;
        }

        private void shuffleSongsButton_Click(object sender, EventArgs e)
        {
            track = 0;
            playlistBox.Items.Clear();
            List<PlaylistItem> randomizedList = new List<PlaylistItem>();
            Random rnd = new Random();
            while (playlist.Count > 0)
            {
                int index = rnd.Next(0, playlist.Count);
                randomizedList.Add(playlist[index]);
                playlist.RemoveAt(index);
            }
            int startPoint = track;
            foreach (PlaylistItem song in randomizedList)
            {
                song.TrackNumber = track;
                playlist.Add(song);
                track++;
            }
            int endPoint = track;
            for (int i = startPoint; i < endPoint; i++)
            {
                playlistBox.Items.Add(playlist[i].Song + " - " + playlist[i].Artist + " - " + playlist[i].Album);
            }

        }

        private void secTimer_Tick(object sender, EventArgs e)
        {
            seconds++;
            if (seconds > 59)
            {
                minutes++;
                seconds = 0;
            }
            if (seconds < 10)
            {
                timeLeftLabel.Text = minutes.ToString() + ":0" + seconds.ToString();
            }
            else
            {
                timeLeftLabel.Text = minutes.ToString() + ":" + seconds.ToString();
            }

            if (mp3Handler.volumeStream.Position >= mp3Handler.volumeStream.Length &&
                playlist.Count > 0)
            {
                getTrackNumber();
                if (currentlyPlayingTrackNumber < playlist.Count)
                {
                    playNextSongInPlaylist();
                    resetTimer();
                }
            }
        }

        private void AddSongToPlaylist(string albumArt)
        {
            string song = songSelectDialog.FileName;
            int amount = playlist.Count;
            TagLib.File file = TagLib.File.Create(song);
            PlaylistItem newSong = new PlaylistItem
            {
                Artist = file.Tag.FirstArtist,
                Song = file.Tag.Title,
                Album = file.Tag.Album,
                Year = file.Tag.Year.ToString(),
                SongFile = song,
                TrackNumber = playlist[amount - 1].TrackNumber + 1,
                artPath = albumArt
            };
            track = track + 1;
            playlist.Add(newSong);
            playlistBox.Items.Add(newSong.Song + " - " + newSong.Artist + " - " + newSong.Album);
        }

        private void AddFolderToPlaylist()
        {
            int startPoint = track;
            TagLib.File file = null;
            foreach (String song in songPathsInFolder)
            {
                file = TagLib.File.Create(song);
                PlaylistItem newSong = new PlaylistItem
                {
                    Artist = file.Tag.FirstArtist,
                    Song = file.Tag.Title,
                    Album = file.Tag.Album,
                    Year = file.Tag.Year.ToString(),
                    SongFile = song,
                    TrackNumber = track
                };
                playlist.Add(newSong);
                track++;
            }
            int endPoint = track;
            for (int i = startPoint; i < endPoint; i++)
            {
                playlistBox.Items.Add(playlist[i].Song + " - " + playlist[i].Artist + " - " + playlist[i].Album);
            }
            songPathsInFolder.Clear();
        }

        private void playlistBox_MouseDoubleClick(object sender, EventArgs e)
        {
            if (playlistBox.SelectedItem != null)
            {
                int first;
                string songfromlist = playlistBox.SelectedItem.ToString();
                first = songfromlist.IndexOf("-");
                string songname = songfromlist.Substring(0, first - 1);
                int songOccurance = playlist.FindIndex(
                    delegate(PlaylistItem track)
                    {
                        return track.Song == songname;
                    });
                if (mp3Handler.selectedSong != "")
                {
                    mp3Handler.StopPlayback();
                    mp3Handler.CloseWaveOut();
                }
                mp3Handler.selectedSong = playlist[songOccurance].SongFile;
                currentlyPlayingTrackNumber = playlist[songOccurance].TrackNumber;
                setLabels(currentlyPlayingTrackNumber);
                getTrackNumber();
                resetTimer();
                mp3Handler.Play();
                makeLabelsVisible();
                setAlbumArtInPictureBox();
                first = 0;
                songname = "";
                songOccurance = 0;
                
            }
        }

        private void getTrackNumber()
        {
            currentTrack =
                playlist.FindIndex(
                delegate(PlaylistItem track)
                {
                    return track.SongFile == mp3Handler.selectedSong;
                });
            currentlyPlayingTrackNumber = playlist[currentTrack].TrackNumber;
        }

        private void playNextSongInPlaylist()
        {
            int nextTrack = currentlyPlayingTrackNumber + 1;
            int indexOfNextTrack =
                playlist.FindIndex(
                delegate(PlaylistItem track)
                {
                    return track.TrackNumber == nextTrack;
                });
            if (mp3Handler.selectedSong != null)
                completeStop();
            currentlyPlayingTrackNumber = nextTrack;
            mp3Handler.selectedSong = playlist[currentlyPlayingTrackNumber].SongFile;
            setLabels(currentlyPlayingTrackNumber);
            setAlbumArtInPictureBox();
            resetTimer();
            makeLabelsVisible();
            mp3Handler.Play();

        }

        private void playSelectedSong()
        {
            if (playlistBox.SelectedIndex != -1)
            {
                if (mp3Handler.selectedSong != "")
                {
                    completeStop();
                }
                currentlyPlayingTrackNumber = playlist[playlistBox.SelectedIndex].TrackNumber;
                mp3Handler.selectedSong = playlist[playlistBox.SelectedIndex].SongFile;
                currentlyPlayingTrackNumber = playlist[playlistBox.SelectedIndex].TrackNumber;
                setLabels(playlistBox.SelectedIndex);
                makeLabelsVisible();
                setAlbumArtInPictureBox();
                resetTimer();
                mp3Handler.Play();
            }
        }

        private void playPreviousSongInPlaylist()
        {
            int previousTrack = currentlyPlayingTrackNumber - 1;
            int indexOfPreviousTrack =
                playlist.FindIndex(
                delegate(PlaylistItem track)
                {
                    return track.TrackNumber == previousTrack;
                });
            if (mp3Handler.selectedSong != "")
                completeStop();
            currentlyPlayingTrackNumber = previousTrack;
            mp3Handler.selectedSong = playlist[currentlyPlayingTrackNumber].SongFile;
            setLabels(currentlyPlayingTrackNumber);
            makeLabelsVisible();
            setAlbumArtInPictureBox();
            resetTimer();
            mp3Handler.Play();

                
        }

        private void resetTimer()
        {
            secTimer.Stop();
            seconds = 0;
            minutes = 0;
            secTimer.Start();
        }

        private void completeStop()
        {
            mp3Handler.StopPlayback();
            mp3Handler.CloseWaveOut();
        }

        private void clearPlaylistButton_Click(object sender, EventArgs e)
        {
            if (mp3Handler.selectedSong != "")
            {
                completeStop();
            }
            mp3Handler.selectedSong = "";
            secTimer.Stop();
            currentlyPlayingTrackNumber = 0;
            currentTrack = 0;
            seconds = 0;
            minutes = 0;
            playlist.Clear();
            playlistBox.Items.Clear();
            clearLabels();

        }

        private void addAlbumArtToSongInfo(bool folder)
        {
            if (folder)
            {
                //Get album art from folder.
                albumArtInFolder = Directory.GetFiles(folderMusicBrowser.SelectedPath, "*.jpg").ToList<string>();
                //If there is album art, set that as the album art path.
                if (albumArtInFolder[0] != "")
                {
                    for (int i = artStart; i < playlist.Count; i++)
                    {
                        playlist[i].artPath = albumArtInFolder[0];
                    }
                    artStart = playlist.Count();
                }
                //If there is no album art, get art from last.fm
                else
                {
                    for (int i = artStart; i < playlist.Count; i++)
                    {
                        playlist[i].artPath = GetAlbumArtFromLastFm(playlist[artStart].Artist, playlist[artStart].Album, apiKey);
                    }
                    artStart = playlist.Count();
                }
            }
            else
            {
                albumArtInFolder = Directory.GetFiles(songSelectDialog.FileName.Remove(songSelectDialog.FileName.IndexOf(songSelectDialog.SafeFileName)), "*.jpg").ToList<string>();
                AddSongToPlaylist(albumArtInFolder[0]);
                
            }
        }

        private void setAlbumArtInPictureBox()
        {
            if (playlist[currentlyPlayingTrackNumber].artPath.Contains("http"))
            {
                albumArtBox.ImageLocation = playlist[currentlyPlayingTrackNumber].artPath;
            }
            Image image = Image.FromFile(playlist[currentlyPlayingTrackNumber].artPath);
            albumArtBox.Image = image;
        }

        private void setLabels(int index)
        {
            songLabel.Text = playlist[index].Song;
            artistLabel.Text = playlist[index].Artist;
            albumLabel.Text = playlist[index].Album;
        }

        private string GetAlbumArtFromLastFm(string artist, string album, string myApiKey)
        {
            string theArtWorkUrl = string.Empty;

            try
            {
                //form the url to query LastFM
                string baseUrl = "http://ws.audioscrobbler.com/2.0/?method=album.getinfo&api_key=" + myApiKey +
                    "&artist=" + artist + "&album=" + album;

                //create our settings
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;
                settings.IgnoreComments = true;

                //initialize a pointer
                int a = 0;

                using (XmlReader reader = XmlReader.Create(baseUrl, settings))
                {
                    while ((reader.Read()))
                    {
                        if ((reader.NodeType == XmlNodeType.Element & "image" == reader.LocalName))
                        {
                            //we are in the right node so read and exit
                            if (a == 2)
                            {
                                theArtWorkUrl = reader.ReadElementString("image");
                                break; // TODO: might not be correct. Was : Exit While
                            }
                            else
                            {
                                //not in right node so go to next
                                a = a + 1;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(theArtWorkUrl))
                {
                    return theArtWorkUrl;
                }
                else
                {
                    return theArtWorkUrl;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return theArtWorkUrl;
            }
        }

        private void makeLabelsVisible()
        {
            playlistBox.Visible = true;
            timeLeftLabel.Visible = true;
            songLabel.Visible = true;
            artistLabel.Visible = true;
            albumLabel.Visible = true;

        }

        private void clearLabels()
        {
            playlistBox.Visible = false;
            timeLeftLabel.Visible = false;
            songLabel.Visible = false;
            artistLabel.Visible = false;
            albumLabel.Visible = false;
        }

    }
    }
