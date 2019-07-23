import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';

import { GoogleLogin } from 'react-google-login';

import Sidebar from "./componenets/sidebar";
import Progress from './componenets/progressbar';
import SongTable from './componenets/songtable';
import UploadModal from './componenets/uploadmodal';
import SongFeed from './componenets/songfeed';

import './index.css';

var apiPath = window.STREAMER_API_URL ? window.STREAMER_API_URL : 'http://localhost:55430/';

const songFeedId = 0;
const libraryId = 1;

class Player extends React.Component {
  constructor(props) {
    super(props);
    this.player = React.createRef();
    this.songTable = React.createRef();
    this.progressBar = React.createRef();
    this.uploadModal = React.createRef();
    this.songFeed = React.createRef();
    this.state = {
      session: null,
      counter: 0,
      librarySongs: [],
      songFeedSongs: [],
      selectedSong: null,
      playingSong: null,
      playingSongPlaylistId: null,
      playingSongIndex: null,
      isLoaded: false,
      volume: 0.1,
      isPlaying: false,
      songProgress: 0,
      loadError: null,
      orientation : null,
      songLists: [
        { name:"Song feed", id:0 },
        { name:"All songs", id:1 }
      ],
      selectedSongListId: 0
    };
  }

  componentDidMount() {

    var storedLogin = JSON.parse(localStorage.getItem('loginResult'));

    if (storedLogin != null) {
      
      this.setState({
        session: storedLogin.session
      });

      this.fetchSongs(storedLogin.session)
    }
    
    this.setState({isLoaded:true});

    this.songFeed.current.populate();

    if (window.navigator.mediaSession !== null) {
      navigator.mediaSession.setActionHandler('play', () => this.playOrPause());
      navigator.mediaSession.setActionHandler('pause', () => this.playOrPause());
      navigator.mediaSession.setActionHandler('previoustrack', () => this.playPreviousSong());
      navigator.mediaSession.setActionHandler('nexttrack', () => this.playNextSong());
    }

    this.setState({
      songs: this.songFeed.current.state.list
    });

    this.player.ontimeupdate = () => this.onProgress();
    this.player.onended = () => this.playNextSong();
  }

  signOut() {
    var auth2 = window.gapi.auth2.getAuthInstance();
    auth2.signOut().then(() => {
      
      var session = this.state.session;

      if (session)
      {
        fetch(apiPath + "/session", {
          method: 'DELETE',
          headers: {
            'Accept': 'application/json',
            'X-Session': session
          }
        });
      }

      this.setState({session: null});
      localStorage.removeItem('loginResult');
      window.location.reload();
    });
  }
  
  onSignIn(idToken) {
    fetch(apiPath + "/session/googleAuth", {
      method: 'POST',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({IdToken: idToken})
    })
    .then(res => res.json())
    .then(
      (result) => {
        this.setState({
          session: result.session
        });
        
        localStorage.setItem('loginResult', JSON.stringify({ session: result.session }));
        
        this.fetchSongs(result.session);
      },
      (error) => {
        console.error('Error when logging in.', error);
      }
    );
  }

  fetchSongs(session) {

    if (session === null) {
      session = this.state.session;
    }

    fetch(apiPath + "/library/songs", {
      headers: {
        'X-Session': session
      }
    })
    .then(res => {
      if (res.status === 401) {

        localStorage.removeItem('loginResult');

        this.setState({
          librarySongs: [],
          isLoaded: true,
          session: null,
          loadError: null
        });

        return;
      }

      return res.json();
    })
    .then(
      (result) => {
        this.setState({
          librarySongs: result,
          isLoaded: true,
          loadError: null
        });
      },
      (error) => {
        console.error(error);
        this.setState({
          librarySongs: [],
          isLoaded: true,
          loadError: error
      });
    });
  }

  getSongsForPlaylist (playlistIndex) {
    const { songLists } = this.state;
    var id = songLists[playlistIndex].id;
    if (id === songFeedId) {
      return this.state.songFeedSongs;
    } else if (id === libraryId) {
      return this.state.librarySongs;
    }
    else {
      // other playlists
      console.log('Not implemented');
    }
  }

