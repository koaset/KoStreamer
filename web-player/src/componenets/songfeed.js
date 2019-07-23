import { Component } from 'react';

class SongFeed extends Component {

  constructor(props) {
    super(props);

    this.state = {
      currentIndex: 0,
      list: [],
      nextSongs: 10,
      prevSongs: 3,
    };
  }

  populate() {
    if (this.props.songSource === null || this.props.songSource.length === 0) {
      return;
    }

    const { nextSongs } = this.state;
    var numSongs = 0;

    var newSongs = [];

    while (numSongs < nextSongs) {
      var song = this.getRandomSong();
      newSongs.push(song);
      numSongs++;
    }

    this.updateSongs(newSongs);
  }

  setPlayingAndGetIndex(index) {
    var { prevSongs } = this.state;
    var newSongs = this.state.list.slice();
    var newIndex;
    if (index < prevSongs + 1) {
      newIndex = index;
    }
    else {
      var i = prevSongs;
      while (i < index) {
        newSongs.shift();
        i++;
        var song = this.getRandomSong();
        if (song) {
          newSongs.push(song);
        }
      }
      newIndex = prevSongs;
    }

    this.updateSongs(newSongs);

    this.setState({
      currentIndex: newIndex
    });
    return newIndex;
  }

  updateSongs(newSongs) {
    if (this.props.onUpdated){
      this.props.onUpdated(newSongs);
    }

    this.setState({
      list: newSongs
    });
  }

  getRandomSong() {
    var sourceList = this.props.songSource;
    if (sourceList === null || sourceList.length === 0) {
      throw new Error("No source specified");
    }
    var randomIndex = this.getRandom(0, sourceList.length); 
    return sourceList[randomIndex];
  }

  getRandom(min, max) {
    return Math.floor((Math.random() * max) + min);
  }

  componentDidUpdate (prevProps) {
    if (prevProps.songSource && prevProps.songSource.length === 0 && 
      this.props.songSource && this.props.songSource.length > 0) {
      this.populate();
    }
  }
  
  render() {
    return null;
  }
}

export default SongFeed;
