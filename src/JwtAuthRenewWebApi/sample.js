
$.get('http://localhost:30908/api/v1/ping')
  .done(function () {
    // This call will always work because it doesn't have the [Authorize] attribute
  });

$.get('http://localhost:30908/api/v1/ping/authenticated')
  .fail(function () {
    // This call will only work when the request is authenticated because it uses the [Authorize] attribute
  });

var credentials = {
  emailAddress: 'liz.lemon@example.com',
  password: 'Password1'
};

var promise = $.post('http://localhost:30908/api/v1/users/authenticate', credentials);
promise.done(function (data) {
  var jwt = data.token; // Could be stored in localStorage

  $.get({
    url: 'http://localhost:30908/api/v1/ping/authenticated',
    headers: {
      Authorization: 'Bearer ' + jwt
    }
  })
    .done(function () {
      // This call will now succeed because we are passing the JWT in the Authorization header
    });

  $.get({
    url: 'http://localhost:30908/api/v1/users/3001',
    headers: {
      Authorization: 'Bearer ' + jwt
    }
  })
    .done(function () {
      // This call will succeed when we are asking for our own user record, but will fail if we try to access another user's record
    });
});

