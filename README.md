# JwtAuthRenewWebApi

A sample for adding JWT authentication and sliding expiration support in Web API 2.

### Project Status

The important parts are there. What remains is to do is complete the documentation.

## Features

* Demonstrates validation and authentication using JWT
* Demonstrates sliding expiration / renew / reissue token
* Demonstrates custom claims and how JWT can be used in place of session / cookies
* Works with existing Web API authentication, including the `[Authorized]` attribute
  * [Authentication and Authorization in ASP.NET Web API](https://www.asp.net/web-api/overview/security/authentication-and-authorization-in-aspnet-web-api)
* Uses Microsoft's JSON Web Token Handler library -- a well-tested and authoritative source for JWT support in Web API
  * [JSON Web Token Handler](https://msdn.microsoft.com/en-us/library/dn205065(v=vs.110).aspx)
  * [`System.IdentityModel.Tokens.Jwt`](https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt/)
* Well-documented code and examples, including how to write a custom `IAuthenticationFilter` and `DelegatingHandler`
  * [Authentication Filters in ASP.NET Web API 2](https://www.asp.net/web-api/overview/security/authentication-filters)
  * [HTTP Message Handlers in ASP.NET Web API](https://www.asp.net/web-api/overview/advanced/http-message-handlers)
* IIS hosting
* Stepping stone to OAuth(2)

*The project is written to use the Web API 2 pipeline and IIS hosting, but the concepts are easily adaptable to a self-hosted OWIN type project.*

## Using the Sample

Coming soon...

## Documentation

* Using the sample
* [Issuing a JWT](docs/Issuing-JWT.md)
* [Validating a JWT](docs/Validating-JWT.md)
* Reissusing a JWT with a new (sliding) expiration

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
