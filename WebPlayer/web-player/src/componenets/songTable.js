import React, { Component } from 'react';
import { List } from 'react-virtualized'
import 'react-virtualized/styles.css'

class SongTable extends Component {

  render() {
    const { songs } = this.props;
    return <List
      className='song-table'
      rowCount={songs.length}
      width={document.body.clientWidth}
      height={window.innerHeight}
      rowHeight={50}
      rowRenderer={this.rowRenderer}
      overscanRowCount={3}
    />
  }

  rowRenderer = ({ index, isScrolling, key, style }) => {
    
    const {songs, handleRowDoubleClick} = this.props;

    const song = songs[index];
    let styles = {...style, ...{
      backgroundColor: index % 2 === 0 ? "#f9f9f9" : "#e8e8e8"
    }};
    
    return (
      <div 
        key={key}
        style={styles}
        onDoubleClick={() => handleRowDoubleClick(song)}
      >
        <img 
          alt=''
          className='song-row-image'
          draggable={false} 
          height={50} 
          width={50} 
          src={`data:${song.artMimeType};base64,${song.art}`}
          background="black"
        />
        <div className='song-row-text'>
          <div>{song.title}</div>
          <div>{this.getRowArtistString(song)}</div>
        </div>
      </div>
    );
  };

  getRowArtistString(song) {
    var ret = '';
    if (song.artist && song.artist !== '')
       ret += song.artist;
    if (song.album && song.album !== '')
       ret += ' - ' + song.album;
    return ret;
  }
}

export default SongTable;