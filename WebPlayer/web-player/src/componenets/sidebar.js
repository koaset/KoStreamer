import React, { Component } from 'react';
import ReactSidebar from "react-sidebar";

class Sidebar extends Component {

  render() {
    return (
      <ReactSidebar
          sidebar={<b>Sidebar content</b>}
          docked={true}
          open={true}
          transitions={false}
          styles={
            { 
              sidebar: {
                background: "white",
                width: "200px"
              }
            }}
        >
        {this.props.children}
      </ReactSidebar>
    )
  }

}

export default Sidebar;