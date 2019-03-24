import React, { Component } from 'react';
import { Table, Column, AutoSizer } from 'react-virtualized'
import 'react-virtualized/styles.css'

class SongTable extends Component {
  constructor(props) {
    super(props)

    console.log(this.props)
}

  render() {
    const { songs } = this.props;
    return <AutoSizer>
      {
        ({ height, width }) => (
          <Table
            className='song-table'
            gridStyle={{outline:"0px"}}
            width={width}
            height={height - 60}
            headerHeight={20}
            rowHeight={30}
            rowCount={songs.length}
            rowGetter={({ index }) => songs[index]}
            onRowDoubleClick={(row) => this.props.handleRowDoubleClick(row.rowData)}
            rowStyle={ 
              (data) =>  {
                var index = data.index;
                var color = "#f5f5f5";
                if ((index % 2) === 0)
                  color = "#ffffff";
                return {
                  backgroundColor: color,
                  outline: "0px",
                  paddingRight: "0px"
                };
              }
            }
          >
            <Column
              label='Title'
              dataKey='title'
              width={200}
            />
            <Column
              label='Length'
              dataKey='lengthString'
              width={60}
            />
            <Column
              width={100}
              label='Artist'
              dataKey='artist'
            />
            <Column
              width={250}
              label='Album'
              dataKey='album'
            />
            <Column
              width={100}
              label='Genre'
              dataKey='genre'
            />
            <Column
              width={100}
              label='Rating'
              dataKey='rating'
              cellDataGetter={(row) => {
                if (row.rowData.rating < 0)
                  return '';
                return row.rowData.rating;
              }}
            />
          </Table>
      )}
    </AutoSizer> 
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
        <div>
          <div className='song-row-text'>
            <div>{song.title}</div>
            <div>{this.getRowArtistString(song)}</div>
          </div>
          <div className='song-row-text'>
            <div>Test</div>
          </div>
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