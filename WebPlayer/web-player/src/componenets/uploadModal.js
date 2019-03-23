import React, { Component } from 'react';
import FineUploaderTraditional from 'fine-uploader-wrappers'
import Gallery from 'react-fine-uploader'
import 'react-fine-uploader/gallery/gallery.css'
import Modal from 'react-modal';

class UploadModal extends Component {

  constructor(props) {
    super(props);
    this.state = {
      isOpen: this.props.isOpen
    };
  }

  render() {
    const session = this.props.session;
    const modalText = session ? "Drag music files to upload!" : "You need to be logged in to upload.";

    return <Modal
      className='upload-modal'
      appElement={document.getElementById('root')}
      isOpen={this.state.isOpen}
    >
      <h3>{modalText}</h3>
      <button onClick={() => this.close()}>Close</button>
      {session ? <div>{<Gallery uploader={ this.uploader() } />}</div> : null}
    </Modal>
  }

  uploader() {
    return new FineUploaderTraditional({
      options: {
          chunking: {
              enabled: true,
              success: {
                endpoint: this.props.apiUrl + '/library/song/upload/complete'
              }
          },
          deleteFile: {
              enabled: false,
              endpoint: '/uploads'
          },
          request: {
              endpoint: this.props.apiUrl + '/library/song/upload',
              customHeaders: {
                "X-Session": this.props.session
              }
          },
          retry: {
              enableAuto: false
          },
          callbacks: {
            onComplete: (_, __, response) => this.props.onUploadComplete((response))
          }
      }
    });
  }

  show() {
    this.setState({isOpen: true});
  }

  close() {
    this.setState({isOpen: false});
  }
}

export default UploadModal;