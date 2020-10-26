# MetaCTF CyberGames 2020 - When life gives you hashes, crack them!

![cryptography category](https://img.shields.io/badge/category-crypto-lightgrey.svg) 
![points](https://img.shields.io/badge/points-525-lightgray.svg)
![solves](https://img.shields.io/badge/solves-13-lightgray.svg)

## Challenge

> You and your team of penetration testers recently compromised a site running [Umbraco CMS](https://umbraco.com/), an open-source ASP.NET content management system. Now, in order to escalate privileges, you'd like to dump the site's passwords. Here's [a zip](https://metaproblems.com/219d776ffafa9a189b569689f2c5be3f/UmbracoCms8-8.zip) of the files from the server which includes the database.

> Your goal is to recover Aaron's password which you'll submit as the flag. Note that their password is verbatim in rockyou.txt, so you can do a straight dictionary attack without any rules.

# Solution

We need to locate the database in the folder, find the hash for the user Aaron, and crack it using the rockyou wordlist.

After some poking around and googling about Umbraco, I located the database file `Umbraco.sdf` inside `UmbracoCms.8.8.0/App_Data`. 

![database location](https://i.imgur.com/1zVGPDg.png)

Using SQL Compact Query Analyzer to view the sdf file, we can see Aaron's password hash in the `umbracoUser` table.

![aaron database entry](https://i.imgur.com/PpyTQOk.png)

The hash is `RTnbzngRZFDZcvE5mioAHQ==e2+n3Gg3oBpH+nPWlQIjiAKYU4tWALorc83axst1dPU=`. We can also see that the hash algorithm is `HMAC-SHA256`, and it seems to be base64 encoded.

After a lot of trial and error attempting to crack this with hashcat and John, I took a different approach. I did a lot of googling about the hash algorithm and eventually found this stackoverflow post, [Validating HMACSHA256 in C#](https://stackoverflow.com/questions/59070030/validating-hmacsha256-in-c-sharp),
with working code in the answers. After slight modification to use Aaron's hash and to check the hash against passwords from a wordlist, I had the final [program](hashcrack.cs) I needed to crack the hash.

After running the program, we can see that the password is `iloveaaron`.
![cracking the hash](https://media.giphy.com/media/k9kmzAFBRxEHI0PXwH/giphy.gif)

### Forethoughts

I am very interested to see other people's solutions to this challenge. I spent a solid 2 hours on cracking the hash. Maybe I just wasn't looking in the right places, 
or lacked some fundamental knowldege, but I liked the fact that I couldn't find much concrete info on how to crack this. Having to write my own C# to modify code found 
on an obscure stackoverflow post was fun because I had never even touched C# prior to this. I almost gave up many times, but I'm very glad I stuck with it and managed 
to solve a challenge that only 12 other teams had solved.
