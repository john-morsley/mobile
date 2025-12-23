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



- Log into your Twilio account and add the URL given when you run the above command


