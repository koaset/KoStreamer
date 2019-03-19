using System;

namespace Streamer.API.Domain
{
    public static class IdGenerationHelper
    {
        public static string GetNewId(Func<string, bool> validationFunction)
        {
            string newId;
            var maxTries = 10;
            var i = 0;
            do
            {
                newId = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 10);
                if (++i >= maxTries)
                {
                    throw new Exception();
                }

            } while (!validationFunction(newId));

            return newId;
        }
    }
}
