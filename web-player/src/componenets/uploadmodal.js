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
      <div className='upload-top'>
        <h3>{modalText}</h3>
        <button className='control-button' onClick={() => this.close()}>Close</button>
      </div>
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
            onValidate: (file) => this.validateFile(file),
            onComplete: (_, __, response) => this.props.onUploadComplete((response))  
          }
      }
    });
  }

  validateFile(file) {
    try {
      const allowedExtensions = ["mp3", "m4a", "wma", "aac", "flac"];
      var split = file.name.split('.');
      var extension = split[split.length - 1];
      return allowedExtensions.indexOf(extension) > -1;
    }
    catch(error) {
      console.log(error);
      return false;
    }
  }

  show() {
    this.setState({isOpen: true});
  }

  close() {
    this.setState({isOpen: false});
  }
}

export default UploadModal;