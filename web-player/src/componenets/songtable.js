import React, { Component } from 'react';
import { Table, Column, AutoSizer } from 'react-virtualized'
import 'react-virtualized/styles.css'

class SongTable extends Component {

  render() {
    const { songs, playingIndex } = this.props;
    const isPortrait = window.innerWidth * 1.5 < window.innerHeight;
    return <AutoSizer>
      {
        ({ height, width }) => (
          <Table
            gridStyle={{outline:"0px"}}
            width={width}
            height={height - 120}
            headerHeight={20}
            rowHeight={30}
            rowCount={songs.length}
            rowGetter={({ index }) => songs[index]}
            onRowDoubleClick={(row) => this.props.handleRowDoubleClick(row.rowData)}
            rowStyle={ 
              (data) =>  {
                var index = data.index;
                return {
                  outline: "0px",
                  paddingRight: "0px",
                  backgroundColor: (index % 2) === 0 ? "#ffffff" : "#f5f5f5",
                  fontWeight: index === playingIndex ? "bold" : "",
                };
              }
            }
          >
            <Column
              label='Title'
              dataKey='title'
              width={250}
            />
            {isPortrait ? null :
            <Column
              label='Length'
              dataKey='lengthString'
              width={60}
            />}
            <Column
              width={250}
              label='Artist'
              dataKey='artist'
            />
            <Column
              width={250}
              label='Album'
              dataKey='album'
            />
            {isPortrait ? null :
            <Column
              width={150}
              label='Genre'
              dataKey='genre'
            />}
            {isPortrait ? null : 
            <Column
              width={60}
              label='Rating'
              dataKey='rating'
              cellDataGetter={(row) => {
                if (row.rowData.rating < 0)
                  return '';
                return row.rowData.rating;
              }}
            />}
          </Table>
      )}
    </AutoSizer> 
  }
}

export default SongTable;