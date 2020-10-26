# MetaCTF CyberGames 2020 - Board Meeting Gone Wrong

![cryptography category](https://img.shields.io/badge/category-crypto-lightgrey.svg) 
![points](https://img.shields.io/badge/points-325-lightgray.svg)
![solves](https://img.shields.io/badge/solves-66-lightgray.svg)

## Challenge

> I stole [this sensitive document](https://metaproblems.com/0ce0ee95d3572d0cb20f1c348b96e5ff/Board_Meeting_Notes.docx) that contains some really important board notes. I have a feeling I can get some serious insight on stonks here.

> There are a few things I know about the person I stole it from. He likes animals, he likes to speak like he's a hacker to make himself seem cool, and he was born in 1972. I hope that helps.

> Can you help me crack it? I will make sure to share some of the profits.

# Solution

### Overview

An encrypted .docx file is given. The challenge here is creating a custom wordlist based on a few parameters. Given what is known about the owner of the file, the parameters are:

* Each word should be an animal
* Leet speak should be used
* 1972 or 72 should be appended to each password

### Preparing

The first thing needed to crack the file is creating a hash, which cane be done using office2john.

`wget https://raw.githubusercontent.com/magnumripper/JohnTheRipper/bleeding-jumbo/run/office2john.py`

`python office2john.py Board_Meeting_Notes.docx > hash.txt`

We now have the hash that can be used to crack the password `Board_Meeting_Notes.docx:$office$*2013*100000*256*16*e6e06de5805713d9d971f4bcb249e0c6*34a42cf8762b521292400e6854d4be75*a1a5a0a3b7038ab0fd37115744a0ca264e4f88a33110bed83440ca9668e9b138`. Also note that it's showing that it is Office 2013.

### Creating the wordlist

We can start with a list of common animals. I used [this](https://gist.githubusercontent.com/mbauer14/eaaa001b7fb8073dd576/raw/84501d87e7ac3a134700862a6b22916c9cb16773/animals.txt) 
list of about 200 common animals. 

Next, we need to convert the words to leet speak. I opted to create leet speak permutations of each word, just in case only some letters of the password used leet substitutions.
To do this easily, I used [this](https://github.com/madglory/permute_wordlist) python script. The default dictionary of this script isn't quite what I wanted, so I changed it to this:
```
leetDict = {
  'i': ['i', '!', '1'],
  'l': ['l', '1', '!'],
  'b': ['b', '8'],
  'a': ['a', '4', '@'],
  'e': ['e', '3'],
  'g': ['g', '9'],
  'o': ['o', '0'],
  'q': ['q', '9'],
  's': ['s', '5'],
  't': ['t', '7'],
}
```
After running the script, we have a fairly large permutated wordlist. 

The only remaining parameter is appending 1972 to the end of each password. A simple sed command will work fine. 

`sed -i s/$/1972/ wordlist.txt`

I kept a backup of the permutated wordlist in case the correct password did not have 1972 appended. If this password list were to have failed, I would have tried again with just 72.

### Cracking the hash

I prefer to use hashcat. First, run `hashcat --example-hash | grep 2013 -B1` to find the mode for MS Office 2013. The mode is 9600. We have everything we need to crack the hash.

We can run this command, using `-a 0` for dictionary mode, `-m 9600` for MS Office 2013, `--username` to ignore usernames in the hash file, `-o password.txt` to output the cracked hash to a file, 
`hash.txt` to specify our hash file, and `wordlist.txt` to specify the wordlist we created.

`hashcat -a 0 -m 9600 --username -o password.txt hash.txt wordlist.txt`

After some time, hashcat found the correct password. Reading password.txt shows `$office$*2013*100000*256*16*e6e06de5805713d9d971f4bcb249e0c6*34a42cf8762b521292400e6854d4be75*a1a5a0a3b7038ab0fd37115744a0ca264e4f88a33110bed83440ca9668e9b138:d0lph1n1972`. 
The password is `d0lph1n1972`. Upon opening the .docx, we are greeted with the flag.

![reading the document](https://i.imgur.com/ejmHANp.png)
