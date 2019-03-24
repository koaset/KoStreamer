import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';

import ReactPlayer from 'react-player'
import { GoogleLogin } from 'react-google-login';

import Sidebar from "./componenets/sidebar";
import Progress from './componenets/progressbar';
import SongTable from './componenets/songtable';
import UploadModal from './componenets/uploadmodal';
import SongFeed from './componenets/songfeed';

import './index.css';

var baseUrl = 'https://localhost:44361';

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
      showingSongs: [],
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
    this.setState({
      songs: this.songFeed.current.state.list
    });
  }

  signOut() {
    var auth2 = window.gapi.auth2.getAuthInstance();
    auth2.signOut().then(() => {
      
      var session = this.state.session;

      if (session)
      {
        fetch(baseUrl + "/session", {
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
    fetch(baseUrl + "/session/googleAuth", {
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

        this.fetchSongs();

        localStorage.setItem('loginResult', JSON.stringify({ session: result.session }));
      },
      (error) => {
        console.error('Error when logging in.', error);
      }
    );
  }

  fetchSongs(session) {

    if (session == null) {
      session = this.state.session;
    }

    fetch(baseUrl + "/library/songs", {
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

  getSongs () {
    return this.getSongsForPlaylist(this.state.selectedSongListId);
  }

  getSongsForPlaylist (playlistIndex) {
    const { songLists } = this.state;
    var id = songLists[playlistIndex].id;
    if (id === songFeedId) {
      return this.state.songFeedSongs;
    } else if (id == libraryId) {
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
    const { isPlaying, volume, isLoaded, playingSong, songProgress, session, librarySongs, showingSongs } = this.state;
    
    var playPauseButton = this.playPauseButton();
    var volDownButton = <button className='control-button' onClick={() => this.setState({volume:Math.min(volume + 0.01, 1)})}>vol+</button>;
    var volUpButton = <button className='control-button' onClick={() => this.setState({volume:Math.max(volume - 0.01, 0)})}>vol-</button>;
    var playPreviousButton = <button className='control-button' onClick={() => this.playPreviousSong()}>&lt;&lt;</button>;
    var playNextButton = <button className='control-button' onClick={() => this.playNextSong()}>>></button>;
    var uploadButton = <button className='control-button' onClick={() => this.uploadModal.current.show()}>upload</button>;
    
    var songUrl = isLoaded && playingSong != null ? baseUrl + '/library/song/play?id=' + playingSong.id + '&sessionId=' +  session : null;
    
    return (
      <div>
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
              <div className='now-playing'>{this.getNowPlayingString(playingSong)}</div>
            </div>
            <Progress 
              className='search-bar' 
              ref={this.progressBar}
              onClick={(clickInfo) => this.player.current.seekTo(this.progressBar.current.clickPercentage(clickInfo))} 
              completed={songProgress == null ? 0 : songProgress
            }/>
            <SongTable 
              songs={this.getSongs()} 
              ref={this.songTable} 
              handleRowDoubleClick={(s) => this.handleDoubleClick(s)} 
            />
            <ReactPlayer
              ref={this.player}
              className='react-player'
              width='0%'
              height='0%'
              url={songUrl}
              playing={isPlaying}
              volume={volume}
              progressInterval={50}
              loop={false}
              onProgress={o => this.onProgress(o)}
              onEnded={() => this.playNextSong()}
              style={{visibility: "hidden"}}
            />
            <UploadModal 
              ref={this.uploadModal}
              onUploadComplete={(result) => this.onUploadComplete(result)}
              session={session}
              apiUrl={baseUrl}
            />
            <SongFeed 
              ref={this.songFeed} 
              songs={librarySongs} 
              onUpdated={(s) => this.setState({songFeedSongs:s})}
            />
          </div>
        </Sidebar>
      </div>
    );
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

  onProgress(progressInfo) {
    const { playingSong } = this.state;
    var val = progressInfo.playedSeconds / (playingSong.durationMs / 1000) * 100;
    var percent = this.clampNumber(val, 0, 100);
    this.setState({songProgress: percent});
  }

  clampNumber(x, min, max)  {
    return Math.min(Math.max(x, min), max);
  }

  playPauseButton() {
    const { selectedSong, playingSong } = this.state;
    return <button className='control-button' onClick={() => { 
        var songToPlay = playingSong === null ? selectedSong : playingSong;
        this.playOrPause(songToPlay);
        
        if (this.state.isPlaying)
        {
          this.pauseSong();
          return;
        }

        this.playSong(songToPlay);
      }
    }>play/pause</button>
  }

  playOrPause(){
    this.setState({isPlaying:!this.state.isPlaying});
  }

  pauseSong() {
    this.setState({isPlaying:false});
  }

  playSong(song, playListId) {
    var songs = this.getSongsForPlaylist(playListId);
    var index = songs.findIndex(s => s.id === song.id);
    
    this.setState({
      playingSong: song,
      playingSongPlaylistId: playListId,
      playingSongIndex: index,
      isPlaying: true
    });
  }

  playNextSong() {
    const {playingSong, playingSongPlaylistId, playingSongIndex} = this.state;
    
    var songs = this.getSongsForPlaylist(playingSongPlaylistId);
    var currentIndex = playingSongIndex;
    var nextIndex = 0;
    if (playingSong !== null)
    {
      nextIndex = ++currentIndex < songs.length ? currentIndex : 0;
    }

    this.playSong(songs[nextIndex], playingSongPlaylistId);
  }

  playPreviousSong() {
    const {playingSong, playingSongPlaylistId, playingSongIndex} = this.state;

    var songs = this.getSongsForPlaylist(playingSongPlaylistId);
    var currentIndex = playingSongIndex;
    var previousIndex = songs.length - 1;
    if (playingSong !== null)
    {
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
    if (!response.success)
    {
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
