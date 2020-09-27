# DarkCTF – Powershell & Suspicious

![forensics category](https://img.shields.io/badge/category-forensics-lightgrey.svg) ![author](https://img.shields.io/badge/Author-Mr.Ghost-lightgrey.svg)

Powershell: 
![points](https://img.shields.io/badge/points-430-lightgray.svg)
![solves](https://img.shields.io/badge/solves-78-lightgray.svg)

Suspicious: 
![points](https://img.shields.io/badge/points-460-lightgray.svg)
![solves](https://img.shields.io/badge/solves-49-lightgray.svg)

## Attached Files

> file.zip

## Challenge

##### Suspicious

> Suspicious software created a key. I want that key to track that software.

##### Powershell

> I want to know what is happening in my Windows Powershell.

## Summary
A zip archive containing another zip archive with an mp3 file signature is given. Extracting from the second archive gives files PowerShell.xml and Suspicious.reg, which each contain a hidden flag.

## Solution

After running `file file.zip` and `zipinfo file.zip`, file.zip appears to be a normal zip file. Instead of unzipping, we can use `binwalk -eM file.zip` to extract known file types from file.zip and also recursively extract files from extracted files. 

We now have PowerShell.xml and Suspicious.reg


#### Powershell

PowerShell.xml is a file containing many blocks in this format 
```
<Events><Event xmlns='http://schemas.microsoft.com/win/2004/08/events/event'><System><Provider Name='PowerShell'/><EventID Qualifiers='0'>400</EventID><Level>4</Level><Task>4</Task><Keywords>0x80000000000000</Keywords><TimeCreated SystemTime='2020-09-20T06:30:07.000000000Z'/><EventRecordID>18</EventRecordID><Channel>Windows PowerShell</Channel><Computer>WIN-6CNOVHMFLR0</Computer><Security/></System><EventData><Data>Available</Data><Data>None</Data><Data>  NewEngineState=Available
        PreviousEngineState=None

        SequenceNumber=9

        HostName=ConsoleHost
        HostVersion=2.0
        HostId=a539e857-7bd0-4885-b64c-5fa903ac0f86
        EngineVersion=2.0
        RunspaceId=a12c9265-77f5-4474-8ce6-c7343c320a30
        PipelineId=
        CommandName=
 cd C:\tools    CommandType=
        ScriptName=
        CommandPath=
        CommandLine=</Data></EventData></Event>
```
The relevant info seems to be CommandName. We can grep for CommandName and include the line after the line that matches our query to see the command and ignore the irrelevant lines. `cat PowerShell.xml | grep -A1 CommandName`

After manually inspecting the commands, we see a base64 decode command. `echo "ZGFya0NURntDMG1tNG5kXzBuX3AwdzNyc2gzbGx9" | base64 -d`. If we run this command, we get the flag `darkCTF{C0mm4nd_0n_p0w3rsh3ll}`


#### Suspicious

Suspicious.reg is a windows registry file. It is 1,447,945 lines long, so manual inspection isnt a viable option. 

The challenge description says `"Suspicious software created a key."` This sounds like the software is called "Suspicious". It is not saying `"A suspicious software..."`. We can try to grep the registry for "Suspicious" with `cat Suspicious.reg | grep -i suspicious` to find registry entries from "Suspicious", but this does not seem to work. After many failed greps, it is evident that there is something in the file preventing it from working. Inspecting the file in a hex editor shows that every other byte is null. 

We can include the null bytes in our grep with `cat Suspicious.reg | grep -Pai 's\x00u\x00s\x00p\x00i\x00c\x00i\x00o\x00u\x00s'`, but if the flag is in a different registry key, we will need to run more greps, and it would be more convenient not to have to include the null bytes. Instead, we can remove the null bytes from the file using `sed -i 's/\x00//g' Suspicious.reg`. Now we can freely grep the file. 

Using `cat Suspicious.reg | grep -i -A1 "suspicious"` will return lines including "suspicious", and `-A1` will make it also return the following line.  The second line will show the registry

There is one registry key from Suspicious.
```
[HKEY_USERS\S-1-5-21-1473425136-1446414652-728660776-1000\Software\Suspicious]
@="ZGFya0NURntIM3IzXzFzXzV1NXAxYzEwdXN9"
```

The registry key looks like base64. If we decode it (`echo ZGFya0NURntIM3IzXzFzXzV1NXAxYzEwdXN9 | base64 -d`), we get the flag `darkCTF{H3r3_1s_5u5p1c10us}`.
