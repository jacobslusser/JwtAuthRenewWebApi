(function () {
    'use strict';

    var token;

    // Look for an updated JWT in every repsonse
    $(document).ajaxComplete(function (event, jqXHR, ajaxOptions) {
        if (jqXHR.status >= 200 && jqXHR.status < 400) {
            var newToken = jqXHR.getResponseHeader('Set-Authorization');
            if (newToken) {
                token = newToken;
            }
        }
    });

    $('#ping').submit(function (event) {
        event.preventDefault();
        var settings = {
            url: 'api/v1/ping/authenticated',
            headers: {
                Authorization: (token ? 'Bearer ' + token : '')
            }
        };
        $.get(settings)
            .then(function () {
                alert('#ping-alert', 'success', 'Success!', "You are authenticated and have ping'd an API that requires authentication.");
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                if (jqXHR.status === 401) {
                    alert('#ping-alert', 'danger', 'On snap!', 'You are not authenticated and therefore cannot access this API.');
                }
                else {
                    alert('#ping-alert', 'danger', 'On snap!', 'An unexpected error occurred. Are you sure the server is running?');
                }
            });
    });

    $('#authenticate').submit(function (event) {
        event.preventDefault();
        var credentials = $(this).serialize();
        $.post('api/v1/users/authenticate', credentials)
            .then(function (user) {
                // The JWT is returned in the token property
                token = user.token;
                alert('#authenticate-alert', 'success', 'Success!', 'The authentication token (JWT) is now stored in a local variable and will be used in any subsequent requests.');
                console.log('Token: ' + token);
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                if (jqXHR.status === 401) {
                    alert('#authenticate-alert', 'danger', 'On snap!', 'Invalid email address and/or password.');
                }
                else {
                    alert('#authenticate-alert', 'danger', 'On snap!', 'An unexpected error occurred. Are you sure the server is running?');
                }
            });
    });

    // A simple helper function to display alerts
    function alert(placeholder, type, title, message) {
        var html = $('#alert-template').text();
        $(html).appendTo(placeholder).addClass('alert-' + type)
            .find('.alert-title').html(title).end()
            .find('.alert-message').html(message);
    }
}());
