# IAT-Design
<b>IAT Design\bin\Release</b> will contain the necessary files once the software is built with Microsoft Visual Studio, which can be downloaded here: https://visualstudio.microsoft.com/vs/community/. It is not quite ready in its current form to be put on the server and will not connect to it. If you really want, you can download the server software and follow the setup instructions. If you're psychotically fixatted on me or view it as social commentary or something. For the latest function version of my software that is fully functional, go to https://iatsoftware.net. Documentation and sample tests are available there as well.

![iat-design-screenshots](https://user-images.githubusercontent.com/35156960/155852232-4c53ddf5-c079-4f59-ac82-ea3e0dedf670.gif)


In order for the program to start, it must be activated but this is not possible right now wikth all the URLs pointing to localhost instead of iatsoftware.net. Insert the following ik a file named IATDesign.xml in <b>%USER%\AppData\Local\IATSoftware</b>

```xml
<?xml version="1.0" encoding="utf-8"?>
<IATDesign>
  <Version>1.1.1.43</Version>
  <Version_1_1_confirmed>True</Version_1_1_confirmed>
  <IATActivationKey>laH8pGseVWi++RPTwjWHQxTrCGMBI6ciMwCIWfEWM7qzt9iszRk30wZYdiZqwYPy</IATActivationKey>
  <UserEMail>nikki@bix.blue</UserEMail>
  <IATProductCode>2L9JBMR74EYKHJ7RKWPE</IATProductCode>
  <ClientName>Ms Nikki Lissome</ClientName>
</IATDesign>
```

Note that the AppData directory is hidden by default.
