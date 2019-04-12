#!/bin/sh

# create config file
cat <<EOF >./config.js
window.STREAMER_API_URL='${RUNTIME_STREAMER_API_URL}';
EOF

# start using args
exec "$@";