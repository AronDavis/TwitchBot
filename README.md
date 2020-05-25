# TwitchBot
MrSheila!


Make sure to add an `App.config` file with Username, OAuth key, and Channel like so:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <startup> 
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5.2" />
    </startup>

  <appSettings>
	<add key="username" value="USERNAME_HERE"/>
    <add key="oauth" value="OAUTH_TOKEN_HERE"/>
	<add key="channel" value="CHANNEL_TO_JOIN_HERE"/>
  </appSettings>
</configuration>
```
