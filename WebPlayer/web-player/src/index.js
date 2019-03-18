import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter, Route } from 'react-router-dom';
import ReactPlayer from 'react-player'
import ReactTable from 'react-table'
import "react-table/react-table.css";
import Progress from './componenets/progressbar';
import './index.css';
import GoogleLogin from 'react-google-login';

var baseUrl = 'http://dev.koaset.com:8080';

class Player extends React.Component {
  constructor(props) {
    super(props);
    this.player = React.createRef();
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
      loadError: null
    };
  }

  componentDidMount() {

      try {
        var storedLogin = JSON.parse(localStorage.getItem('loginResult'));
        if (storedLogin != null) {
          
          this.setState({
            session: storedLogin.session
          });
        }
        this.fetchSongs(storedLogin.session)
      }
      catch (error)
      {
        console.log('Error when reading stored login: ' + error);
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
      .then(res => res.json())
      .then(
        (result) => {
          this.setState({
            songs: result,
            isLoaded: true,
            loadError: null
          });
        },
        (error) => {
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
    var songUrl = isLoaded && playingSong != null ? baseUrl + '/library/song/play?id=' + playingSong.id + '&sessionId=' +  session : null;
    var failedText = <div>{loadError == null ? "" : "Failed to load!"}</div>

    return (
      <div>
        <div className='menu-bar'>
          <div className='control-buttons'>
            {playPauseButton}
            {volDownButton}
            {volUpButton}
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
        />}
        {this.progressBarRender()}
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
    const progressBar = this.progressBar.current;
    var percent = this.clampNumber(a.pageX/progressBar.state.width, 0, 1);
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

  pauseSong() {
    this.setState({isPlaying:false});
  }

  playSong(s) {
    this.setState({
      playingSong: s,
      isPlaying: true
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