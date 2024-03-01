using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Tech_Com.Managers;

namespace Tech_Com.Pages;

public struct Info
{
	public int Pos;
	public int Current;
}

public partial class QRTicket : ContentPage
{
    IPEndPoint? IPEndPoint;
    Task? Check;
    System.Timers.Timer timer; 
    public QRTicket()
	{
		InitializeComponent();
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Task.Run(async () =>
        {
            while (true)
            {
                //it's hacky, it's unreliable and i hate it. the trifecta of a perfect program
                string addressString = string.Empty;
                
                string[] addressStrings = NetworkInterface.GetAllNetworkInterfaces()
                .Where(i => i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                .SelectMany(i => i.GetIPProperties().UnicastAddresses)
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.Address.ToString())
                .ToArray();
                
                foreach(string _ip in addressStrings)
                {
                    Console.Write(_ip);
                }
                IPEndPoint? ip;
                if (!IPEndPoint.TryParse(addressString, out ip))
                {
                    await DisplayAlert("Oops! something went wrong", "make sure you are connected to a network", "ok");
                    //wait 10sec and check again
                    Thread.Sleep(10000);
                }
                TcpClient thisUser = new TcpClient();
                try
                {
                    thisUser.Connect(ip);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                if(thisUser.Connected == true)
                {
                    var stream = thisUser.GetStream();
                    Memory<byte> memory = new();
                    await stream.ReadAsync(memory, new CancellationTokenSource(1000).Token);
                    string text = Encoding.UTF8.GetString(memory.ToArray());
                    if (text == "telecom")
                    {
                        IPEndPoint = ip;
                        return;
                    }
                }
            }
        });
    
        Refresh(null, null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (Check == null)
        {
            return;
        }
        Check.Dispose();
        Check = null;
        timer.Stop();
    }
    public async void ScanQR(object sender, EventArgs e)
	{
        if(IPEndPoint == null)
        {
            await DisplayAlert("Oh crap!", "the ip is null. did you not connect to a network?", "sure");
            return;
        }
        TcpClient tcpClient = new TcpClient(IPEndPoint);
        await tcpClient.ConnectAsync(IPEndPoint);
        if(tcpClient.Connected == false)
        {
            await DisplayAlert("Oh No!", "failed to connect to the host! make sure you are connected to the right host", "OK");
        }
        timer.Elapsed += Refresh;
        Refresh(null, null);
    }

	public async void Refresh(object sender, EventArgs e)
	{
		await NetworkManager.Send(Encoding.UTF8.GetBytes("RefreshPlease"), new CancellationTokenSource(1500).Token);

		Memory<byte> data = await NetworkManager.Receive(new CancellationTokenSource(1500).Token);

		string message = Encoding.UTF8.GetString(data.ToArray());
        int scanOrder = 0;
        string Accumulated = string.Empty;
        Info info = new();
        foreach (char C in message)
        {
            switch (C)
            {
                case '/':
                    scanOrder++;

                    switch (scanOrder)
                    {
                        case 0:
                            info.Pos = int.Parse(Accumulated);
                            Accumulated = string.Empty;
                            break;
                        case 1:
                            info.Current = int.Parse(Accumulated);
                            Accumulated = string.Empty;
                            break;
                    }
                    break;
            }
            Accumulated += C;
        }
    }
}