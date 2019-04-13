#!/bin/sh

# create config file
cat <<EOF >./config.js
window.STREAMER_API_URL='${RUNTIME_STREAMER_API_URL}';
EOF

# copy nginx conf if needed
if [ "$API_PROXY" = "true" ]
then
  cp /nginx.conf /etc/nginx/conf.d/default.conf;
fi

# start using args
exec "$@";