  selectPlaylist (index) {
    this.setState({
      selectedSongListId: index
    });
  }

  render() {
    const { isLoaded, playingSong, songProgress, session, librarySongs, playingSongPlaylistId, selectedSongListId, playingSongIndex } = this.state;
    const isPortrait = window.innerWidth * 1.5 < window.innerHeight;

    var playPauseButton = this.playPauseButton();
    var volDownButton = <button className='control-button' onClick={() => this.addToVolume(0.01)}>vol+</button>;
    var volUpButton = <button className='control-button' onClick={() => this.addToVolume(-0.01)}>vol-</button>;
    var playPreviousButton = <button className='control-button' onClick={() => this.playPreviousSong()}>&lt;&lt;</button>;
    var playNextButton = <button className='control-button' onClick={() => this.playNextSong()}>>></button>;
    var uploadButton = <button className='control-button' onClick={() => this.uploadModal.current.show()}>upload</button>;
    
    var songUrl = isLoaded && playingSong != null ? apiPath + '/library/song/play?id=' + playingSong.id + '&sessionId=' +  session : null;
    var showingSongs = this.getSongsForPlaylist(this.state.selectedSongListId);
    
    return (
      <div>
          <audio 
            ref={ref => this.player = ref}
            controls={false}
            loop={false}
          >
            <source src={songUrl}></source>
          </audio>
          <Sidebar 
            songLists={this.state.songLists}
            onItemSelected={(i) => this.selectPlaylist(i)}
          >
          <div className='main'>
            <div className='menu-bar'>
              <div className='control-buttons'>
                {playPreviousButton}
                {playPauseButton}
                {volDownButton}
                {volUpButton}
                {playNextButton}
                {uploadButton}
              </div>
              {this.googleButton()}
              {<div 
                className='now-playing' 
                style={isPortrait ? { clear: "both" } : null}
              >
                {this.getNowPlayingString(playingSong)}
              </div>}
            </div>
            <Progress 
              className='search-bar' 
              ref={this.progressBar}
              onClick={(clickInfo) => this.onProgressBarClick(clickInfo)}
              completed={songProgress === null ? 0 : songProgress
            }/>
            <SongTable 
              songs={showingSongs} 
              playingIndex={playingSongPlaylistId === selectedSongListId ? playingSongIndex : null}
              ref={this.songTable} 
              handleRowDoubleClick={(s) => this.handleDoubleClick(s)} 
            />
            <UploadModal 
              ref={this.uploadModal}
              onUploadComplete={(result) => this.onUploadComplete(result)}
              session={session}
              apiUrl={apiPath}
            />
            <SongFeed 
              ref={this.songFeed} 
              songSource={librarySongs} 
              onUpdated={(s) => this.setState({songFeedSongs:s})}
            />
          </div>
        </Sidebar>
      </div>
    );
  }

  addToVolume(increment) {
    var newVol = increment > 0 ? Math.min(this.state.volume + increment, 1) : Math.max(this.state.volume + increment, 0);
    this.player.volume = newVol;
    this.setState({volume:newVol});
  }

  onProgressBarClick(clickInfo) {
    const playingSong = this.state.playingSong;
    if (playingSong) {
      const clickPercent = this.progressBar.current.clickPercentage(clickInfo);
      this.player.currentTime = clickPercent * playingSong.durationMs / 1000;
      this.setState({songProgress: clickPercent * 100});
    }
   }

  googleButton() {
    var loggedIn = this.state.session ? true : false;
    return loggedIn ? (
    <button
      className='google-button'
      onClick={() => {this.signOut()}}
    >Logout</button>
    ) : (
    <GoogleLogin
      className='google-button'
      clientId="900614446703-5p76k96hle7h4ucg4qgdcclcnl4t7njj.apps.googleusercontent.com"
      buttonText="Login"
      onSuccess={(r) => this.onSignIn(r.tokenObj.id_token)}
      onFailure={() =>{}}
    />
    );
  }

