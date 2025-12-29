Morsley UK Mobile API
=====================

The API performs the following:

- Send an SMS message - POST /api/sms/
- Receive an SMS (via a callback Web Hook from Twilio) - POST /api/sms/twilio-callback
- Get an existing SMS by ID - GET /api/sms/{id}
- Delete an existing SMS by ID - DELETE /api/sms/{id} 

NGROK
-----

It is possible to test the callback Web Hook by using ngrok: ngrok.com

This app grants you an externally public URL, which you can add to Twilio as the callback URL, and it will direct traffic to your locally hosted API.

- Download at: https://dashboard.ngrok.com/get-started/setup/windows and use the 'Download' tab to download manually.
- Create an account and Log in.
- Click 'Your Authtoken' and scroll down to the 'Command Line' section and copy the command to the clipboard.

```
ngrok config add-authtoken [YOUR AUTH TOKEN]
```

- Execute the above command 

```
ngrok http [PORT]
```

Which is currently...

```
ngrok http 5123
```

Take the 'Forwarding' URL and append our Twilio callback endpoint...

i.e.

```
https://unpalsied-kevin-quaveringly.ngrok-free.dev/api/sms/twilio-callback
```

...and add this to Twilio:

- Log into your Twilio account
- Phone Numbers -> Active Numbers -> [Number]
- Scroll down to 'Messaging Configuration'
- Set the 'A message comes in' URL to the above URL

Now any callbacks to Twilio will be redirected to localhost.

So start up your development environment in debug mode, set a breakpoint, and that breakpoint should be hit when the send an SMS to the [Number].


