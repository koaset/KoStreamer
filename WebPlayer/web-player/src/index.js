import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';
import ReactPlayer from 'react-player'
import ReactTable from 'react-table'
import "react-table/react-table.css";
import Progress from './componenets/progressbar';
import './index.css';

import GoogleLogin from 'react-google-login';

import FineUploaderTraditional from 'fine-uploader-wrappers'
import Gallery from 'react-fine-uploader'
import 'react-fine-uploader/gallery/gallery.css'


var baseUrl = 'https://localhost:44361';

class Player extends React.Component {
  constructor(props) {
    super(props);
    this.player = React.createRef();
    this.songTable = React.createRef();
    this.progressBar = React.createRef();
    this.state = {
      session: null,
      counter: 0,
      songs: [],
      selectedSong: null,
      playingSong: null,
      isLoaded: false,
      volume: 0.1,
      isPlaying: false,
      songProgress: 0,
      loadError: null,
      orientation : null
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
  }

  signOut() {
    var auth2 = window.gapi.auth2.getAuthInstance();
    auth2.signOut().then(() => {
      this.setState({session: null});
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
          songs: [],
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
          songs: result,
          isLoaded: true,
          loadError: null
        });
      },
      (error) => {
        console.error(error);
        this.setState({
          songs: [],
          isLoaded: true,
          loadError: error
      });
    });
  }

  render() {
    const { isPlaying, volume, isLoaded, playingSong, loadError, session } = this.state;
    var playPauseButton = this.playPauseButton();
    var volDownButton = <button className='control-button' onClick={() => this.setState({volume:Math.min(volume + 0.01, 1)})}>vol+</button>;
    var volUpButton = <button className='control-button' onClick={() => this.setState({volume:Math.max(volume - 0.01, 0)})}>vol-</button>;
    var playPreviousButton = <button className='control-button' onClick={() => this.playPreviousSong()}>&lt;&lt;</button>;
    var playNextButton = <button className='control-button' onClick={() => this.playNextSong()}>>></button>;
    var songUrl = isLoaded && playingSong != null ? baseUrl + '/library/song/play?id=' + playingSong.id + '&sessionId=' +  session : null;
    var failedText = <div>{loadError === null ? "" : "Failed to load!"}</div>

    return (
      <div>
        <div className='menu-bar'>
          <div className='control-buttons'>
            {playPreviousButton}
            {playPauseButton}
            {volDownButton}
            {volUpButton}
            {playNextButton}
          </div>
          {<GoogleLogin
            className='login-button'
            clientId="900614446703-5p76k96hle7h4ucg4qgdcclcnl4t7njj.apps.googleusercontent.com"
            buttonText="Login"
            onSuccess={(r) => this.onSignIn(r.tokenObj.id_token)}
            onFailure={() =>{}}
          />}
          <div className='now-playing'>{playingSong === null ? ' ' : playingSong.title + ' - ' +  playingSong.artist}</div>
        </div>
        
        {this.songList()}
        {failedText}
        {<ReactPlayer
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
        />}
        {this.progressBarRender()}
        {<Gallery uploader={ this.uploader() } />}
      </div>
    );
  }

  progressBarRender() {
    const songProgress = this.state.songProgress;
    return <Progress 
      className='search-bar' 
      ref={this.progressBar} 
      onClick={(a) => this.clickProgressBar(a)} 
      completed={songProgress == null ? 0 : songProgress
    }/>
  }

  onProgress(progressInfo) {
    const { playingSong } = this.state;
    var val = progressInfo.playedSeconds / (playingSong.durationMs / 1000) * 100;
    var percent = this.clampNumber(val, 0, 100);
    this.setState({songProgress: percent});
  }

  clickProgressBar(a) {
    const playingSong = this.state.playingSong;
    if (playingSong == null)
      return;
    const player = this.player.current;
        var percent = this.clampNumber(a.pageX / window.innerWidth, 0, 1);
    player.seekTo(percent);
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

  playSong(s) {
    this.setState({
      playingSong: s,
      isPlaying: true
    });
  }

  playNextSong() {
    const currentSong = this.state.playingSong;
    const songs = this.songTable.current.resolvedData;

    var nextIndex = 0;
    if (currentSong !== null)
    {
      var rowIndex = songs.findIndex((element) => {return element.id === currentSong.id});
      nextIndex = ++rowIndex < songs.length ? rowIndex : 0;
    }

    this.setState({
      playingSong: songs[nextIndex],
      isPlaying: true
    });
  }

  playPreviousSong() {
    const currentSong = this.state.playingSong;
    const songs = this.songTable.current.resolvedData;

    var previousIndex = songs.length - 1;
    if (currentSong !== null)
    {
      var rowIndex = songs.findIndex((element) => {return element.id === currentSong.id});
      previousIndex = --rowIndex >= 0 ? rowIndex : songs.length - 1;
    }

    this.setState({
      playingSong: songs[previousIndex],
      isPlaying: true
    });
  }

  uploader() {
    const session = this.state.session;
    return new FineUploaderTraditional({
      options: {
          chunking: {
              enabled: true,
              success: {
                endpoint: baseUrl + '/library/song/upload/complete'
              }
          },
          deleteFile: {
              enabled: false,
              endpoint: '/uploads'
          },
          request: {
              endpoint: baseUrl + '/library/song/upload',
              customHeaders: {
                "X-Session": session
              }
          },
          retry: {
              enableAuto: false
          },
          callbacks: {
            onComplete: (_, __, response) => this.onUploadComplete(response)
          }
      }
    });
  }

  onUploadComplete(response) {
    if (!response.success)
    {
      return;
    }
    
    var song = response.song;

    var newSongs = this.state.songs.slice();    
    newSongs.push(song);  

    this.setState({
      songs: newSongs
    });
  }

  songList() {
    const { error, isLoaded, songs } = this.state;
    if (error) {
      return <div>Error: {error.message}</div>;
    } else if (!isLoaded) {
      return <div>Loading...</div>;
    } else {
        return <ReactTable
        ref={this.songTable}
        data={songs}
        columns={[
          {
            columns: [
              {
                Header: "Title",
                accessor: "title"
              },
              {
                Header: "Artist",
                accessor: "artist"
              },
              {
                Header: "Album",
                accessor: "album"
              },
              {
                Header: "Duration",
                accessor: "lengthString"
              },
              {
                Header: "Genre",
                accessor: "genre"
              },
              {
                Header: "Rating",
                accessor: "rating"
              }
            ]
          }
        ]}
        defaultSorted={[
          {
            id: "Title",
            desc: false
          }
        ]}
        defaultPageSize={10}
        className="song-table -striped -highlight"
        getTdProps={(state, rowInfo, column, instance) => {
          return {
            onClick: (e, handleOriginal) => {
              if (rowInfo != null)
                this.handleClick(rowInfo.original);
              if (handleOriginal) {
                handleOriginal();
              }
            },
            onDoubleClick: (e, handleOriginal) => {
              if (rowInfo != null)
                this.handleDoubleClick(rowInfo.original);
              if (handleOriginal) {
                handleOriginal();
              }
            }
          };
        }}
      />
    }
  }

  handleClick(s) {
      this.setState({
        selectedSong: s
      })
  }
  
  handleDoubleClick(s) {
    this.setState({
      playingSong: s,
      isPlaying: true
    })
  }
}

// ========================================

ReactDOM.render((
  <BrowserRouter basename={'/player'}>
    <Player/>
  </BrowserRouter>
), document.getElementById('root'));