  getNowPlayingString(playingSong) {
    var nowPlayingString = '';
    if (playingSong)
    {
      nowPlayingString = playingSong.title;
      if (playingSong.artist && playingSong.artist !== '')
        nowPlayingString += ' - ' + playingSong.artist;
    }
    return nowPlayingString;
  }

  onProgress() {
    const { playingSong } = this.state;
    var val = this.player.currentTime / (playingSong.durationMs / 1000) * 100;
    var percent = this.clampNumber(val, 0, 100);
    this.setState({songProgress: percent});
  }

  clampNumber(x, min, max)  {
    return Math.min(Math.max(x, min), max);
  }

  playPauseButton() {
    const { playingSong, selectedSongListId } = this.state;
    return <button className='control-button' onClick={() => { 
        if (playingSong === null)  {
            var songs = this.getSongsForPlaylist(selectedSongListId);
            if (songs.length > 0) {
              this.playSong(songs[0], selectedSongListId);
            }
        }
        else {
          this.playOrPause();
         }
      }
    }>play/pause</button>
  }

  playOrPause(){
    const newIsPlaying = !this.state.isPlaying;
    if (newIsPlaying) {
      this.player.play();
    }
    else {
      this.player.pause();
    }
    
    this.setState({isPlaying:newIsPlaying});
  }

  pauseSong() {
    this.player.current.pause();
    this.setState({isPlaying:false});
  }

  playSong(song, playListId) {
    var songs = this.getSongsForPlaylist(playListId);
    var index = songs.findIndex(s => s.id === song.id);
    
    if (playListId === songFeedId) {
      this.songFeed.current.playSongAt(index);
      index = this.songFeed.current.state.numPrev;
    }
    
    this.player.volume = this.state.volume;
    this.player.load();
    this.player.play().catch(err => {});
    
    this.setSongMetadata(song);
    this.setState({
      playingSong: song,
      playingSongPlaylistId: playListId,
      playingSongIndex: index,
      isPlaying: true
    });
  }

  setSongMetadata(song) {
    if (window.navigator.mediaSession !== null) {
      window.navigator.mediaSession.metadata = new window.MediaMetadata({
        title: song.title,
        artist: song.artist,
        album: song.album
      });
    }
  }

  playNextSong() {
    const {playingSong, playingSongPlaylistId, playingSongIndex} = this.state;
    
    var songs = this.getSongsForPlaylist(playingSongPlaylistId);
    var currentIndex = playingSongIndex;
    var nextIndex = 0;
    if (playingSong !== null) {
      nextIndex = ++currentIndex < songs.length ? currentIndex : 0;
    }

    this.playSong(songs[nextIndex], playingSongPlaylistId);
  }

  playPreviousSong() {
    const {playingSong, playingSongPlaylistId, playingSongIndex} = this.state;

    if (playingSongPlaylistId === songFeedId && playingSongIndex === 0) {
      return;
    }

    var songs = this.getSongsForPlaylist(playingSongPlaylistId);
    var currentIndex = playingSongIndex;
    var previousIndex = songs.length - 1;
    if (playingSong !== null) {
      previousIndex = --currentIndex >= 0 ? currentIndex : songs.length - 1;
    }

    this.playSong(songs[previousIndex], playingSongPlaylistId);
  }

  handleClick(s) {
    this.setState({
      selectedSong: s
    })
  }
  
  handleDoubleClick(s) {
    this.playSong(s, this.state.selectedSongListId);
  }

  onUploadComplete(response) {
    if (!response.success) {
      return;
    }
    
    var song = response.song;
    var newSongs = this.state.librarySongs.slice();    
    newSongs.push(song);  

    this.setState({
      librarySongs: newSongs
    });
  }
}

// ========================================

ReactDOM.render((
  <BrowserRouter basename={'/player'}>
    <Player/>
  </BrowserRouter>
), document.getElementById('root'));
