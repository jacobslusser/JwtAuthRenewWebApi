# JwtAuthRenewWebApi

A sample project for adding JWT authentication and sliding expiration support in Web API 2.

### Background

I like JWT. I think it is a simple and elegant solution for authenticating REST / AJAX / Web API calls.
For some time now I've been urging my coworkers to switch our single-page-application (SPA) websites to use JWT for authentication and state instead of cookies and session.

For those unfamiliar with it, however, it can be scary, confusing, and even difficult to integrate into their new / existing projects based on some of the scattered resources and documentation out there.
This sample project is meant to illustrate that it is in fact quite simple.

### Project Status

**Done!** Just follow the instructions below for downloading and running the sample project and read the linked documentation if you want a better understanding of how it all works.

## Features

* Demonstrates validation and authentication using JWT
* Demonstrates sliding expiration / renew / reissue token
* Demonstrates custom claims and how JWT can be used in place of session / cookies
* Works with existing Web API authentication, including the `[Authorized]` attribute
* Uses Microsoft's JSON Web Token Handler library -- a well tested and authoritative source for JWT support in Web API
* Well documented code and examples, including how to write a custom `IAuthenticationFilter` and `DelegatingHandler`
* IIS hosting
* Stepping stone to OAuth(2)

*The project is written to use the Web API 2 pipeline and IIS hosting, but the concepts are easily adaptable to a self-hosted OWIN type project.*

## Using the Sample

Clone or download the repo and open it in Visual Studio and hit `F5` to run.
That should fire-up the project in Visual Studio's (IIS Express) development server (on port 30908?).

To see how JWTs are exchanged from browser to server and vice versa, open the `Example.html` file in the site root (`http://localhost:30908/example.html`).
This file has a little jQuery in it to demonstrate some of the API calls with and without JWT authentication.
Using your Chrome Developer Tools and Visual Studio, trace some of the API calls.
Read the docs.
Learn.

## Documentation

I tend to over explain things. To that end, there is more documentation explaining the code than there is actual code in the project.

* [Issuing a JWT](docs/Issuing-JWT.md)
* [Validating a JWT](docs/Validating-JWT.md)
* [Reissusing a JWT with a New (Sliding) Expiration](docs/Sliding-Expiration.md)

## Credits

The [JwtAuthWebApi](https://github.com/rnarayana/JwtAuthWebApi) project here on GitHub motivated me to create this project.
I liked the idea of a simple project to explain JWT in Web API, but found that project to be outdated and overly complicated.

## License

The MIT License (MIT)

Copyright (c) 2016 Jacob Slusser, https://github.com/jacobslusser

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
