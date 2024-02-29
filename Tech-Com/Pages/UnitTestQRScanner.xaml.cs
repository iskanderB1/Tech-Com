using System.Net.Sockets;
using System.Text;

namespace Tech_Com.Pages;

public partial class UnitTestQRScanner : ContentPage
{
	TcpListener? server;
	List<TcpClient> clients = new();
	Task Task;
	int currentPos = 10;
	public UnitTestQRScanner()
	{
		InitializeComponent();
		server = new(7000);
		server.Start();
		Task = Task.Run(async () =>
		{
			while (true)
			{
				clients.Add(await server.AcceptTcpClientAsync(new CancellationTokenSource(2000).Token));
			}
		});
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
		Task.Run(async () =>
		{
			while(true)
			{
				await server.Server.SendAsync(new byte[] { ((byte)currentPos) });
			}	
		});
        Task.Run(async () =>
        {
			Thread.Sleep(Random.Shared.Next(1000, 4200));
			currentPos++;
        });
    }
    private void Test(object sender, EventArgs e)
	{
		if(clients.Count == 0)
		{
			DisplayAlert("Oh NOOOOOO!", "No clients were connected!", "ok");
			return;
		}
		foreach(var client in clients)
		{
			byte[] data = Encoding.UTF8.GetBytes("telecom");
			server.Server.SendAsync(data);
		}

	}
}