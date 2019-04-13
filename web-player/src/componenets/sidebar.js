import React, { Component } from 'react';
import ReactSidebar from "react-sidebar";
import { List } from 'react-virtualized'
import './sidebar.css';

class Sidebar extends Component {

  constructor(props) {
    super(props);
    this.itemList = React.createRef();
    this.state = {
      width: 85,
      selectedPlaylistId: this.props.songLists[0].id
    };
  }

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
          {this.props.songLists[index].name}
        </span>
      </div>
    );
  }

  itemClicked(itemIndex) {
    const current = this.state.selectedPlaylistId;

    if (current === itemIndex)
      return;

    this.setState({
      selectedPlaylistId: itemIndex
    });
    this.itemList.current.forceUpdateGrid();
    if (this.props.onItemSelected) {
      this.props.onItemSelected(itemIndex);
    }
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
                rowCount={this.props.songLists.length}
                width={this.state.width}
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