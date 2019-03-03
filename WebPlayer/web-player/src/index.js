import React from 'react';
import ReactDOM from 'react-dom';
import ReactPlayer from 'react-player'
import ReactTable from 'react-table'
import "react-table/react-table.css";

import './index.css';

var dockerHosted = true;
var baseUrl = dockerHosted ? 'http://192.168.99.100:8080/api/' : 'https://localhost:44361/api/';

class Player extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      counter: 0,
      songs: [],
      selectedSong: null,
      playingSong: null,
      isLoaded: false,
      volume: 0.1,
      isPlaying: false
    };
  }

  componentDidMount() {
    fetch(baseUrl + "songs")
      .then(res => res.json())
      .then(
        (result) => {
          this.setState({
            songs: result,
            isLoaded: true
          });
        },
        (error) => {
          this.setState({
            error,
            isLoaded: true
          });
        }
      )
    }

  render() {
    const { isPlaying, volume, isLoaded, playingSong } = this.state;
    var playPauseButton = this.playPauseButton();
    var volDownButton = <button onClick={() => this.setState({volume:Math.min(volume + 0.01, 1)})}>vol+</button>;
    var volUpButton = <button onClick={() => this.setState({volume:Math.max(volume - 0.01, 0)})}>vol-</button>;
    
    var songUrl = isLoaded && playingSong != null ? baseUrl + 'song/stream/' + playingSong.id : null;

    return (
      <div>
        <div class='now-playing'>{playingSong === null ? ' ' : 'Playing:' + playingSong.title}</div>
        <div>{playPauseButton}{volDownButton}{volUpButton}</div>
        {<ReactPlayer
              ref={this.ref}
              className='react-player'
              playsinline='true'
              width='0%'
              height='0%'
              url={songUrl}
              playing={isPlaying}
              volume={volume}
              //muted={muted}
              /*onReady={() => console.log('onReady')}
              onStart={() => console.log('onStart')}
              onPlay={this.onPlay}
              onEnablePIP={this.onEnablePIP}
              onDisablePIP={this.onDisablePIP}
              onPause={this.onPause}
              onBuffer={() => console.log('onBuffer')}
              onSeek={e => console.log('onSeek', e)}
              onEnded={this.onEnded}
              onError={e => console.log('onError', e)}
              onProgress={this.onProgress}
              onDuration={this.onDuration}*/
            />}
            {this.songList()}
      </div>
    );
  }

  playPauseButton(){
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
