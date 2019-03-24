import React, { Component } from 'react';
import ReactSidebar from "react-sidebar";
import { List } from 'react-virtualized'
import './sidebar.css';

class Sidebar extends Component {

  constructor(props) {
    super(props);
    this.itemList = React.createRef();
    this.state = {
      width: 200,
      selectedPlaylistId: this.data[0].id
    };
  }

  data = [
    { name:"Song feed", id:0 },
    { name:"All songs", id:1 },
    { name:"Artists", id:2 },
    { name:"Albums", id:3 },
    { name:"Playlists", id:4 }
  ];

  rowRenderer = ({ index, isScrolling, key, style }) => {
    var textClass = this.state.selectedPlaylistId === index ? 'sidebar-list-text-selected' : 'sidebar-list-text';
    return (
      <div 
        key={key}
        style={{margin:"10px"}}
      >
        <span 
          className={textClass}
          onClick={() => this.itemClicked(index)}
        >
          {this.data[index].name}
        </span>
      </div>
    );
  }

  itemClicked(itemIndex) {
    this.setState({
      selectedPlaylistId: itemIndex
    });
    this.itemList.current.forceUpdateGrid();
  }
  
  render() {
    return (
      <ReactSidebar
          docked={true}
          open={true}
          transitions={false}
          styles={
            { 
              sidebar: {
                background: "#dcdbd3",
                width: this.state.width
              }
            }}
          sidebar={
            <div>
              <List
                ref={this.itemList}
                className='sidebar-list'
                rowCount={this.data.length}
                width={this.state.width - 50}
                height={600}
                rowHeight={40}
                rowRenderer={this.rowRenderer}
                overscanRowCount={3}
              />
            </div>
          }
        >
        {this.props.children}
      </ReactSidebar>
    )
  }

}

export default Sidebar;