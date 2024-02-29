using System.Net;
using System.Net.Sockets;


namespace Tech_Com.Managers
{
    
    public static class NetworkManager
    {
        public static TcpClient? User;
        public static IPEndPoint TelecomServer = new IPEndPoint(new IPAddress(8585858), 30009);//TODO: hardcoded ip for now
        public static async Task<bool> Connect(IPEndPoint Ip, CancellationToken cancellationToken)
        {
            if(User == null)
            {
                User = new TcpClient(AddressFamily.InterNetwork);
            }

            await User.ConnectAsync(Ip, cancellationToken);

            return User.Connected;
        }

        public static async Task<Memory<byte>> Receive(CancellationToken token)
        {
            if(User == null)
            {
                return null;
            }

            var stream = User.GetStream();

            Memory<byte> buffer = null;
            try
            {
                await stream.ReadAsync(buffer, token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;//just for readability sake. it doesnt effect the logic.
            }

            return buffer;
        }

        public static async Task<bool> Send(ReadOnlyMemory<byte> message, CancellationToken token)
        {
            if(User == null)
            {
                return false;
            }
            var stream = User.GetStream();
            try
            {
                await stream.WriteAsync(message, token);
            }
            catch(IOException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }
    }
}
