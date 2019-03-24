import React, { Component } from 'react';
import { Table, Column, AutoSizer } from 'react-virtualized'
import 'react-virtualized/styles.css'

class SongTable extends Component {

  render() {
    const { songs } = this.props;
    return <AutoSizer>
      {
        ({ height, width }) => (
          <Table
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
}

export default SongTable;