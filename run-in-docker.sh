#!/bin/bash
docker run \
  --interactive \
  --tty \
  --volume $(pwd)/test/GdepTest:/var/www/default \
  --volume $(pwd)/jexus/default:/usr/jexus/siteconf/default \
  --publish 8088:80 \
  beginor/jexus-x64:5.8.2.20
