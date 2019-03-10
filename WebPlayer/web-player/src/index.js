import React from 'react';
import ReactDOM from 'react-dom';
import ReactPlayer from 'react-player'
import ReactTable from 'react-table'
import "react-table/react-table.css";
import Progress from './componenets/progressbar';
import './index.css';

var baseUrl = process.env.STREAMER_API_URL;

if (baseUrl == null)
{
  baseUrl = 'https://localhost:44361';
}

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
    this.onSignIn = this.onSignIn.bind(this);
  }
  
  componentDidMount() {
    window.gapi.signin2.render('g-signin2', {
      'scope': 'https://www.googleapis.com/auth/plus.login',
      'width': 100,
      'height': 25,
      'longtitle': false,
      'theme': 'light',
      'onSuccess': this.onSignIn
    }); 

    fetch(baseUrl + "/library/songs")
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
        }
      );
    }

    signOut() {
      var auth2 = window.gapi.auth2.getAuthInstance();
      auth2.signOut().then(() => {
        this.setState({session: null});
        window.location.reload();
      });
    }
  
    onSignIn(googleUser) {
      var idToken = googleUser.getAuthResponse().id_token;

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
          this.setState({session: result.session});
        },
        (error) => {
          console.error('Error when logging in.', error);
        }
      );
    }

  render() {
    const { isPlaying, volume, isLoaded, playingSong, songProgress, loadError } = this.state;
    var playPauseButton = this.playPauseButton();
    var volDownButton = <button onClick={() => this.setState({volume:Math.min(volume + 0.01, 1)})}>vol+</button>;
    var volUpButton = <button onClick={() => this.setState({volume:Math.max(volume - 0.01, 0)})}>vol-</button>;
    var songUrl = isLoaded && playingSong != null ? baseUrl + 'song/play?id=' + playingSong.id : null;
    var failedText = <div>{loadError == null ? "" : "Failed to load!"}</div>

    return (
      <div>
        <div id="g-signin2" data-onsuccess="onSignIn" />
        <button onClick={() => this.signOut()}>Sign out</button>
        <div className='now-playing'>{playingSong === null ? ' ' : 'Playing:' + playingSong.title}</div>
        <div>{playPauseButton}{volDownButton}{volUpButton}</div>
        <Progress ref={this.progressBar} onClick={(a) => this.clickProgressBar(a)} completed={songProgress == null ? 0 : songProgress} />
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
            {this.songList()}
      </div>
    );
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
    const { isPlaying, selectedSong, playingSong } = this.state;
    return <button onClick={() => { 
      var newIsPlaying = !isPlaying;
      var songToPlay = playingSong === null ? selectedSong : playingSong;
      this.setState({
        isPlaying:newIsPlaying,
        playingSong:songToPlay
      })}
    }>play/pause</button>
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
        className="-striped -highlight"
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

ReactDOM.render(<Player/>, document.getElementById("root"));
