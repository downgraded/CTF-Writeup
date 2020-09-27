# DarkCTF – Agent-U

![web category](https://img.shields.io/badge/category-web-lightgrey.svg) ![author](https://img.shields.io/badge/Author-Mr.Ghost-lightgrey.svg)

![points](https://img.shields.io/badge/points-395-lightgray.svg)
![solves](https://img.shields.io/badge/solves-108-lightgray.svg)

## Challenge

> Agent U stole a database from my company but I don't know which one. Can u help me to find it?

>http://agent.darkarmy.xyz/

>flag format darkCTF{databasename}

# Solution

By reading the title, we can assume that Agent-U means User-Agent, and the description mentions a database. With the mentions of user agent and database, we can assume that this challenge will involve SQL injection in the User Agent header.

Upon visiting the website, we see a login panel, and our IP address displayed on the page. The source code contains a comment reading `<!-- TRY DEFAULT LOGIN admin:admin -->`. When we login with the admin:admin credentials, we see our User-Agent also reflected on the page. This confirms the assumption that the User Agent is involved.

To test for SQL injection, we can open Burp Suite, turn on the proxy tool, refresh the page, and send the intercepted request to the repeater tool. Changing our User-Agent header to `'a` and sending a request will result in an error message: `You have an error in your SQL syntax; check the manual that corresponds to your MySQL server version for the right syntax to use near 'a', '###.###.###.###', 'admin')' at line 1`. Additionally, our User-Agent is still reflected on the page.

This error message reveals a few details about the SQL query being made:
* It is using MySQL
* We can see part of the query: `a', '###.###.###.###', 'admin')`. This has a few revealing details:
  * Our User-Agent, IP address, and username is part of the statement.
  * We get a SQL error message, but our User-Agent is still reflected normally on the page. This means we will have to use error based SQL injection.
  * The closing parenthesis after 'admin' shows that we are enclosed in parenthesis. Possibly inside a SELECT INTO statement
  * Given the above details, we can guess that part of the MySQL query may look something like this: `SELECT (UserAgent, IP, Username) INTO ...`
    * By modifying our User-Agent to `'a`, the query would look like this: `SELECT (''a', '###.###.###.###', 'admin') INTO ...`. We closed the user agent string on our own, but are left with a trailing `a'` which is invalid syntax.
    
Now that we know the SQL query and our SQL version, we can construct a payload to extract information from the error messages. We can first test for injection with the payload `' + (select @@version) + '`, which will make the query `SELECT ('' + (select @@version) + '', '###.###.###.###', 'admin')`. This query is valid syntax and will make the database give this error message:`Truncated incorrect DOUBLE value: '5.7.31'`, showing that the database is using MySQL 5.7.31. We can try to replace `@@version`, with `database()` to show the database name, but I couldn't find a way to get an error message with the result of `database()`

## My Initial Solution
We can use conditional error messages to extract the database name one character at a time. If we use the payload `' + (SELECT IF(1=1,1/0,1)) + '`, MySQL will check if our supplied conditional statement (1=1) is true. If 1=1 is true, it will select 1/0, resulting in a `Division by 0` error message. If our supplied conditional statement is false, there will be no error message.

Rather than using `1=1` as our conditional, we can use SUBSTRING() to test if the characters of the database name are equal to other characters. This is best done one character at a time.

The payload for extracting the first character of the database name is `' + (SELECT IF(SUBSTRING(database(),1,1)='a',1/0,1)) + '`.
  * SUBSTRING('abc', 1, 1) will return 'a', substring('abc', 2, 1) will return 'b', etc.
  * We can iterate through every printable character to replace 'a' until our conditional statement is true, and then we move to the second character and repeat the process until the flag is done.

There is no reason to do all of this by hand, so I wrote a quick (and ugly) python script to automate this process.


## The Real Solution

However, about half way into exploiting the boolean based solution, I realized that there was a much simpler way to get the database name in the error message and I wasted my time.

We can try to select from a table that doesn't exist. The MySQL error message when doing so will say `Table '<database_name>.<table>' doesn't exist`. 

If we use the payload `'+(select a from b)+'` as our User-Agent, we get the error message `Table 'ag3nt_u_1s_v3ry_t3l3nt3d.b' doesn't exist`

> Flag: darkCTF{ag3nt_u_1s_v3ry_t3l3nt3d}
