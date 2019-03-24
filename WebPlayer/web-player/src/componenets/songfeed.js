import { Component } from 'react';

class SongFeed extends Component {

  constructor(props) {
    super(props);

    this.state = {
      index: 0,
      list: [],
      nextSongs: 15,
      numPrev: 5
    };
  }

  populate() {
    var sourceList = this.props.songs;
    if (sourceList === null || sourceList.length === 0) {
      return;
    }
    
    const { list, nextSongs, numPrev } = this.state;
    var numSongs = list.length;

    if (numSongs < nextSongs) {
      var newSongs = [];

      while (numSongs < nextSongs)
      {
        var randomIndex = this.getRandom(0, sourceList.length);
        newSongs.push(sourceList[randomIndex]);
        numSongs++;
      }
      
      console.log('new songs');
      console.log(newSongs);

      if (this.props.onUpdated){
        this.props.onUpdated(newSongs);
      }
      
      this.setState({
        list: newSongs
      });
    }
  }

  getRandom(min, max) {
    return Math.floor((Math.random() * max) + min);
  }

  componentDidUpdate (prevProps) {
    if (prevProps.songs.length === 0 && this.props.songs.length > 0) {
      this.populate();
    }
  }
  
  render() {
    return null;
  }
}

export default SongFeed;
