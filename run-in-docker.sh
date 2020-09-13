#!/bin/bash
docker run --rm \
  --interactive \
  --tty \
  --volume $(pwd)/test/GdepTest/wwwroot:/var/www/default \
  --volume $(pwd)/test/GdepTest/bin/Debug/net461:/var/www/default/bin \
  --volume $(pwd)/test/GdepTest/bin/Debug/net461/GdepTest.exe.config:/var/www/default/web.config \
  --volume $(pwd)/jexus/default:/usr/jexus/siteconf/default \
  --publish 8088:80 \
  --name jexus-test \
  beginor/jexus-x64:6.2.2.1
rm -rf $(pwd)/test/GdepTest/wwwroot/bin $(pwd)/test/GdepTest/wwwroot/web.config
