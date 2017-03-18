using Common.RemoteStorage.Command;
using System;
using System.Collections.Generic;
using System.Text;
using Common.RemoteStorage.Models;

namespace SQLDatabase.RemoteStorage.Command
{
    public class LocationPoster : ILocationPoster
    {
        public void PostLocation(byte[] identifier, Location location)
        {
            throw new NotImplementedException();
        }
    }
}
