# MetaCTF CyberGames 2020 - Web Browsing as a Service

![web category](https://img.shields.io/badge/category-web-lightgrey.svg) 
![points](https://img.shields.io/badge/points-375-lightgray.svg)
![solves](https://img.shields.io/badge/solves-42-lightgray.svg)

## Challenge

> Since we've been specializing in converting various services into web applications, why not do one more. We're 
proud to present: Web Browsing as a Service! Now you can use our application to visit any HTTP/HTTPS page on the 
internet! In order to prevent access to our internal web server, where we keep all our secrets, we've changed the 
port of our internal web-server and implemented a WAF to make sure nobody can get access to it.

> Feel free to try it out here: http://host1.metaproblems.com:5730/

> Note: Some brute forcing is permitted for this challenge.

# Solution

The challenge description instantly screams server side request forgery. In an SSRF exploit, we can make the web server 
return internal endpoints that unauthorized users can not access. The challenge here is to bypass the WAF and find the 
correct port.

Attempting to access `localhost` or any IP including `127.` will be blocked by the WAF.

![WAF filtering localhost/127.0.0.1](https://i.imgur.com/ML2rUMV.png)

Fortunately, the IPv6 loopback address is not blocked. Accessing `http://[0:0:0:0:0:0:0:1]` or the shortened version 
of the same address `http://[::1]` will make the webpage return itself.

![IPv6 loopback address working](https://i.imgur.com/Ssa76kc.png)

From this point, it is just a matter of finding the correct port for the flag. As there are 65,535 ports, there is no reason
to ever attempt to do this manually. I quickly wrote a [python script](scan.py) to scan all of the ports.

As the length of the base website's response without returning another website has a length of `445`, anything with a differing 
length should mean that there is something hosted on the accessed port.

After some time of the script running, we see that port 7439 returns a unique response.

If we look at what `http://[::1]:7439` returns, we can see the flag.

![http://[::1]:7439](https://i.imgur.com/ysr0t49.png)

>A similar but alternative solution would have been using a service like `localtest.me`. `http://localtest.me` points to `127.0.0.1`, so its another potential WAF bypass if `http://[::1]` was blocked as well.
