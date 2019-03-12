import React from 'react';
import PropTypes from 'prop-types';

class Progress extends React.Component {

  constructor(props){
    super(props)
    this.divRef = React.createRef()
  }

  componentDidMount() {
    this.setState({width: this.divRef.current.offsetWidth})
    setInterval(() => this.setState({ time: Date.now()}), 1000)
  }

  static propTypes = {
    completed: ((props, propName) => {
      if (typeof props[propName] !== 'number')
        return Progress.throwError('Invalid Props: "completed" should ∈ ℝ ');
      if( props[propName] < 0 || props[propName] > 100) {
        return Progress.throwError('Invalid Props: "completed" should be between 0 and 100' );
      }
    }),
    color: PropTypes.string,
    animation: PropTypes.number,
    height: PropTypes.oneOfType([
      PropTypes.string,
      PropTypes.number
    ])
  }

  static defaultProps = {
    completed: 0,
    color: '#0BD318',
    animation: 0,
    height: 10
  }

  static throwError() {
    return new Error(...arguments);
  }

  render () {
    const {color, completed, animation, height, className, children, ...rest} = this.props;
    const style = {
      backgroundColor: color,
      width: completed + '%',
      transition: `width ${animation}ms`,
      height: height
    };

    return (
      <div ref={this.divRef} className={className || "progressbar-container"} {...rest}>
        <div className="progressbar-progress" style={style}>{children}</div>
      </div>
    );
  }
}

export default Progress;