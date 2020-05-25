namespace TwitchBot
{
    public enum MessageTypeEnum
    {
        Unknown = 0,
        
        //001 to 099 are used for client-server connections only and should never travel between servers.
        Welcome = 1,
        YourHost = 2,
        Created = 3,
        MyInfo = 4,

        //Replies generated in the response to commands are found in the range from 200 to 399.
        NameReply = 353,
        EndOfNames = 366,
        MessageOfTheDay = 372,
        MessageOfTheDayStart = 375,
        MessageOfTheDayEnd = 376,

        //Error replies are found in the range from 400 to 599.

        //1,000+ are for non-numeric message types
        Capability = 1000,
        Join = 1001,
        Part = 1002,
        PrivateMessage = 1004,
        Ping = 1005
    }
}
