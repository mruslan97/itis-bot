using System;
using System.Collections.Generic;
using System.Text;
using AutoFixture;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.Tests.Utils
{
    public static class TgBotFixtureUtils
    {
        public static void ConfigureFixtureForCreateChat(Fixture fixture)
        {
            fixture.Customize<Chat>((composer => composer.Without((elem => elem.PinnedMessage))));
        }
    }
}
