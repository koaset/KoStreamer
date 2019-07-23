import { Component } from 'react';

class SongFeed extends Component {

  constructor(props) {
    super(props);

    this.state = {
      index: 0,
      list: [],
      nextSongs: 15,
      numPrev: 0
    };
  }

  populate() {
    if (this.props.songSource === null || this.props.songSource.length === 0) {
      return;
    }

    const { list, nextSongs } = this.state;
    var numSongs = list.length;

    if (numSongs < nextSongs) {
      var newSongs = [];

      while (numSongs < nextSongs) {
        var song = this.getRandomSong();
        newSongs.push(song);
        numSongs++;
      }

      this.updateSongs(newSongs);
    }
  }

  playSongAt(index) {
    if (index === 0) {
      return;
    }
    
    var newSongs = this.state.list.slice();

    var i = 0;
    while (i < index) {
      newSongs.shift();
      i++;
      var song = this.getRandomSong();
      if (song) {
        newSongs.push(song);
      }
    }

    this.updateSongs(newSongs);
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
