# CSGO-Match-Notifier
Get notified when you find a match in CSGO

# This is far from finished
I recommend against using this at the current moment. I will work on it a bit here and there and try to improve it.

---

To run it currently all you have to do is compile it and add `+con_logfile matchOutput.log` to your CSGO Launch Options. Run the compiled EXE and put an audio file called `match.wav` into the same folder as the EXE, once a match is found that audio file will be played.

# Use it on your Android
Download the current Release of the App [here](https://github.com/Slowline/CSGO-Match-Notifier-App/releases).
If you dont trust the apk, you can build it yourself by cloning the repository and using the [tns build](https://docs.nativescript.org/tooling/docs-cli/project/testing/build) command.

The Software will ask you on startup whether you want to start a websocket to notify you on the phone or not.
If you choose yes, an IP-Address will be shown which you need to enter inside the Android App for it to work.
