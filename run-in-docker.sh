#!/bin/bash
docker run --rm \
  --interactive \
  --tty \
  --volume $(pwd)/test/GdepTest:/var/www/default \
  --volume $(pwd)/jexus/default:/usr/jexus/siteconf/default \
  --publish 8088:80 \
  --name jexus-test \
  beginor/jexus-x64:5.8.3-RC1
