using Open.Nat;
using Serilog;
using System;

namespace Library.Server.Startup
{
    public class NatHelper
    {
        public static async void MakeSureNatMappingExists()
        {
            try
            {
                var deviceTask = new NatDiscoverer().DiscoverDeviceAsync();
                var portMapping = new Mapping(Protocol.Tcp, 5000, 32600, "Library.Server");
                var device = await deviceTask;
                await device.CreatePortMapAsync(portMapping);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unable to map NAT");
            }
        }
    }
}
